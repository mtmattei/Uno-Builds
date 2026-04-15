using Uno.Extensions.Reactive;

namespace Orbital.Presentation;

public partial record StudioModel(IStudioService Studio)
{
    public IFeed<StudioStatus> License => Feed.Async(Studio.GetStatusAsync);
    public IFeed<ImmutableList<StudioFeature>> Features => Feed.Async(Studio.GetFeaturesAsync);
    public IFeed<ImmutableList<McpConnector>> Connectors => Feed.Async(Studio.GetConnectorsAsync);

    public IFeed<string> LicenseTitle => License.Select(s =>
        s.IsValid ? $"Uno Platform Studio {s.Tier}" : "Uno Platform Studio");

    public IFeed<string> LicenseDetail => License.Select(s =>
        s.IsValid ? $"{s.AccountEmail} \u00B7 v{s.Version}" : $"{s.AccountName}@{Environment.MachineName}");

    public IFeed<string> LicenseBadge => License.Select(s =>
        s.IsValid ? "Active" : s.Tier);

    public IFeed<bool> LicenseIsValid => License.Select(s => s.IsValid);
}
