# Interactions Brief — YouTube × Microsoft (Uno Platform port)

**Status:** Draft v0.1 · Owner: DevRel · Source of truth for motion & feel

This brief covers motion language, transitions, micro-interactions, gestures, and accessibility behavior. Visual specs (color, type, layout) live in the design brief; component composition lives in the architecture brief. Every transition described here is implemented through standard XAML mechanisms — `VisualStateManager`, `Storyboard`, `ConnectedAnimation`, MVUX `IFeed.IsExecuting` — never code-behind animation imperatives.

---

## 1. Motion principles

Three rules govern every animated change in the app:

1. **Motion serves wayfinding, not decoration.** If a transition does not help the user understand what changed or where they are, we cut it.
2. **Duration scales with travel distance, not with importance.** A nav-pill slide and a drawer slide-in have different durations because they cover different physical distances on screen, not because one matters more.
3. **Feedback is immediate; resolution can be relaxed.** State acknowledgement (focus ring, press depression, ripple) starts within one frame; the *result* of an action (drawer fully open, route fully changed) can take 300–400 ms with easing.

### 1.1 Duration tiers

| Tier         | Duration | Use                                                         |
| ------------ | -------- | ----------------------------------------------------------- |
| Instant      | 0–80 ms  | State acknowledgement: focus ring fade-in, press depression |
| Snap         | 120 ms   | Hover overlays, chip selection, badge pulse single beat     |
| Standard     | 250 ms   | Card lift, thumbnail zoom, search field focus halo          |
| Expressive   | 350 ms   | Active-nav indicator slide, drawer open, route swap         |
| Long         | 600 ms   | Hero entrance, page-load reveal sequence                    |

Anything longer than 600 ms is considered an effect (skeleton shimmer loops, avatar breathing) and runs on its own clock.

### 1.2 Easing

| Curve              | Cubic-bezier                       | Use                                              |
| ------------------ | ---------------------------------- | ------------------------------------------------ |
| Standard ease-out  | `0.16, 1, 0.30, 1`                 | Default for entrances and slides                 |
| Spring             | `0.34, 1.56, 0.64, 1`              | Press releases, scale-in (carousel buttons, play disc) |
| Linear             | n/a                                | Loops only (avatar ring rotation, skeleton shimmer) |
| Decelerate         | `0, 0, 0.2, 1`                     | Drawer close, dismissals (no overshoot, just settle) |

Curves are encoded once as `KeySpline` resources in `App.xaml` and reused by name. We do not author per-storyboard easing.

## 2. Macro transitions — page-level

### 2.1 First paint of HomePage

Sequence (total ≈ 800 ms, all staggered, all running off the dispatcher idle):

1. Page title fades in from y +14 dp · standard · 0 ms delay
2. Hero card fades + scales from 0.985 · expressive · 80 ms delay
3. Recommendation rail items cascade in from x +12 dp · standard · 40 ms each, starting at 200 ms
4. "For You" header + carousel together · expressive · 320 ms delay
5. Trending grid · expressive · 480 ms delay

Implemented via `Microsoft.UI.Xaml.Media.Animation.ConnectedAnimationService` on the `Loaded` event for the page-level reveal, then per-section `Storyboard`s on each region's `Loaded`. We do not animate every card individually — that becomes jittery on slower hardware. We animate the *containers*; child layouts settle inside.

### 2.2 Route change between sidebar items

The sidebar uses a `Region.Navigator="Visibility"` shell. Behavior:

- **Old route** fades to opacity 0 over 120 ms (snap), Y +6 dp
- **New route** fades from 0, Y -6 dp, over 250 ms (standard) starting 80 ms after the old one began leaving — they overlap by ~40 ms so the screen never goes blank
- The sidebar pill slides simultaneously (see §3.1)

If a route is selected for the first time, its content is also playing the entrance sequence from §2.1 — the two cascades stack naturally without manual coordination.

### 2.3 Drawer open / close (mobile)

The sidebar collapses to `utu:DrawerControl` at Normal and below. Open animation:

- Drawer translates X from `-100%` to `0` · expressive · standard ease-out
- Scrim fades from 0 to 0.6 alpha · standard · linear
- Hamburger icon rotates 90° to an X · standard · spring

Close runs the inverse with decelerate easing, 200 ms — closing should feel faster than opening so it never blocks dismissal.

## 3. Meso interactions — component-level

### 3.1 Sidebar active-nav pill

The Primary-tinted pill behind the active nav item is a single visual element animated by translation, not added/removed per click. Mechanism:

- Bound to `ShellModel.CurrentRoute` via `IState<string>`
- A `VisualStateManager.GoToState` call on the host triggers a state with the new translation
- Storyboard animates `TranslateTransform.Y` over 350 ms with the standard ease-out

This avoids the layout thrash of opacity-toggling per item, which is what hand-rolled implementations usually do.

### 3.2 Search field

Three-state interaction:

- **Idle** — OutlineVariant border, leading magnifier icon at 70 % opacity
- **Focused** — Primary border, 4 dp glow halo (a sibling `Border` with `Translation="0,0,8"` and a soft Primary brush, opacity bound to `IsFocused`), magnifier icon to 100 %, suggestions flyout opens. 250 ms standard.
- **Has text** — trailing clear button fades in over 120 ms, suggestions filter inline

Suggestions flyout uses `Popup` with light dismissal. Items appear with a 4-deep stagger (40 ms each) — not because they need it dramatically, but because the stagger gives the flyout an apparent direction (top-down) which reinforces "these came from your typing."

### 3.3 Carousel (For You row)

`ScrollViewer` with `HorizontalScrollMode="Enabled"`, `IsScrollInertiaEnabled="True"`, hosting an `ItemsRepeater` with `StackLayout`.

- Mouse: hover surfaces 36-dp circular `Button`s on the left and right edges (fade in 120 ms). Click scrolls by 60 % of viewport with `ChangeView(..., disableAnimation: false)` — the built-in animation is acceptable here.
- Touch: native pan + flick. Inertia is platform default; we don't override it.
- Keyboard: `Left/Right` when focus is inside the repeater scrolls one card width; `Home/End` jumps to extremes.
- The buttons disable themselves when `ScrollViewer.HorizontalOffset` is at either limit, fading their opacity to 0 (so they never look broken).

Snap behavior: `ScrollViewer.HorizontalSnapPointsType="Optional"`. Cards snap to the start edge if the user releases near a boundary; otherwise they free-scroll. Mandatory snapping feels coercive on desktop.

### 3.4 Video card hover

A composite of three concurrent micro-animations, all triggered by entering `PointerOver` state:

1. Card translates Y -3 dp · standard
2. Thumbnail `Image` scales to 1.04 with the same duration · standard
3. Play disc (a 56 dp `Border` with the Primary fill) scales from 0.7 to 1.0 · spring

On exit, all three reverse over 200 ms decelerate. The `RenderTransformOrigin` on the image is 0.5,0.5 so the zoom is centered.

Pressed state pushes the card down by 1 dp and runs a Material ripple inside the `CardContentControl` (built into the Material template — we don't add it).

### 3.5 Hero play button

- **Hover:** lift 1 dp + scale 1.02 + shadow Z 16 → 24 · standard
- **Press:** scale 0.98 · snap, then ripple from the click point
- **Click:** trigger `Play` command on `HomeModel`. The button's Material template handles the ripple. After the command resolves, `Frame.Navigate` runs the route transition.

The ripple is a `Microsoft.UI.Xaml.Controls.Primitives.RippleEffect` (or the Material toolkit's overlay if Material's button style is in effect — usually it is). We do not roll our own.

### 3.6 Carousel chevron buttons

Hidden by default, fade in over 120 ms when pointer enters the carousel container. Each is a `RepeatButton` so holding triggers continuous scroll. Disabled-edge state fades opacity to 0 — visible but inert is worse than absent here because it implies "still there to find more."

## 4. Micro-interactions

### 4.1 Avatar breathing ring

The conic-gradient ring on the user avatar runs two concurrent loops:

- **Rotation** — full 360° over 6 s, linear, infinite. Storyboard on a `RotateTransform`.
- **Brightness pulse** — `Opacity` 0.85 ↔ 1 over 3 s, ease-in-out, infinite.

Both pause when the app goes to background. They also pause if the user has `Reduced Motion` enabled (see §6).

### 4.2 Notification dot

A 6-dp `Ellipse` with a Primary fill and a Primary glow (a sibling `Ellipse` with `Translation="0,0,4"` and 50 % opacity). Animation: scale 0.85 → 1.10 over 2.4 s, ease-in-out, infinite. The unread badge on the bell uses the same animation but at 8 dp.

### 4.3 Bell shake

When the notification panel closes (only — not on open, where the panel itself is the feedback), the bell icon plays a 600 ms shake: rotation keyframes at 0°, 12°, -10°, 6°, -4°, 0°, with the standard ease-out across the whole sequence. Origin is the bell handle (`RenderTransformOrigin="0.5,0.0"`).

### 4.4 Verified tick

The teal verified glyph next to channel names plays a one-shot `Storyboard` on first appearance (when its containing card enters the viewport): scale 0 → 1 with the spring curve, 350 ms. After that it's static. Implemented via an attached behavior listening to `EffectiveViewportChanged`, not by animating every tick on page load.

## 5. Loading, empty, error

`utu:LoadingView` is the canonical mechanism. Each section's content is wrapped:

```xml
<utu:LoadingView Source="{Binding ForYou}">
  <utu:LoadingView.LoadingContent>
    <local:VideoCardSkeleton />
  </utu:LoadingView.LoadingContent>
  <ItemsRepeater ItemsSource="{Binding ForYou}" .../>
</utu:LoadingView>
```

`LoadingView.Source` is bound to the `IListFeed` directly — MVUX implements `ILoadable`, so no glue is needed.

### 5.1 Skeleton shimmer

The card skeleton is a layout-identical `Border` stack with `OutlineVariantBrush` fills and a `LinearGradientBrush` shimmer running across at 1.5 s, linear, infinite. We render at most 4 skeletons per row regardless of viewport — more is just animation cost without informational gain.

### 5.2 Empty state

When a feed resolves to zero items (e.g. user has no subscriptions), a centered `AutoLayout` shows: a 64-dp glyph at 30 % opacity, a `TitleMedium` headline, a `BodyMedium` helper line, and an optional CTA button. No animation — empty is a destination, not a transition.

### 5.3 Error state

Failed feeds render a `BodyMedium` line with `ErrorBrush` foreground and a "Try again" `Button` bound to the feed's `Refresh` command. We never silently retry — users should know something went wrong, and they should choose whether to keep trying.

## 6. Accessibility & input modalities

### 6.1 Keyboard

- `/` focuses the search field from anywhere (intercepted on `KeyDown` at the `MainPage` level, ignored if focus is already in a text input)
- `Tab` cycles in document order: top-bar → sidebar → main content
- Carousel keyboard handling per §3.3
- `Esc` dismisses any open flyout (notification panel, search suggestions)
- `Enter` / `Space` activate the focused control — defaults are correct, we don't intercept

### 6.2 Screen reader

Every entrance animation has a corresponding `AutomationPeer` announcement. The page title, when shown, announces "YouTube Home, page loaded." Loading state changes announce "Loading For You" / "For You loaded, 6 items." We use `AutomationProperties.LiveSetting="Polite"` on the title so it doesn't interrupt mid-sentence.

### 6.3 Reduced motion

The app honors the OS reduced-motion preference. When enabled:

- Entrance sequences (§2.1) collapse to a single 120-ms opacity fade
- Avatar ring stops rotating; brightness pulse stops
- Carousel scroll uses `disableAnimation: true`
- Card hover loses the lift and zoom (only the play disc fades — we keep that because it conveys functionality)
- All looping animations stop

This is read once at startup via `Windows.UI.ViewManagement.UISettings` (or platform equivalent on iOS/Android) and stored in a `bool` resource that styles bind against. We do not poll.

### 6.4 Touch targets

Per the design brief, all interactive elements are ≥ 44×44 dp. The hero `Play Video` pill is 44 dp tall × ~120 dp wide. Sidebar nav rows are 40 dp tall but with a 4 dp top/bottom hit-test extension via `MinHeight="44"` on the parent `AutoLayout` row.

## 7. What we explicitly do not animate

Calling these out so they don't get added later by accident:

- **Theme switching** — instant. No crossfade. Not exposed to the user anyway (system-driven).
- **Search input typing** — placeholder text does not animate out, suggestions do not stagger on every keystroke (only on initial open per §3.2).
- **Scroll position restoration** when returning to a route — instant jump, not a smooth scroll. Smooth scrolling here feels like the page is fighting the user.
- **Image fade-in** on load — this looks classy but in practice creates a "loading-loading-loading" feel as users scan a grid. Images appear instantly; their containers were already revealed in the entrance sequence.

---
*Cross-references: see the architecture brief for the `IFeed.IsExecuting` contract that powers `LoadingView`, and the design brief §7 for the static state visuals these transitions move between.*
