# AdTokens IDE - Implementation Brief

## Project Overview

**AdTokens IDE** (also known as "SponsoredAI" or "FreeTierIDE") is a satirical Uno Platform application that simulates an ad-supported AI chat assistant embedded in a fake IDE shell. The core joke: while the AI is "thinking," users are subjected to rotating advertisements - implying the ads are funding each token of the response.

---

## UI Analysis (Based on Mockup)

The mockup reveals a dark-themed IDE with the following key components:

### 1. Title Bar
- macOS-style window controls (red/yellow/green dots)
- Centered title: "AdTokens IDE - SponsoredAI.sln"
- Dark background (#1e1e1e typical)

### 2. Menu Bar
- Items: **File**, **Edit**, **View**, **Build**, **AI Chat**, **Help**
- Non-functional or joke options (satirical)

### 3. Solution Explorer (Left Sidebar)
- TreeView-style hierarchy showing:
  ```
  SponsoredAI
  ├── Controls
  │   ├── ChatPanel.xaml
  │   ├── ChatPanel.xaml.cs
  │   └── AdCarousel.xaml
  ├── ViewModels
  │   ├── ChatViewModel.cs
  │   └── AdViewModel.cs
  ├── Services
  │   └── FakeAIService.cs
  ├── MainPage.xaml
  └── appsettings.json
  ```
- Icons for folders (yellow) and C# files (green)

### 4. Chat Panel (Main Content Area)
- Header with AI avatar, "AI Assistant" label, "Powered by ads • Free tier"
- Token counter in top-right: "1,247 tokens used" (yellow/gold color)
- User messages: Right-aligned, purple/violet background, rounded corners
- AI messages: Left-aligned, dark gray background (#2d2d2d), rounded corners
- Sponsor badge on AI messages: "Sponsored by NullPointer Coffee • 2:34 PM"
- Typing indicator with animated dots: "Generating response..."

### 5. Ad Carousel (The Star Feature)
- Appears during AI "thinking" state
- Header: "Your response is sponsored by" with progress bar and "Ad 1/3"
- Ad card with:
  - Gradient icon/logo placeholder
  - Product name: "NullPointer Coffee™"
  - Tagline: "The only coffee strong enough to debug your code at 3 AM..."
  - Disclaimer: "*Results may vary. Coffee cannot fix your code."
  - Token sponsorship: "This ad sponsors ~50 tokens"
  - "Learn More" button
- Rotates every 2-3 seconds

### 6. Input Area
- Placeholder: "Ask the AI anything... (sponsored response)"
- Send button (purple)

### 7. Status Bar (Bottom)
- "Ready" status with green indicator
- "AI: Connected" with yellow indicator
- "Ads: Active"
- "Free Tier • 5,000 tokens" limit display
- Shimmering "GO PREMIUM" button (gold/yellow gradient)
- Line/Column indicator: "Ln 42"

---

## Technical Architecture

### Project Structure (Uno Platform)

```
AdTokensIDE/
├── AdTokensIDE.csproj
├── App.xaml
├── App.xaml.cs
├── GlobalUsings.cs
├── Strings/
│   └── en/
│       └── Resources.resw
├── Assets/
│   ├── Icons/
│   │   ├── file-csharp.png
│   │   ├── file-xaml.png
│   │   ├── folder.png
│   │   └── folder-open.png
│   └── Ads/
│       ├── nullpointer-coffee.png
│       ├── ergocode-chair.png
│       ├── brainstack-supplements.png
│       └── cloudless-hosting.png
├── Models/
│   ├── ChatMessage.cs
│   ├── Advertisement.cs
│   ├── SolutionItem.cs
│   └── TokenUsage.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── ChatViewModel.cs
│   ├── AdCarouselViewModel.cs
│   └── SolutionExplorerViewModel.cs
├── Services/
│   ├── IAIService.cs
│   ├── ClaudeAIService.cs
│   ├── IAdRotationService.cs
│   └── AdRotationService.cs
├── Controls/
│   ├── FakeMenuBar.xaml / .xaml.cs
│   ├── SolutionExplorer.xaml / .xaml.cs
│   ├── ChatPanel.xaml / .xaml.cs
│   ├── ChatMessageControl.xaml / .xaml.cs
│   ├── AdCarousel.xaml / .xaml.cs
│   ├── AdCard.xaml / .xaml.cs
│   ├── TypingIndicator.xaml / .xaml.cs
│   └── StatusBar.xaml / .xaml.cs
├── Themes/
│   ├── IDEDarkTheme.xaml
│   └── ColorPaletteOverride.xaml
└── MainPage.xaml / .xaml.cs
```

### Recommended UnoFeatures (csproj)

```xml
<UnoFeatures>
  Toolkit;
  Material;
  Mvvm;
  ThemeService;
  Http;
  Configuration;
</UnoFeatures>
```

**Additional NuGet Packages**:
- `Anthropic.SDK` - Official Claude API client (or use raw HttpClient)

### Target Frameworks

```xml
<TargetFrameworks>
  net8.0-windows10.0.22621;
  net8.0-browserwasm;
  net8.0-desktop;
</TargetFrameworks>
```

Primary targets:
- **Windows (WinUI 3)** - Primary development
- **WebAssembly** - Easy sharing/demos
- **Desktop (Skia)** - macOS, Linux support

---

## Component Specifications

### 1. Models

#### ChatMessage.cs
```csharp
public record ChatMessage(
    string Id,
    string Content,
    bool IsUser,
    DateTime Timestamp,
    string? SponsorName = null,
    int TokenCount = 0
);
```

#### Advertisement.cs
```csharp
public record Advertisement(
    string Id,
    string ProductName,
    string Tagline,
    string Disclaimer,
    string IconSource,
    string GradientStart,
    string GradientEnd,
    int SponsoredTokens,
    string LearnMoreUrl
);
```

#### SolutionItem.cs
```csharp
public record SolutionItem(
    string Name,
    string IconKey,
    bool IsFolder,
    ObservableCollection<SolutionItem>? Children = null
);
```

### 2. ViewModels (CommunityToolkit.Mvvm)

#### ChatViewModel.cs
```csharp
public partial class ChatViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages = new();

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private int _totalTokensUsed;

    [RelayCommand]
    private async Task SendMessageAsync();
}
```

#### AdCarouselViewModel.cs
```csharp
public partial class AdCarouselViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Advertisement> _ads = new();

    [ObservableProperty]
    private int _currentAdIndex;

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private double _progress; // 0.0 to 1.0

    [ObservableProperty]
    private int _sponsoredTokenCount;
}
```

### 3. Services

#### IAIService.cs
```csharp
public interface IAIService
{
    IAsyncEnumerable<string> StreamResponseAsync(
        string prompt,
        CancellationToken ct = default);

    int EstimateTokenCount(string text);
}
```

#### ClaudeAIService.cs
The `ClaudeAIService` will:
- Connect to Claude API using the Anthropic SDK
- Stream responses in real-time for authentic "thinking" experience
- Track actual token usage from API response metadata
- Handle API errors gracefully (show satirical error messages)

```csharp
public class ClaudeAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public async IAsyncEnumerable<string> StreamResponseAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Use Claude Messages API with streaming
        // Each chunk yields text as it arrives
    }
}
```

**API Key Management**: Load from environment variable `ANTHROPIC_API_KEY`.

#### IAdRotationService.cs
```csharp
public interface IAdRotationService
{
    IReadOnlyList<Advertisement> GetAds();
    void StartRotation(Action<int> onAdChanged);
    void StopRotation();
}
```

The `AdRotationService` will:
- Use `DispatcherTimer` with 2-3 second intervals
- Cycle through fake advertisements
- Track token sponsorship counts

---

## UI Implementation Details

### Main Layout (MainPage.xaml)

Use a `Grid` with the following structure:
```
┌─────────────────────────────────────────────────┐
│ [Title Bar - Custom or OS native]              │ Auto
├─────────────────────────────────────────────────┤
│ [MenuBar]                                       │ Auto
├────────────┬────────────────────────────────────┤
│            │                                    │
│ Solution   │     Chat Panel                     │ *
│ Explorer   │     (with Ad Carousel overlay)    │
│            │                                    │
│ 220px      │           *                        │
├────────────┴────────────────────────────────────┤
│ [StatusBar]                                     │ Auto
└─────────────────────────────────────────────────┘
```

### Key Controls Mapping

| UI Element | Uno Platform Control | Notes |
|------------|---------------------|-------|
| Solution Explorer | `TreeView` | Hierarchical data with custom ItemTemplate |
| Menu Bar | `MenuBar` + `MenuBarItem` + `MenuFlyoutItem` | Fully supported on all platforms |
| Chat Messages | `ItemsRepeater` + `AutoLayout` | Virtualized list with custom templates |
| Ad Carousel | `FlipView` + `PipsPager` | Use `SelectorExtensions.PipsPager` from Toolkit |
| Typing Indicator | Custom `UserControl` with `ProgressRing` | Animated dots |
| Progress Bar (ads) | `ProgressBar` | Determinate mode for ad timing |
| Status Bar | `AutoLayout` (Horizontal) | Horizontal stack with status indicators |
| Token Counter | `TextBlock` with binding | Animated count-up effect optional |

### Responsive Layout Strategy

Use `VisualStateManager` with adaptive triggers to handle different screen sizes:

```xml
<VisualStateManager.VisualStateGroups>
    <VisualStateGroup>
        <!-- Wide: Full IDE layout with sidebar -->
        <VisualState x:Name="Wide">
            <VisualState.StateTriggers>
                <AdaptiveTrigger MinWindowWidth="900" />
            </VisualState.StateTriggers>
            <VisualState.Setters>
                <Setter Target="SolutionExplorerColumn.Width" Value="220" />
                <Setter Target="SolutionExplorer.Visibility" Value="Visible" />
            </VisualState.Setters>
        </VisualState>

        <!-- Narrow: Chat-only, hide sidebar -->
        <VisualState x:Name="Narrow">
            <VisualState.StateTriggers>
                <AdaptiveTrigger MinWindowWidth="0" />
            </VisualState.StateTriggers>
            <VisualState.Setters>
                <Setter Target="SolutionExplorerColumn.Width" Value="0" />
                <Setter Target="SolutionExplorer.Visibility" Value="Collapsed" />
            </VisualState.Setters>
        </VisualState>
    </VisualStateGroup>
</VisualStateManager.VisualStateGroups>
```

**Breakpoints**:
- **Wide (900px+)**: Full IDE layout with Solution Explorer sidebar
- **Narrow (<900px)**: Chat panel only, Solution Explorer hidden, simplified status bar

### Dark Theme Resources (ColorPaletteOverride.xaml)

```xml
<ResourceDictionary>
    <!-- IDE Background Colors -->
    <Color x:Key="IDEBackgroundColor">#1E1E1E</Color>
    <Color x:Key="IDESidebarColor">#252526</Color>
    <Color x:Key="IDEPanelColor">#2D2D2D</Color>
    <Color x:Key="IDEBorderColor">#3C3C3C</Color>

    <!-- Chat Colors -->
    <Color x:Key="UserMessageColor">#7C3AED</Color>
    <Color x:Key="AIMessageColor">#374151</Color>
    <Color x:Key="SponsorBadgeColor">#F59E0B</Color>

    <!-- Status Colors -->
    <Color x:Key="StatusReadyColor">#10B981</Color>
    <Color x:Key="StatusWarningColor">#F59E0B</Color>
    <Color x:Key="PremiumGoldColor">#FFD700</Color>

    <!-- Text Colors -->
    <Color x:Key="PrimaryTextColor">#FFFFFF</Color>
    <Color x:Key="SecondaryTextColor">#9CA3AF</Color>
    <Color x:Key="MutedTextColor">#6B7280</Color>
</ResourceDictionary>
```

### Animations

1. **Typing Indicator**: Three dots with staggered opacity animation
2. **Ad Carousel Transition**: Fade or slide between ads
3. **Token Counter**: Count-up animation when response completes
4. **Premium Button**: Shimmer/gradient animation using `Storyboard`

Example shimmer animation:
```xml
<Storyboard x:Name="ShimmerAnimation" RepeatBehavior="Forever">
    <DoubleAnimation
        Storyboard.TargetName="ShimmerTransform"
        Storyboard.TargetProperty="X"
        From="-100" To="200"
        Duration="0:0:2"/>
</Storyboard>
```

---

## Fake Advertisements (Sample Data)

| Product | Tagline | Disclaimer | Tokens |
|---------|---------|------------|--------|
| NullPointer Coffee™ | "The only coffee strong enough to debug your code at 3 AM. Now with 50% more caffeine!" | *Results may vary. Coffee cannot fix your code. | ~50 |
| ErgoCode Chair Pro | "Sit for 16 hours straight without regret. Your spine will thank you (eventually)." | *Not responsible for actual spine health. | ~75 |
| BrainStack Supplements | "10x your coding speed with our proprietary nootropic blend!" | *FDA has not evaluated these claims. | ~60 |
| CloudLess Hosting | "Host your apps on our servers. They're just computers we found." | *99.9% uptime not guaranteed. | ~45 |
| GitBlame Insurance | "When production goes down, we'll find someone else to blame!" | *Legal protection not included. | ~55 |

---

## Interaction Flow

1. **User sends message** → Input box clears, message appears right-aligned
2. **AI "thinking" begins** →
   - Typing indicator appears
   - Ad carousel slides in/expands
   - Ads rotate every 2-3 seconds
   - Progress bar fills based on estimated response time
3. **Response streams in** →
   - Text appears incrementally (simulated streaming)
   - Token counter ticks up
   - Current sponsor badge updates
4. **Response complete** →
   - Ad carousel collapses/hides
   - Final sponsor badge shown on message
   - Total tokens updated

---

## Stretch Goals (Post-MVP)

1. **"GO PREMIUM" Button** - Shows modal: "Premium coming soon! For now, enjoy our sponsors."
2. **Ad Blocker Detection** - Fake error dialog: "Ad blocker detected! AI tokens require sponsor support."
3. **Sound Effects** - Coin/cash register sounds as tokens generate
4. **Sponsor Tiers** - Bronze/Silver/Gold badges based on ad value
5. **Fake Build Output** - Console panel showing "Build succeeded" messages
6. **Easter Eggs** - Hidden menu options with joke functionality

---

## Estimated Implementation Effort

| Component | Effort | Priority |
|-----------|--------|----------|
| Project setup + dark theme | 1 hour | P0 |
| Main layout (Grid shell) | 1 hour | P0 |
| MenuBar (fake menus) | 1 hour | P1 |
| Solution Explorer (TreeView) | 2 hours | P1 |
| Chat Panel + Messages | 3 hours | P0 |
| Ad Carousel System | 3 hours | P0 |
| Claude AI Service (streaming) | 2.5 hours | P0 |
| Ad Rotation Service | 1 hour | P0 |
| Status Bar | 1 hour | P1 |
| Responsive Layout | 1.5 hours | P1 |
| Animations + Polish | 2 hours | P2 |
| **Total** | **~18-20 hours** | |

---

## Design Decisions

| Question | Decision |
|----------|----------|
| **Window Chrome** | Native OS chrome - simpler for a sample app |
| **AI Integration** | Real AI backend (Claude API) - makes the demo more compelling |
| **Ad Click Tracking** | "Learn More" does nothing for now |
| **Responsive Behavior** | Yes - hide Solution Explorer on narrow screens, adapt layout |
| **Sound Effects** | No - out of scope for sample |
| **Localization** | English only |
| **Persistence** | No persistence required - reset on launch (simpler for sample app) |

---

## Unresolved Questions

None - all design decisions have been finalized.

---

## Next Steps

1. Create Uno Platform project with recommended template options
2. Set up dark theme and color palette
3. Implement main layout shell
4. Build Chat Panel and message components
5. Implement Ad Carousel (the star feature)
6. Wire up services and view models
7. Add animations and polish
8. Test on Windows and WebAssembly targets

---

*Document generated based on UI mockup analysis and Uno Platform best practices.*
