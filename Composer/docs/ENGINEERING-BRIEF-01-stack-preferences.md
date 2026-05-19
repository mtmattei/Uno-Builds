# Engineering Brief 01 — Stack Preferences Layer

**Status:** ready to implement
**Queue position:** 1 of 5 (shortest-win)
**Depends on:** none
**Unblocks:** Briefs 02, 03, 04, 05

## Goal

Add a new layer 0 — `Stack` — that captures the user's stack defaults before they begin composing. Every downstream MARKDOWN_GEN entry references these preferences so the produced briefs are stack-correct (MVUX vs MVVM, Skia vs Native, Material vs Cupertino, etc.) even without AI augmentation.

## Why this brief first

The single highest-leverage upgrade for output quality. Today every downstream brief is stack-agnostic — a generated Architecture brief just says "uses an app pattern" rather than "uses MVUX records with `IState`/`IFeed`." With this layer, the same generator can substitute the exact stack and produce stack-grounded output. No AI required; no other architectural risk.

Cost: small, additive only. Doesn't modify any existing layer's behavior — just inserts a new one before Intent.

## Scope

### In scope
- New `Stack` layer inserted at position 0 of `LAYERS`
- New `StackPreferencesCanvas` component with 7 captured fields
- New `STACK_DEFAULTS` + `STACK_OPTIONS` constants
- New `stackPrefs` state in the main component (+ snapshot, + reset wiring)
- New `MARKDOWN_GEN.stack(state)` entry
- Substitution from `s.stackPrefs` in at least 3 downstream MARKDOWN_GEN entries (`arch`, `interact`, `scaffold`)
- Locked context card summary for Stack
- RECAPS bridge sentence for `intent` referencing the just-locked stack
- ProgressIndicator total updated from 8 to 9
- CompositionStack renders 9 rows
- Files Rail file list shows `stack-preferences.md` first

### Out of scope
- AI augmentation (Brief 03)
- Direct manipulation in other canvases (separate work)
- Persistent stack-prefs across sessions (no persistence in v11)
- UI for "load my org's stack defaults" (future, when persistence lands)

## The 7 captured fields

| Field         | Type        | Default                                     | Allowed values                                              |
|---------------|-------------|---------------------------------------------|-------------------------------------------------------------|
| `pattern`     | enum string | `MVUX`                                      | `MVUX`, `MVVM`, `MVUX + Messaging`                          |
| `markup`      | enum string | `XAML`                                      | `XAML`, `C# Markup`                                         |
| `renderer`    | enum string | `Skia`                                      | `Skia`, `Native`                                            |
| `http`        | enum string | `Kiota`                                     | `Kiota`, `Refit`, `None`                                    |
| `nav`         | enum string | `Region`                                    | `Region`, `Frame`, `None`                                   |
| `theme`       | enum string | `Material`                                  | `Material`, `Fluent`, `Cupertino`, `Custom`                 |
| `platforms`   | string[]    | `['Wasm', 'iOS', 'Android', 'Windows']`     | subset of `['Wasm', 'iOS', 'Android', 'macOS', 'Windows', 'Linux']` |

These match Matt's canonical Uno conventions ("MVUX/Region/Material defaults" per userMemories).

## Prototype changes — `composer-context-engine.jsx`

The prototype is the executable spec. All design intent lands here first; the Uno port follows.

### Change 1 — new constants near top (after `INTENT_EXAMPLE`)

```javascript
// Stack defaults shown on first load. Single source of truth so both
// StackPreferencesCanvas and the main component agree on what counts as
// "still showing the default" vs "user has personalized it".
const STACK_DEFAULTS = {
  pattern:   'MVUX',
  markup:    'XAML',
  renderer:  'Skia',
  http:      'Kiota',
  nav:       'Region',
  theme:     'Material',
  platforms: ['Wasm', 'iOS', 'Android', 'Windows'],
};

// All allowable values per field. Single-select fields render as
// radio-row groups; `platforms` renders as multi-select chips.
const STACK_OPTIONS = {
  pattern:   ['MVUX', 'MVVM', 'MVUX + Messaging'],
  markup:    ['XAML', 'C# Markup'],
  renderer:  ['Skia', 'Native'],
  http:      ['Kiota', 'Refit', 'None'],
  nav:       ['Region', 'Frame', 'None'],
  theme:     ['Material', 'Fluent', 'Cupertino', 'Custom'],
  platforms: ['Wasm', 'iOS', 'Android', 'macOS', 'Windows', 'Linux'],
};
```

### Change 2 — `LAYERS` array prepended

```javascript
const LAYERS = [
  { id: 'stack',    label: 'Stack',          file: 'stack-preferences.md',   hint: "what we're building on" },
  { id: 'intent',   label: 'Intent',         file: 'README.md',              hint: 'what the app is for' },
  { id: 'ux',       label: 'UX',             file: 'ux-flows.md',            hint: 'how users move through it' },
  { id: 'arch',     label: 'Architecture',   file: 'architecture.md',        hint: 'how it is shaped' },
  { id: 'design',   label: 'Design System',  file: 'design-system.md',       hint: 'how it feels' },
  { id: 'interact', label: 'Interactions',   file: 'interaction-spec.md',    hint: 'every state of every flow' },
  { id: 'data',     label: 'Data',           file: 'data-contracts.md',      hint: 'shapes and contracts' },
  { id: 'impl',     label: 'Implementation', file: 'implementation-plan.md', hint: 'phased build plan' },
  { id: 'scaffold', label: 'Scaffold',       file: 'scaffold.command',       hint: 'runnable starting point' },
];
```

`LAYERS.length` is now 9. The `ProgressIndicator` already uses `LAYERS.length` so no change there. The `01 / 08` counter naturally becomes `01 / 09`.

### Change 3 — `RECAPS` entry

```javascript
const RECAPS = {
  stack:    null,
  intent:   "Stack chosen — now let's name what we're building on it.",
  ux:       "We've named what we're building — now let's trace how someone uses it.",
  arch:     "With the user's path mapped — let's figure out the shape underneath.",
  design:   'Modules in place — let\'s give the surface a feel.',
  interact: "The design's settled — now every state of every flow.",
  data:     'Interactions captured — let\'s nail down the shapes.',
  impl:     'Shapes locked in — let\'s plan how it gets built.',
  scaffold: "Eight layers locked. Here's what ships.",
};
```

`stack` has null (it's the first layer, no recap). `intent`'s recap is rewritten to reference the just-locked stack.

### Change 4 — `MARKDOWN_GEN.stack(state)`

Place at the top of `MARKDOWN_GEN`:

```javascript
stack: (s) => {
  const p = s.stackPrefs;
  const isMvux = p.pattern === 'MVUX' || p.pattern === 'MVUX + Messaging';
  return `# Stack Preferences

## Architecture

- **Pattern:** ${p.pattern}
- **Markup:** ${p.markup}
- **Renderer:** ${p.renderer}
- **HTTP client:** ${p.http}
- **Navigation:** ${p.nav}
- **Theme:** ${p.theme}

## Target platforms

${p.platforms.map((plat) => `- ${plat}`).join('\n')}

## What this means downstream

${isMvux ? `**MVUX is the chosen reactive pattern.** Models expose \`IFeed<T>\` and \`IState<T>\`; commands are public async methods invoked via the implicit \`IAsyncCommand\` binding. No \`INotifyPropertyChanged\`. No code-behind navigation calls.\n\n` : ''}${p.markup === 'XAML' ? `**XAML is the chosen markup.** All UI lives in \`.xaml\` files with code-behind kept thin. Bindings prefer \`Mode=TwoWay\` for state updates.\n\n` : `**C# Markup is the chosen markup.** All UI lives in fluent C# expressions. No \`.xaml\` files.\n\n`}${p.nav === 'Region' ? `**Region-based navigation is the chosen approach.** Use \`uen:Region.Attached\` and \`uen:Region.Name\` on container elements; bind \`Navigation.Request\` declaratively. Never invoke \`INavigator\` from code-behind.\n\n` : p.nav === 'Frame' ? `**Frame navigation is the chosen approach.** Standard \`Frame.Navigate\` pattern from code-behind.\n\n` : ''}${p.renderer === 'Skia' ? `**Skia renderer is the chosen rendering backend.** All Uno Toolkit controls render consistently across iOS, Android, macOS, Linux, Windows, and WebAssembly.\n\n` : `**Native renderer is the chosen rendering backend.** Each platform renders via its native UI primitives.\n\n`}${p.theme === 'Material' ? `**Material theme is the chosen design system.** Resources resolve through Material brushes (\`PrimaryBrush\`, \`SecondaryBrush\`, etc.). Prefer Material if both Material and Fluent are present.\n\n` : p.theme === 'Cupertino' ? `**Cupertino theme is the chosen design system.** iOS-style controls with Cupertino brushes.\n\n` : p.theme === 'Fluent' ? `**Fluent theme is the chosen design system.** WinUI-style controls with Fluent brushes.\n\n` : `**Custom theme.** No pre-shipped theme resources — fully bespoke design system.\n\n`}${p.http === 'Kiota' ? `**Kiota is the chosen HTTP client.** Typed client generation from OpenAPI definitions. Use \`AddKiotaClient<T>()\` in DI.\n\n` : p.http === 'Refit' ? `**Refit is the chosen HTTP client.** Interface-based typed clients.\n\n` : `**No HTTP client.** App is local-only / offline-first.\n\n`}
## Agent prompt

> When generating downstream code, every architectural decision must match
> these preferences exactly. Reference patterns by their canonical Uno
> names. If a recommended pattern is at odds with these preferences
> (e.g. \`MVVM\` when the user chose \`MVUX\`), flag it and stop generating
> rather than silently substituting.
`;
},
```

This entry alone produces ~50-80 lines of structured stack documentation. Combined with downstream substitution it transforms every other layer's output.

### Change 5 — downstream substitution

At minimum, update `arch`, `interact`, and `scaffold` MARKDOWN_GEN entries. Pattern is the same in each — destructure stackPrefs at top and substitute into the template.

**`arch` (replacement):**
```javascript
arch: (s) => {
  const ctx = deriveContext(s.intent);
  const p = s.stackPrefs;
  const isMvux = p.pattern === 'MVUX' || p.pattern === 'MVUX + Messaging';
  const stateLayer = isMvux ? 'State (MVUX)' : 'ViewModels';
  const stateContent = isMvux ? 'Feeds, States, Selection' : 'INPC-backed properties + ICommand';
  return `# Architecture

## Pattern

${p.pattern}${isMvux ? ' over MVVM. Reactive feeds with offline-first state.' : '. Standard property-change notification with ICommand.'}

## Modules

- **Pages** — Route surfaces${p.nav === 'Region' ? ' via \`uen:Region.Attached\`' : ''}
- **Navigation** — ${p.nav === 'Region' ? 'Region-based routes' : p.nav === 'Frame' ? 'Frame-based navigation' : 'No nav extensions'}
- **${stateLayer}** — ${stateContent}
- **Services** — ${ctx.entityTitle}, ${ctx.userNoun.charAt(0).toUpperCase() + ctx.userNoun.slice(1)}, Schedule
${ctx.isOfflineFirst || p.http === 'None' ? '' : `- **HTTP (${p.http})** — ${p.http === 'Kiota' ? 'Typed clients, generated from OpenAPI' : 'Refit-style typed clients'}\n`}- **Storage** — Local cache${ctx.isOfflineFirst ? ', offline-first' : ''}

## Why this fits

${isMvux ? 'MVUX gives both views identical reactive feeds with offline-first state — no MVVM ceremony, no Rx plumbing.' : 'MVVM provides familiar two-way binding via INotifyPropertyChanged.'} ${p.nav === 'Region' ? 'Navigation regions let Shell and TabBar animate independently, which the dispatch flow in §02 depends on.' : ''}

## Agent prompt

> When generating XAML, use ${p.nav === 'Region' ? '\`uen:Region.Attached\` for navigation surfaces' : 'Frame.Navigate for navigation'} and bind \`ItemsSource\` to ${isMvux ? 'feed properties (an IFeed)' : 'ObservableCollection properties'}. Never construct ${isMvux ? 'Models' : 'ViewModels'} in code-behind.
`;
}
```

**`interact` (minimal update — substitute VSM contract reference):**
```javascript
interact: (s) => {
  const p = s.stackPrefs;
  return `# Interaction Spec

## States

Six states across three flows.

[... existing content unchanged ...]

## VSM contract

${p.markup === 'XAML'
  ? `Generated XAML must use \`utu:VisualStateManager.States\` (Uno.Toolkit) bound to an \`IState<string>\` named \`CurrentState\`. VSM group names follow \`{FlowIdPascalCased}StateGroup\`; state names match the StateKind enum verbatim.`
  : `Generated C# Markup must use \`VisualStateManager.States\` programmatically with \`CurrentState\` state binding.`}
`;
}
```

**`scaffold` (replacement command builder):**
```javascript
scaffold: (s) => {
  const p = s.stackPrefs;
  const appName = (s.intent.appType || 'App').replace(/[^a-zA-Z0-9]/g, '');
  const platforms = p.platforms.map((x) => x.toLowerCase()).join(',');
  const features = [
    'config', 'http', 'logging', 'nav',
    p.pattern === 'MVUX' || p.pattern === 'MVUX + Messaging' ? 'mvux' : 'mvvm',
  ].join(',');
  return `# Scaffold

## Command

\`\`\`bash
dotnet new unoapp \\
  -n ${appName} \\
  --tfm net10.0 \\
  --platforms ${platforms} \\
  --markup ${p.markup === 'XAML' ? 'xaml' : 'csharp'} \\
  --presentation ${p.pattern === 'MVUX' || p.pattern === 'MVUX + Messaging' ? 'mvux' : 'mvvm'} \\
  --theme ${p.theme.toLowerCase()} \\
  --features ${features}
\`\`\`
`;
}
```

The `ScaffoldCanvas` component's inline command builder should be updated to use the same substitution rules — extract a `buildScaffoldCommand(intent, stackPrefs)` helper and call it from both places.

### Change 6 — new `StackPreferencesCanvas` component

Place after `IntentCanvas` in the JSX, before `UXCanvas`:

```javascript
function StackPreferencesCanvas({ stackPrefs, setStackPrefs, onDirty, layerState }) {
  const isPreviewing = layerState === 'previewing';
  // Single-select fields render as radio rows; platforms renders as multi-select chips.
  const singleSelect = [
    { key: 'pattern',  label: 'Pattern' },
    { key: 'markup',   label: 'Markup' },
    { key: 'renderer', label: 'Renderer' },
    { key: 'http',     label: 'HTTP' },
    { key: 'nav',      label: 'Navigation' },
    { key: 'theme',    label: 'Theme' },
  ];

  const updateField = (key, value) => {
    setStackPrefs({ ...stackPrefs, [key]: value });
    onDirty();
  };

  const togglePlatform = (plat) => {
    const current = stackPrefs.platforms;
    const next = current.includes(plat)
      ? current.filter((x) => x !== plat)
      : [...current, plat];
    setStackPrefs({ ...stackPrefs, platforms: next });
    onDirty();
  };

  return (
    <div>
      <SectionHeader
        filename="stack-preferences.md"
        badge={isPreviewing ? 'PREVIEW' : 'EDITING'}
        badgeColor={isPreviewing ? C.amber : C.ink3}
      />
      <div>
        {singleSelect.map((f, i) => (
          <div key={f.key} style={{
            display: 'grid', gridTemplateColumns: '120px 1fr',
            gap: 16, alignItems: 'center', padding: '12px 0',
            borderBottom: `1px solid ${C.hairline2}`,
          }}>
            <Eyebrow size={10} color={C.ink3} weight={500}>{f.label}</Eyebrow>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
              {STACK_OPTIONS[f.key].map((opt) => {
                const isActive = stackPrefs[f.key] === opt;
                return (
                  <button key={opt}
                    onClick={() => !isPreviewing && updateField(f.key, opt)}
                    disabled={isPreviewing}
                    style={{
                      fontFamily: SANS, fontSize: 13, fontWeight: 500,
                      padding: '5px 12px',
                      background: isActive ? C.ink : 'transparent',
                      color: isActive ? C.paper : C.ink2,
                      border: `1px solid ${isActive ? C.ink : C.hairline}`,
                      borderRadius: 5, cursor: isPreviewing ? 'default' : 'pointer',
                    }}>
                    {opt}
                  </button>
                );
              })}
            </div>
          </div>
        ))}
        <div style={{
          display: 'grid', gridTemplateColumns: '120px 1fr',
          gap: 16, alignItems: 'flex-start', padding: '12px 0',
        }}>
          <Eyebrow size={10} color={C.ink3} weight={500}>Platforms</Eyebrow>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
            {STACK_OPTIONS.platforms.map((plat) => {
              const isActive = stackPrefs.platforms.includes(plat);
              return (
                <button key={plat}
                  onClick={() => !isPreviewing && togglePlatform(plat)}
                  disabled={isPreviewing}
                  style={{
                    fontFamily: SANS, fontSize: 13, fontWeight: 500,
                    padding: '5px 12px',
                    background: isActive ? C.ink : 'transparent',
                    color: isActive ? C.paper : C.ink3,
                    border: `1px solid ${isActive ? C.ink : C.hairline}`,
                    borderRadius: 5, cursor: isPreviewing ? 'default' : 'pointer',
                    display: 'inline-flex', alignItems: 'center', gap: 5,
                  }}>
                  {isActive && <span style={{ fontSize: 10 }}>✓</span>}
                  {plat}
                </button>
              );
            })}
          </div>
        </div>
      </div>
      <Annotation label="Why these defaults">
        MVUX over MVVM means reactive feeds with offline-first state — no MVVM
        ceremony, no Rx plumbing. Region-based navigation lets Shell and TabBar
        animate independently. Material is the canonical Uno theme. Skia renderer
        is consistent across all six platforms. Kiota gives typed HTTP clients
        generated from OpenAPI. These defaults match recommended Uno conventions.
      </Annotation>
      <Annotation label="Agent prompt" voice="quote">
        These stack choices are load-bearing for every downstream brief. If a
        layer's generator references a pattern at odds with these preferences,
        stop and flag rather than silently substitute.
      </Annotation>
    </div>
  );
}
```

### Change 7 — main component state wiring

In `export default function CompositionEngine()`:

```javascript
const [stackPrefs, setStackPrefs] = useState({ ...STACK_DEFAULTS });
```

In the existing `markDirty` snapshot capture, include stackPrefs:
```javascript
setSnapshots((p) => ({
  ...p,
  [currentLayer.id]: {
    intent: { ...intent },
    design: { ...design },
    stackPrefs: { ...stackPrefs },
  },
}));
```

In `discardPreview` and `discardEdits`, restore stackPrefs from snapshot.

In `acceptPreview`, if `previewValues.stackPrefs` is present, adopt it (same as intent/design pattern).

In `Reset`, add:
```javascript
setStackPrefs({ ...STACK_DEFAULTS });
```

### Change 8 — `renderCanvas` adds Stack branch

At the top of `renderCanvas`, before the `intent` branch:

```javascript
if (layer.id === 'stack') return (<>
  <ActiveLayerHeader index={activeIndex} layer={layer} layerState={layerState}
    title="What are we building on?"
    subtitle="Pattern, markup, renderer, navigation, theme, platforms. Every downstream brief references this." />
  <StackPreferencesCanvas stackPrefs={stackPrefs} setStackPrefs={setStackPrefs}
    onDirty={markDirty} layerState={layerState} />
  {footer({
    leadQuestion: 'These defaults match canonical Uno conventions. Anything you want to change before locking?',
    suggestions: ['MVVM instead', 'Add macOS + Linux', 'Custom theme'],
  })}
</>);
```

### Change 9 — locked context card summary

In `lockedCards` `useMemo`:

```javascript
if (layer.id === 'stack') {
  summary = `${stackPrefs.pattern}, ${stackPrefs.theme} theme, ${stackPrefs.platforms.length} platforms.`;
  contents = [
    ['Pattern', stackPrefs.pattern],
    ['Markup', stackPrefs.markup],
    ['Renderer', stackPrefs.renderer],
    ['Theme', stackPrefs.theme],
  ];
}
```

Add `stackPrefs` to the `useMemo` dependency array.

### Change 10 — `FilesRail` and `ScaffoldCanvas` receive `stackPrefs`

`FilesRail` invocation:
```javascript
<FilesRail
  ...
  stackPrefs={stackPrefs}
  ...
/>
```

Inside `FilesRail`, add `stackPrefs` to the destructured props and the `canvasState`:
```javascript
function FilesRail({ activeIndex, lockedIds, visible, intent, design, stackPrefs, overrides, setOverride, onDirty }) {
  // ...
  const canvasState = { intent, design, stackPrefs };
  // ...
}
```

`ScaffoldCanvas` invocation:
```javascript
<ScaffoldCanvas intent={intent} design={design} stackPrefs={stackPrefs} overrides={overrideMarkdown} onShip={lockAndContinue} />
```

Inside `ScaffoldCanvas`, destructure stackPrefs and use it in `buildPromptContext` / `buildScaffoldCommand`.

### Change 11 — version comment bumped to v12

```javascript
// ─── Composition Engine v12 — Stack Preferences Layer ─────────────────
// Built on v11's UX walkthrough fixes. v12 adds a Stack Preferences
// layer at position 0 that captures the user's stack defaults (pattern,
// markup, renderer, HTTP, navigation, theme, platforms). Every downstream
// MARKDOWN_GEN entry substitutes from these preferences to produce
// stack-correct briefs. ProgressIndicator total is now 9.
//
// Unblocks Brief 02 (structured LayerBrief generators) and Brief 03
// (AI-augmented preview generation).
```

## Uno port changes — for the production XAML implementation

When this lands in the Uno project, mirror the React changes with these MVUX-style structures:

### New records and enums in `Models/`

```csharp
public enum StackPattern   { Mvux, Mvvm, MvuxMessaging }
public enum MarkupKind     { Xaml, CSharpMarkup }
public enum RendererKind   { Skia, Native }
public enum HttpClientKind { Kiota, Refit, None }
public enum NavigationKind { Region, Frame, None }
public enum ThemeKind      { Material, Fluent, Cupertino, Custom }
public enum PlatformTarget { Wasm, iOS, Android, MacOS, Windows, Linux }

public record StackPreferences(
    StackPattern Pattern,
    MarkupKind Markup,
    RendererKind Renderer,
    HttpClientKind Http,
    NavigationKind Nav,
    ThemeKind Theme,
    ImmutableHashSet<PlatformTarget> Platforms);
```

### New `StackPreferencesModel` in `Presentation/`

```csharp
public partial record StackPreferencesModel(ShellModel Shell)
{
    public static readonly StackPreferences Defaults = new(
        Pattern:   StackPattern.Mvux,
        Markup:    MarkupKind.Xaml,
        Renderer:  RendererKind.Skia,
        Http:      HttpClientKind.Kiota,
        Nav:       NavigationKind.Region,
        Theme:     ThemeKind.Material,
        Platforms: ImmutableHashSet.Create(
            PlatformTarget.Wasm, PlatformTarget.iOS,
            PlatformTarget.Android, PlatformTarget.Windows));

    public IState<StackPreferences> Values => State.Value(this, () => Defaults);

    public async ValueTask UpdatePattern(StackPattern value)
    {
        var current = await Values;
        await Values.SetAsync(current with { Pattern = value });
        await Shell.MarkDirty(LayerKind.Stack);
    }

    public async ValueTask TogglePlatform(PlatformTarget value)
    {
        var current = await Values;
        var next = current.Platforms.Contains(value)
            ? current.Platforms.Remove(value)
            : current.Platforms.Add(value);
        await Values.SetAsync(current with { Platforms = next });
        await Shell.MarkDirty(LayerKind.Stack);
    }
    // ... similar UpdateXxx methods for Markup, Renderer, Http, Nav, Theme
}
```

### `LayerKind` enum update

```csharp
public enum LayerKind
{
    Stack          = 0,   // new — insert at top
    Intent         = 1,
    UX             = 2,
    Architecture   = 3,
    DesignSystem   = 4,
    Interactions   = 5,
    Data           = 6,
    Implementation = 7,
    Scaffold       = 8,
}
```

Every reference to `LayerKind.Intent == 0` in the codebase needs to be reviewed — should still pass through `Layers.All[0]` rather than hardcoded ordinals.

### `Layers.All` update

Add Stack as the first entry:

```csharp
public static readonly ImmutableArray<LayerDef> All = ImmutableArray.Create(
    new LayerDef(LayerKind.Stack, "Stack", "stack-preferences.md", "what we're building on", "Stack"),
    new LayerDef(LayerKind.Intent, "Intent", "README.md", "what the app is for", "Intent"),
    // ... existing entries
);
```

### New page + canvas

- `Views/Pages/StackPreferencesPage.xaml` — follows the existing page pattern (ScrollViewer + AutoLayout)
- `Views/Canvases/StackPreferencesCanvas.xaml` — tabular grid with `120,*` columns, segmented buttons for single-select, chips for platforms

### Route registration

In `Navigation/RouteMap.cs`, add the Stack route as `IsDefault: true` (the new initial layer):

```csharp
new RouteMap("Stack", View: views.FindByViewModel<StackPreferencesModel>(), IsDefault: true),
new RouteMap("Intent", View: views.FindByViewModel<IntentModel>()),
// ...
```

### DI registration in `App.xaml.cs`

```csharp
services.AddTransient<StackPreferencesModel>();
```

## Acceptance criteria

After implementing this brief, every box ticks:

### Constants and structure
- [ ] `STACK_DEFAULTS` constant defined with all 7 fields
- [ ] `STACK_OPTIONS` constant defined with allowed values per field
- [ ] `LAYERS` array starts with `{ id: 'stack', label: 'Stack', file: 'stack-preferences.md', hint: "what we're building on" }`
- [ ] `LAYERS.length === 9`
- [ ] `RECAPS.stack === null`
- [ ] `RECAPS.intent` references the stack ("Stack chosen — now let's name what we're building on it.")

### Canvas
- [ ] `StackPreferencesCanvas` component renders 6 single-select rows + 1 multi-select platforms row
- [ ] Single-select buttons use ink/paper for active, transparent/ink2 for inactive
- [ ] Platforms chips show `✓` glyph when active
- [ ] Click on any segment marks layer Dirty
- [ ] Annotations render (Why these defaults + Agent prompt)

### State wiring
- [ ] `stackPrefs` state initialized to `{ ...STACK_DEFAULTS }`
- [ ] Snapshot captures stackPrefs on first Clean → Dirty transition
- [ ] Discard preview / edits restores stackPrefs from snapshot
- [ ] Accept preview adopts `previewValues.stackPrefs` if present
- [ ] Reset sets stackPrefs back to defaults

### Generators
- [ ] `MARKDOWN_GEN.stack(state)` returns ≥30 lines of structured stack documentation
- [ ] `MARKDOWN_GEN.arch(state)` substitutes pattern, navigation, HTTP, and state-layer name from `s.stackPrefs`
- [ ] `MARKDOWN_GEN.interact(state)` references VSM contract based on `markup` choice
- [ ] `MARKDOWN_GEN.scaffold(state)` builds the dotnet new command with correct `--platforms`, `--presentation`, `--theme`, `--features` flags from stackPrefs
- [ ] `ScaffoldCanvas`'s inline command builder uses the same `buildScaffoldCommand(intent, stackPrefs)` helper

### Workspace shell
- [ ] CompositionStack renders 9 rows in order (Stack first)
- [ ] ProgressIndicator counter reads `01 / 09` on Stack, `09 / 09` on Scaffold
- [ ] First-launch focused screen renders StackPreferencesCanvas (not IntentCanvas)
- [ ] Locked Stack context card summary reads `{pattern}, {theme} theme, {N} platforms.`
- [ ] Locked Stack contents show 4 facts: Pattern, Markup, Renderer, Theme

### Files Rail
- [ ] `stack-preferences.md` is the first row of the file list
- [ ] Markdown preview renders the synthesized stack document
- [ ] `MarkdownPreview` correctly renders the bulleted lists and bold inline markers

### Smoke tests
- [ ] Land on Stack, change Pattern to MVVM, advance through to Architecture — generated arch.md uses "ViewModels" and "INPC-backed properties" instead of "State (MVUX)" and "Feeds"
- [ ] Land on Stack, add Linux to Platforms, advance to Scaffold — scaffold command's `--platforms` flag includes `linux`
- [ ] Land on Stack, switch to C# Markup, advance to Interactions — interaction-spec.md mentions programmatic VSM API instead of `utu:VisualStateManager.States`
- [ ] Reset returns Stack to defaults
- [ ] Revisit Stack from Architecture preserves the user's prior selections (not example defaults)

## Estimated effort

| Task                                                              | Hours    |
|-------------------------------------------------------------------|----------|
| Constants + LAYERS update + RECAPS update                         | 1        |
| `MARKDOWN_GEN.stack(state)` entry                                 | 2        |
| Downstream substitution in `arch`, `interact`, `scaffold`         | 3        |
| `StackPreferencesCanvas` component                                | 3        |
| `renderCanvas` branch + state wiring + snapshot/discard/reset     | 2        |
| Files Rail + ScaffoldCanvas prop threading                        | 1        |
| Locked context card summary                                       | 0.5      |
| Manual smoke testing through all 9 layers (3 stack permutations)  | 2        |
| Version comment bump                                              | 0.5      |
| **Prototype total**                                               | **~15h** |
| Records + enums + model class                                     | 2        |
| Page + canvas XAML                                                | 4        |
| Route registration + DI                                           | 1        |
| LayerKind enum + Layers.All ordinal cascade                       | 2        |
| Snapshot record additions                                         | 1        |
| **Uno port total**                                                | **~10h** |

Roughly 2 days prototype, 1.5 days Uno port. ~3.5 person-days total.

## What this unblocks

After Brief 01 ships, the next briefs become tractable:

- **Brief 02 — Structured LayerBrief generators:** Can now produce stack-correct templated outputs (~100-300 lines per layer) without AI. The substitution pattern from this brief generalizes — every section template references `state.stackPrefs` alongside intent/design.
- **Brief 03 — `ClaudeLayerPreviewService`:** Gets stack context as a top-level prompt input. Without Brief 01, every AI call would either ignore stack (producing generic output) or guess (producing inconsistent output).
- **Brief 04 — Self-review:** Acceptance criteria per layer can include "every code sample matches the user's stack" as a checkable item.
- **Brief 05 — Cross-brief consistency:** The stack glossary becomes the canonical reference all briefs measure terminology against.

## Notes for the implementer

- The prototype is the executable spec. Land changes there first, run the smoke tests, then port to Uno.
- File integrity discipline: after each major edit, run `awk 'BEGIN{b=0;p=0;br=0} {for(i=1;i<=length($0);i++){c=substr($0,i,1); if(c=="{")b++; if(c=="}")b--; if(c=="(")p++; if(c==")")p--; if(c=="[")br++; if(c=="]")br--}} END{print "balance:", b, p, br}' composer-context-engine.jsx` to confirm 0/0/0 brace balance before continuing.
- The `MARKDOWN_GEN.stack` template uses nested template literals — be careful with backticks inside backticks. Escape via `\`` if needed.
- The single-select segmented buttons use ink/paper coloring (matching the existing flow-tab pattern in InteractionsCanvas). Don't introduce a new color scheme.
- Annotation primitive is reused — no new editorial primitive needed.

---

## End of brief

Ready to implement. After landing, request Brief 02.
