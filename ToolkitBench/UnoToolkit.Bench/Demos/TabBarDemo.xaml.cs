using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace UnoToolkit.Bench.Demos;

public sealed partial class TabBarDemo : UserControl
{
    private int _activeIndex = 0;
    private Storyboard? _running;
    private Storyboard? _runningCanvas;

    private static readonly string[] Labels =
    {
        "A quiet home.",
        "What are you looking for?",
        "Your collection.",
        "About you.",
    };

    private record TabRefs(Grid IconHost, TextBlock Label, Storyboard HoverSb, DoubleAnimation HoverAnim);

    private List<TabRefs> _tabs = new();

    public TabBarDemo()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) =>
        {
            _tabs = new List<TabRefs>
            {
                BuildTab(IconHome,    LabelHome,    Tab0),
                BuildTab(IconSearch,  LabelSearch,  Tab1),
                BuildTab(IconLibrary, LabelLibrary, Tab2),
                BuildTab(IconProfile, LabelProfile, Tab3),
            };

            // Home is the initial active tab. XAML defaults are inactive for all four
            // tabs (consistent Stop()-revert behavior); commit Home to active here.
            CommitHomeState(active: true);
            if (IconHome.RenderTransform is CompositeTransform t)
            {
                t.ScaleX = 1.06;
                t.ScaleY = 1.06;
            }
        };
    }

    private static TabRefs BuildTab(Grid icon, TextBlock label, Border border)
    {
        // Cache hover storyboard + animation per tab. Reused on every PointerEntered/Exited
        // (which fire constantly during mouse movement) instead of allocating fresh.
        border.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        var transform = new CompositeTransform();
        border.RenderTransform = transform;

        var anim = new DoubleAnimation
        {
            Duration = TimeSpan.FromMilliseconds(140),
            EasingFunction = Easings.Decel,
        };
        Storyboard.SetTarget(anim, transform);
        Storyboard.SetTargetProperty(anim, "TranslateY");
        var sb = new Storyboard();
        sb.Children.Add(anim);

        var refs = new TabRefs(icon, label, sb, anim);
        border.PointerEntered += (_, _) => SetHover(refs, true);
        border.PointerExited  += (_, _) => SetHover(refs, false);
        return refs;
    }

    private static void SetHover(TabRefs t, bool over)
    {
        t.HoverSb.Stop();
        t.HoverAnim.To = over ? -1 : 0;
        t.HoverSb.Begin();
    }

    private void OnTabTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.Tag is not string tag) return;
        if (!int.TryParse(tag, out var index)) return;
        TriggerTab(index);
    }

    public void TriggerTab(int index)
    {
        if (index < 0 || index > 3) return;
        if (index == _activeIndex) return;
        var oldIndex = _activeIndex;
        _activeIndex = index;
        Animate(oldIndex, index);
        AnimateCanvasLabel(Labels[index]);
    }

    // Easing aliases — point at the shared static instances in Themes/Easings.cs.
    private static EasingFunctionBase Morph()   => Easings.MorphInOut;
    private static EasingFunctionBase BackOut() => Easings.BackOut;
    private static EasingFunctionBase Decel()   => Easings.Decel;

    private void Animate(int oldIndex, int newIndex)
    {
        _running?.Stop();

        var sb = new Storyboard();

        const int morphMs = 520;
        const int pulseMs = 540;

        // Whole-icon scale-pulse on both old and new
        ScalePulse(sb, _tabs[oldIndex].IconHost, fromScale: 1.06, toScale: 1.0, durationMs: pulseMs);
        ScalePulse(sb, _tabs[newIndex].IconHost, fromScale: 1.0,  toScale: 1.06, durationMs: pulseMs);

        // Label color swap.
        _tabs[oldIndex].Label.Foreground = Theme.Fg4;
        _tabs[newIndex].Label.Foreground = Theme.Fg1;

        // Per-tab morphs — old goes to Rest, new goes to Active
        MorphTab(sb, oldIndex, toActive: false, morphMs);
        MorphTab(sb, newIndex, toActive: true, morphMs);

        sb.Begin();
        _running = sb;
    }

    private static void ScalePulse(Storyboard sb, Grid iconHost, double fromScale, double toScale, int durationMs)
    {
        if (iconHost.RenderTransform is not CompositeTransform t) return;
        var ease = BackOut();
        Add(sb, t, "ScaleX", fromScale, toScale, durationMs, ease);
        Add(sb, t, "ScaleY", fromScale, toScale, durationMs, ease);
    }

    private void MorphTab(Storyboard sb, int index, bool toActive, int morphMs)
    {
        switch (index)
        {
            case 0: MorphHome(sb, toActive, morphMs); break;
            case 1: MorphSearch(sb, toActive, morphMs); break;
            case 2: MorphLibrary(sb, toActive, morphMs); break;
            case 3: MorphProfile(sb, toActive, morphMs); break;
        }
    }

    // Home morph — clean rewrite, four sequential phases:
    //
    //   t=0     Dot drops onto the closed pentagon with a bounce.
    //   t=460   Dot has landed. It fades out + scales to nothing.
    //   t=620   Pentagon swaps to the carved-doorway outline.
    //   t=720   Door panel rises from the floor, filling the cutout.
    //
    // No part of the active state (cutout outline OR door panel) is visible while the
    // dot is in flight or fading — the user reads "ink falls, then becomes the door."
    private void MorphHome(Storyboard sb, bool toActive, int morphMs)
    {
        if (toActive)
        {
            // Reset every animated property to its hidden/start state. The
            // CompositeTransform values are local so this snap takes effect now.
            HomeDotT.ScaleX = 0;
            HomeDotT.ScaleY = 0;
            HomeDotT.TranslateY = -15;
            HomeDot.Opacity = 0;
            HomeDoorT.ScaleY = 0;
            HomeDoor.Opacity = 0;

            // ── Phase 1: Dot drops with bounce (0 – 460 ms) ────────────────────
            const int dropMs = 460;
            AddBounceTranslateY(sb, HomeDotT, beginMs: 0, durationMs: dropMs);
            AddBounceScale(sb, HomeDotT, "ScaleX", beginMs: 0, durationMs: dropMs);
            AddBounceScale(sb, HomeDotT, "ScaleY", beginMs: 0, durationMs: dropMs);
            Add(sb, HomeDot, "Opacity", 0, 1, 200, Morph(), beginMs: 0);

            // ── Phase 2: Dot fades out (460 – 620 ms) ──────────────────────────
            const int dotOut = dropMs;
            Add(sb, HomeDot, "Opacity", 1, 0, 160, Decel(), beginMs: dotOut);
            Add(sb, HomeDotT, "ScaleX", 1, 0, 160, Decel(), beginMs: dotOut);
            Add(sb, HomeDotT, "ScaleY", 1, 0, 160, Decel(), beginMs: dotOut);

            // ── Phase 3: Pentagon swap — cutout appears (620 – 840 ms) ─────────
            const int swapAt = dotOut + 160;
            Add(sb, HomeRest,   "Opacity", 1, 0, 220, Morph(), beginMs: swapAt);
            Add(sb, HomeActive, "Opacity", 0, 1, 220, Morph(), beginMs: swapAt);

            // ── Phase 4: Door panel rises (720 – 1040 ms) ──────────────────────
            const int doorAt = swapAt + 100;
            Add(sb, HomeDoor, "Opacity", 0, 1, 240, Decel(), beginMs: doorAt);
            Add(sb, HomeDoorT, "ScaleY", 0, 1, 320, BackOut(), beginMs: doorAt);
        }
        else
        {
            // Reverse: door collapses, pentagon swaps back. Dot stays hidden —
            // it's purely an entrance gesture.
            Add(sb, HomeDoor, "Opacity", 1, 0, 200, Decel(), beginMs: 0);
            Add(sb, HomeDoorT, "ScaleY", 1, 0, 220, Decel(), beginMs: 0);
            Add(sb, HomeActive, "Opacity", 1, 0, 240, Morph(), beginMs: 80);
            Add(sb, HomeRest,   "Opacity", 0, 1, 240, Morph(), beginMs: 80);
        }

        CommitHomeState(toActive);
    }

    // Pin local DP values to the end state so a later Storyboard.Stop() doesn't
    // snap properties back to XAML defaults (now intentionally = inactive state).
    private void CommitHomeState(bool active)
    {
        if (active)
        {
            HomeRest.Opacity = 0;
            HomeActive.Opacity = 1;
            HomeDot.Opacity = 0;
            HomeDotT.ScaleX = 0;
            HomeDotT.ScaleY = 0;
            HomeDoor.Opacity = 1;
            HomeDoorT.ScaleY = 1;
        }
        else
        {
            HomeRest.Opacity = 1;
            HomeActive.Opacity = 0;
            HomeDot.Opacity = 0;
            HomeDoor.Opacity = 0;
            HomeDoorT.ScaleY = 0;
        }
    }

    // Search: ink dot drops with bounce → handle lifts → loop stretches horizontally
    // → loop crossfades into the filled search-bar pill. Reverse on deactivate.
    private void MorphSearch(Storyboard sb, bool toActive, int morphMs)
    {
        if (toActive)
        {
            // Pre-snap ink dot to its hidden rest position so the drop starts clean.
            SearchInkDotTransform.ScaleX = 0;
            SearchInkDotTransform.ScaleY = 0;
            SearchInkDotTransform.TranslateY = -14;
            SearchInkDot.Opacity = 0;

            // 1. Ink dot drops with bounce (the "ink lands" pattern).
            const int landDuration = 460;
            AddBounceTranslateY(sb, SearchInkDotTransform, beginMs: 0, durationMs: landDuration);
            AddBounceScale(sb, SearchInkDotTransform, "ScaleX", beginMs: 0, durationMs: landDuration);
            AddBounceScale(sb, SearchInkDotTransform, "ScaleY", beginMs: 0, durationMs: landDuration);
            Add(sb, SearchInkDot, "Opacity", 0, 1, 200, Morph(), beginMs: 0);

            // 2. Once the dot has landed, fade it out to clear the loop interior.
            const int morphStart = 380;
            Add(sb, SearchInkDot, "Opacity", 1, 0, 160, Decel(), beginMs: morphStart);
            Add(sb, SearchInkDotTransform, "ScaleX", 1, 0, 160, Decel(), beginMs: morphStart);
            Add(sb, SearchInkDotTransform, "ScaleY", 1, 0, 160, Decel(), beginMs: morphStart);

            // 3. Handle lifts off the icon and fades.
            Add(sb, SearchHandleTransform, "TranslateY", 0, -8, 280, Morph(), beginMs: morphStart);
            Add(sb, SearchHandle, "Opacity", 1, 0, 240, Decel(), beginMs: morphStart + 40);

            // 4. Loop stretches horizontally — 12×12 circle to 18×7 pill footprint.
            Add(sb, SearchLoopTransform, "ScaleX", 1, 1.5, 320, Morph(), beginMs: morphStart + 40);
            Add(sb, SearchLoopTransform, "ScaleY", 1, 0.58, 320, Morph(), beginMs: morphStart + 40);

            // 5. Loop fades out as the filled bar fades in at the same footprint.
            Add(sb, SearchLoop, "Opacity", 1, 0, 200, Decel(), beginMs: morphStart + 220);
            Add(sb, SearchBar, "Opacity", 0, 1, 240, Decel(), beginMs: morphStart + 220);
            Add(sb, SearchBarTransform, "ScaleX", 0.85, 1, 280, BackOut(), beginMs: morphStart + 220);
            Add(sb, SearchBarTransform, "ScaleY", 0.85, 1, 280, BackOut(), beginMs: morphStart + 220);
        }
        else
        {
            // Deactivate: time-reversed mirror of activate.
            // Bar shrinks/fades first (was last in activate), then loop fades back in
            // and unmorphs, then handle drops back into position.
            Add(sb, SearchBar, "Opacity", SearchBar.Opacity, 0, 220, Decel(), beginMs: 0);
            Add(sb, SearchBarTransform, "ScaleX", SearchBarTransform.ScaleX, 0.85, 240, Decel(), beginMs: 0);
            Add(sb, SearchBarTransform, "ScaleY", SearchBarTransform.ScaleY, 0.4, 240, Decel(), beginMs: 0);

            // Loop fades back in at its stretched scale, then unmorphs.
            Add(sb, SearchLoop, "Opacity", SearchLoop.Opacity, 1, 200, Decel(), beginMs: 100);
            Add(sb, SearchLoopTransform, "ScaleX", SearchLoopTransform.ScaleX, 1, 320, Morph(), beginMs: 140);
            Add(sb, SearchLoopTransform, "ScaleY", SearchLoopTransform.ScaleY, 1, 320, Morph(), beginMs: 140);

            // Handle drops back to position and fades in.
            Add(sb, SearchHandleTransform, "TranslateY", SearchHandleTransform.TranslateY, 0, 280, Morph(), beginMs: 220);
            Add(sb, SearchHandle, "Opacity", SearchHandle.Opacity, 1, 240, Decel(), beginMs: 260);
        }
    }

    // TranslateY drop: -14 → +1.8 (overshoots past 0) → -0.4 (small rebound) → 0 (settle).
    private static void AddBounceTranslateY(Storyboard sb, CompositeTransform target, int beginMs, int durationMs)
    {
        var anim = new DoubleAnimationUsingKeyFrames { BeginTime = TimeSpan.FromMilliseconds(beginMs) };
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = -14 });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs * 0.62)),
            Value = 1.8,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn },
        });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs * 0.82)),
            Value = -0.4,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs)),
            Value = 0,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
        });
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, "TranslateY");
        sb.Children.Add(anim);
    }

    // Scale punch past 1, small squish on landing, settle.
    private static void AddBounceScale(Storyboard sb, CompositeTransform target, string property, int beginMs, int durationMs)
    {
        var anim = new DoubleAnimationUsingKeyFrames { BeginTime = TimeSpan.FromMilliseconds(beginMs) };
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0 });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs * 0.62)),
            Value = 1.10,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs * 0.82)),
            Value = 0.96,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
        });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs)),
            Value = 1.0,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
        });
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, property);
        sb.Children.Add(anim);
    }

    // Library: each block translates + scales + spins 360° while crossfading from
    // outlined-square to filled-stripe. Items cascade with an 80ms stagger.
    //
    // CompositeTransform.ScaleX/Y is applied around CenterX/Y (local coords). With
    // CenterX=CenterY=3.5 on a 7×7 rect, scale induces an extra shift of:
    //     dx = 3.5 × (1 - 18/7)  ≈ -5.5
    //     dy = 3.5 × (1 - 3/7)   ≈  2.0
    // So translate values are computed from: tx = (target.X - margin.X) - dx,
    //                                        ty = (target.Y - margin.Y) - dy
    //
    //   Item 1 margin (3, 4)   → target (3, 4.5)   →  tx = 5.5,  ty = -1.5
    //   Item 2 margin (14, 4)  → target (3, 9.0)   →  tx = -5.5, ty =  3.0
    //   Item 3 margin (3, 14)  → target (3, 13.5)  →  tx = 5.5,  ty = -2.5
    //   Item 4 margin (14, 14) → target (3, 18.0)  →  tx = -5.5, ty =  2.0
    private void MorphLibrary(Storyboard sb, bool toActive, int morphMs)
    {
        const double sx = 18.0 / 7;
        const double sy = 3.0 / 7;
        const int stagger = 80;

        ScheduleLibBlock(sb, LibItem1Transform, LibItem1Outline, LibItem1Fill,
            tx:  5.5, ty: -1.5, scaleX: sx, scaleY: sy, toActive, beginMs: 0,           durationMs: morphMs);
        ScheduleLibBlock(sb, LibItem2Transform, LibItem2Outline, LibItem2Fill,
            tx: -5.5, ty:  3.0, scaleX: sx, scaleY: sy, toActive, beginMs: stagger,     durationMs: morphMs);
        ScheduleLibBlock(sb, LibItem3Transform, LibItem3Outline, LibItem3Fill,
            tx:  5.5, ty: -2.5, scaleX: sx, scaleY: sy, toActive, beginMs: stagger * 2, durationMs: morphMs);
        ScheduleLibBlock(sb, LibItem4Transform, LibItem4Outline, LibItem4Fill,
            tx: -5.5, ty:  2.0, scaleX: sx, scaleY: sy, toActive, beginMs: stagger * 3, durationMs: morphMs);
    }

    private static void ScheduleLibBlock(Storyboard sb, CompositeTransform t,
                                         Microsoft.UI.Xaml.Shapes.Shape outline,
                                         Microsoft.UI.Xaml.Shapes.Shape fill,
                                         double tx, double ty, double scaleX, double scaleY,
                                         bool toActive, int beginMs, int durationMs)
    {
        var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

        // Snap rotation to 0 at start so a re-activated tab spins forward again
        // (rotation accumulates otherwise — visually 360° == 0° but matters for animation From=).
        t.Rotation = 0;

        if (toActive)
        {
            Add(sb, t, "TranslateX", 0, tx, durationMs, ease, beginMs);
            Add(sb, t, "TranslateY", 0, ty, durationMs, ease, beginMs);
            Add(sb, t, "ScaleX",     1, scaleX, durationMs, ease, beginMs);
            Add(sb, t, "ScaleY",     1, scaleY, durationMs, ease, beginMs);
            Add(sb, t, "Rotation",   0, 360, durationMs, ease, beginMs);
            Add(sb, outline, "Opacity", 1, 0, durationMs, ease, beginMs);
            Add(sb, fill,    "Opacity", 0, 1, durationMs, ease, beginMs);
        }
        else
        {
            Add(sb, t, "TranslateX", tx, 0, durationMs, ease, beginMs);
            Add(sb, t, "TranslateY", ty, 0, durationMs, ease, beginMs);
            Add(sb, t, "ScaleX",     scaleX, 1, durationMs, ease, beginMs);
            Add(sb, t, "ScaleY",     scaleY, 1, durationMs, ease, beginMs);
            Add(sb, t, "Rotation",   0, 360, durationMs, ease, beginMs); // spin again on the way back
            Add(sb, outline, "Opacity", 0, 1, durationMs, ease, beginMs);
            Add(sb, fill,    "Opacity", 1, 0, durationMs, ease, beginMs);
        }
    }

    // Profile: outline figure transforms into a filled silhouette (head + body) while the
    // status ring + dot pop in with overshoot. The head also pulses scale 0.5 → 1.15 → 1
    // when filling — gives the moment of "lighting up" / identity confirmation.
    private void MorphProfile(Storyboard sb, bool toActive, int morphMs)
    {
        if (toActive)
        {
            // Outline fades out, filled silhouette fades in.
            Add(sb, ProfileHead,       "Opacity", ProfileHead.Opacity,       0, 240, Morph(), beginMs: 60);
            Add(sb, ProfileBody,       "Opacity", ProfileBody.Opacity,       0, 240, Morph(), beginMs: 60);
            Add(sb, ProfileHeadFill,   "Opacity", 0, 1, 280, Decel(),  beginMs: 0);
            Add(sb, ProfileBodyActive, "Opacity", 0, 1, 280, Decel(),  beginMs: 60);

            // Head fill scale-pulse: 0.5 → 1.15 → 1.0 — pops into shape.
            AddPop(sb, ProfileHeadFillTransform, "ScaleX", from: 0.5, peak: 1.15, settle: 1.0, beginMs: 0, durationMs: 360);
            AddPop(sb, ProfileHeadFillTransform, "ScaleY", from: 0.5, peak: 1.15, settle: 1.0, beginMs: 0, durationMs: 360);

            // Status ring backdrop + dot — dot has more dramatic overshoot now (scale to 1.4 then settle).
            Add(sb, ProfileStatusRing, "Opacity", 0, 1, 200, Morph(), beginMs: 180);
            Add(sb, ProfileRingTransform, "ScaleX", 0, 1, 280, BackOut(), beginMs: 180);
            Add(sb, ProfileRingTransform, "ScaleY", 0, 1, 280, BackOut(), beginMs: 180);
            Add(sb, ProfileStatusDot, "Opacity", 0, 1, 200, Morph(), beginMs: 280);
            AddPop(sb, ProfileDotTransform, "ScaleX", from: 0, peak: 1.4, settle: 1.0, beginMs: 280, durationMs: 360);
            AddPop(sb, ProfileDotTransform, "ScaleY", from: 0, peak: 1.4, settle: 1.0, beginMs: 280, durationMs: 360);
        }
        else
        {
            // Reverse: outline returns, filled silhouette fades out, status indicator collapses.
            Add(sb, ProfileHead,       "Opacity", ProfileHead.Opacity,       1, 240, Morph(), beginMs: 80);
            Add(sb, ProfileBody,       "Opacity", ProfileBody.Opacity,       1, 240, Morph(), beginMs: 80);
            Add(sb, ProfileHeadFill,   "Opacity", ProfileHeadFill.Opacity,   0, 200, Morph(), beginMs: 0);
            Add(sb, ProfileBodyActive, "Opacity", ProfileBodyActive.Opacity, 0, 200, Morph(), beginMs: 0);

            Add(sb, ProfileHeadFillTransform, "ScaleX", ProfileHeadFillTransform.ScaleX, 0.5, 200, Morph(), beginMs: 0);
            Add(sb, ProfileHeadFillTransform, "ScaleY", ProfileHeadFillTransform.ScaleY, 0.5, 200, Morph(), beginMs: 0);

            Add(sb, ProfileStatusRing, "Opacity", ProfileStatusRing.Opacity, 0, 180, Morph());
            Add(sb, ProfileRingTransform, "ScaleX", ProfileRingTransform.ScaleX, 0, 200, Morph());
            Add(sb, ProfileRingTransform, "ScaleY", ProfileRingTransform.ScaleY, 0, 200, Morph());
            Add(sb, ProfileStatusDot, "Opacity", ProfileStatusDot.Opacity, 0, 160, Morph());
            Add(sb, ProfileDotTransform, "ScaleX", ProfileDotTransform.ScaleX, 0, 200, Morph());
            Add(sb, ProfileDotTransform, "ScaleY", ProfileDotTransform.ScaleY, 0, 200, Morph());
        }
    }

    // Three-keyframe pop: from → peak (overshoot) → settle. Uses CubicEase per segment.
    private static void AddPop(Storyboard sb, CompositeTransform target, string property,
                               double from, double peak, double settle, int beginMs, int durationMs)
    {
        var anim = new DoubleAnimationUsingKeyFrames { BeginTime = TimeSpan.FromMilliseconds(beginMs) };
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = from });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs * 0.55)),
            Value = peak,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs)),
            Value = settle,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
        });
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, property);
        sb.Children.Add(anim);
    }

    private static void Add(Storyboard sb, DependencyObject target, string property,
                            double from, double to, int durationMs,
                            EasingFunctionBase? ease = null, int beginMs = 0)
    {
        var anim = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromMilliseconds(durationMs),
            BeginTime = TimeSpan.FromMilliseconds(beginMs),
            EasingFunction = ease,
        };
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, property);
        sb.Children.Add(anim);
    }

    private void AnimateCanvasLabel(string newText)
    {
        _runningCanvas?.Stop();

        var sb = new Storyboard();
        // Lead-in delay so the icon strike + lift land before the label swap —
        // makes the moment read as one event ending, not two simultaneous ones.
        var fadeOut = new DoubleAnimation
        {
            BeginTime = TimeSpan.FromMilliseconds(100),
            Duration = TimeSpan.FromMilliseconds(150),
            From = 1, To = 0,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(fadeOut, CanvasLabel);
        Storyboard.SetTargetProperty(fadeOut, "Opacity");
        sb.Children.Add(fadeOut);

        sb.Completed += (s, e) =>
        {
            CanvasLabel.Text = newText;
            var sb2 = new Storyboard();
            var fadeIn = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(180),
                From = 0, To = 1,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(fadeIn, CanvasLabel);
            Storyboard.SetTargetProperty(fadeIn, "Opacity");
            sb2.Children.Add(fadeIn);
            sb2.Begin();
            _runningCanvas = sb2;
        };
        sb.Begin();
        _runningCanvas = sb;
    }
}
