using System.Diagnostics.CodeAnalysis;
using Uno.Resizetizer;

namespace Composer;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    public static IHost? Host { get; private set; }
    protected Window? MainWindow { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)
                        .CoreLogLevel(LogLevel.Warning);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                        .Section<Composer.Services.AnthropicConfig>("Anthropic"))
#if DEBUG
                // User secrets layer for the Anthropic API key — set with:
                //   dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
                // Layered AFTER UseConfiguration so secret values win over the
                // empty default in the embedded appsettings.json.
                //
                // Using the literal userSecretsId overload (instead of the
                // generic AddUserSecrets<App>) so we don't depend on Uno's
                // build pipeline generating a [UserSecretsId] assembly
                // attribute from the csproj prop. The literal must match
                // <UserSecretsId> in Composer.csproj.
                .ConfigureAppConfiguration((ctx, cb) =>
                {
                    if (ctx.HostingEnvironment.IsDevelopment())
                        cb.AddUserSecrets(userSecretsId: "composer-anthropic-2026");
                })
#endif
                .UseLocalization()
                .UseHttp((context, services) =>
                {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                    // Refit client for the Anthropic Messages API. The endpoint
                    // base URL is sourced from configuration so a proxy can be
                    // injected without rebuilding (production should use a proxy
                    // to keep the API key out of the WASM bundle).
                    services.AddRefitClient<Composer.Services.IAnthropicClient>(
                        context,
                        configure: (builder, _) => builder.ConfigureHttpClient(c =>
                        {
                            var baseUrl = context.Configuration["Anthropic:BaseUrl"] ?? "https://api.anthropic.com";
                            c.BaseAddress = new Uri(baseUrl);
                            // Bound the HTTP wait so a stalled Anthropic call surfaces as
                            // TaskCanceledException instead of locking the conversation
                            // in IsThinking=true forever.
                            c.Timeout = TimeSpan.FromSeconds(60);
                        }));
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<Composer.Services.IBundleExporter, Composer.Services.BundleExporter>();
                    // Typed HttpClient for the Uno.Sdk version chip. Singleton service.
                    services.AddHttpClient<Composer.Services.IUnoSdkVersionService, Composer.Services.UnoSdkVersionService>(c =>
                    {
                        c.Timeout = TimeSpan.FromSeconds(8);
                    });
                    // Layer preview service — Anthropic-backed with identity
                    // fallback when the API key is empty. See brief 01 §2.
                    services.AddSingleton<Composer.Services.ILayerPreviewService, Composer.Services.LayerPreviewService>();
                    // Pure-function context deriver — 13 regex rules over Intent
                    // values → DerivedContext. Used by downstream layer models
                    // to keep generated content stack/domain-correct.
                    // Brief §10.2.
                    services.AddSingleton<Composer.Services.IContextDeriver, Composer.Services.ContextDeriver>();
                    // ShellModel + per-layer models — each holds its layer's
                    // canonical IState. ComposerModel takes all 8 layer models
                    // as constructor deps and re-exposes their states as
                    // pass-throughs so existing canvas reflection still works.
                    // Singletons so the same instance backs every binding.
                    services.AddSingleton<Composer.Models.Presentation.ShellModel>();
                    services.AddSingleton<Composer.Models.Presentation.IntentModel>();
                    services.AddSingleton<Composer.Models.Presentation.UXModel>();
                    services.AddSingleton<Composer.Models.Presentation.ArchitectureModel>();
                    services.AddSingleton<Composer.Models.Presentation.DesignModel>();
                    services.AddSingleton<Composer.Models.Presentation.InteractionsModel>();
                    services.AddSingleton<Composer.Models.Presentation.DataModel>();
                    services.AddSingleton<Composer.Models.Presentation.ImplementationModel>();
                    services.AddSingleton<Composer.Models.ComposerModel>();

#if DEBUG
                    // PostConfigure bridge: Uno's UseConfiguration chain doesn't
                    // reliably compose with the standard
                    // ConfigureAppConfiguration/AddUserSecrets layer, so the
                    // bound AnthropicConfig.ApiKey may stay empty even when
                    // `dotnet user-secrets list` shows the key. Read the
                    // secrets file directly here and stamp it into the bound
                    // options if the section value is empty.
                    services.PostConfigure<Composer.Services.AnthropicConfig>(cfg =>
                    {
                        if (!string.IsNullOrEmpty(cfg.ApiKey)) return;
                        var fromSecrets = LoadAnthropicKeyFromUserSecrets("composer-anthropic-2026");
                        if (!string.IsNullOrEmpty(fromSecrets))
                            cfg.ApiKey = fromSecrets!;
                    });
#endif
                }));
                // M3b's region-based navigation was reverted to inline canvas
                // hosting (see ActiveCanvas.SyncSlot). The UseNavigation call
                // + RouteMap registration are dropped — they're dead weight
                // now that the framework never routes a page in.

        MainWindow = builder.Window;

        // NOTE: MainWindow.UseStudio() removed — its DevServer connection
        // probe is synchronous on the UI thread, and when no dev server is
        // running it blocks for ~10-30s of retries (logged as "DevServer
        // isn't able to connect"). That manifests as a frozen window where
        // even the title-bar close button doesn't respond. Re-enable only
        // when actually running with `uno-platform dev-server` started.

        MainWindow.SetWindowIcon();

        // Default window size — full layout (header + 2-column body) needs ~1280×900.
        try
        {
            MainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1320, Height = 920 });
        }
        catch { /* not all platforms support AppWindow.Resize */ }

        Host = builder.Build();

#if DEBUG
        // Startup diagnostic — print whether the Anthropic key was wired so
        // the contextual-fetch silent fallback is visible from the terminal.
        // Length only, never the value.
        try
        {
            var opts = Host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<Composer.Services.AnthropicConfig>>();
            var len = opts.Value.ApiKey?.Length ?? 0;
            System.Console.WriteLine($"[Composer] Anthropic API key configured: {(len > 0 ? "yes" : "NO (empty)")}, length={len}");
        }
        catch { /* diagnostic best-effort */ }
#endif

        // Per docs/ARCHITECTURE-BRIEF-from-scratch.md §4.2 / §8: Shell is the
        // single root content. Uno.Extensions.Navigation discovers the
        // ActivePage region inside the Shell's tree and auto-navigates to the
        // IsDefault route (Stack). The previous Frame.Navigate(typeof(Shell))
        // bootstrap is dropped — it collided with region routing.
        MainWindow.Content = new Shell();
        MainWindow.Activate();
    }

#if DEBUG
    /// <summary>
    /// Reads <c>%APPDATA%\Microsoft\UserSecrets\{id}\secrets.json</c> directly
    /// and returns the <c>Anthropic:ApiKey</c> value if present. This mirrors
    /// what <c>Microsoft.Extensions.Configuration.UserSecrets</c> does
    /// internally, but is invoked from a PostConfigure hook so it survives
    /// Uno's UseConfiguration chain not propagating standard user-secrets
    /// sources into <c>IOptions&lt;AnthropicConfig&gt;</c>.
    /// </summary>
    private static string? LoadAnthropicKeyFromUserSecrets(string userSecretsId)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = System.IO.Path.Combine(appData, "Microsoft", "UserSecrets", userSecretsId, "secrets.json");
            if (!System.IO.File.Exists(path)) return null;

            using var doc = System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(path));
            // user-secrets stores hierarchical keys flat with ":" as separator,
            // e.g. "Anthropic:ApiKey". Try both shapes for robustness.
            if (doc.RootElement.TryGetProperty("Anthropic:ApiKey", out var flat)
                && flat.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                return flat.GetString();
            }
            if (doc.RootElement.TryGetProperty("Anthropic", out var nested)
                && nested.ValueKind == System.Text.Json.JsonValueKind.Object
                && nested.TryGetProperty("ApiKey", out var key)
                && key.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                return key.GetString();
            }
        }
        catch
        {
            // Best-effort — fall through to empty so the banner stays visible.
        }
        return null;
    }
#endif
}
