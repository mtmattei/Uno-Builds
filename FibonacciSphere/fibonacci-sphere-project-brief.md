# Interactive Fibonacci Sphere - Project Brief

## Overview

Build an interactive 3D Fibonacci sphere visualization using C# and XAML with real-time controls for point interaction, animation speed, wobble effects, point size, and motion trails.

---

## What is a Fibonacci Sphere?

A Fibonacci sphere distributes N points evenly across a sphere's surface using the golden angle (≈137.5°). Each point's position is calculated as:

```
golden_ratio = (1 + √5) / 2
golden_angle = 2π / golden_ratio²

For point i of N:
  y = 1 - (2i / (N - 1))           // -1 to 1
  radius = √(1 - y²)
  theta = golden_angle * i
  x = cos(theta) * radius
  z = sin(theta) * radius
```

This creates a visually pleasing, evenly-spaced distribution ideal for interactive visualizations.

---

## Feature Specifications

### 1. Point Interaction
| Feature | Description |
|---------|-------------|
| Hover highlight | Points glow or scale up when mouse/pointer is near |
| Click selection | Clicking a point selects it, showing info or triggering animation |
| Drag rotation | Click-drag on empty space rotates the entire sphere |
| Multi-select | Optional: shift-click to select multiple points |

### 2. Speed Control
| Feature | Description |
|---------|-------------|
| Rotation speed | Control auto-rotation velocity (0 = paused) |
| Animation speed | Global multiplier for all animations |
| Direction toggle | Clockwise / counter-clockwise rotation |
| Easing options | Linear, ease-in-out, elastic |

### 3. Wobble Effect
| Feature | Description |
|---------|-------------|
| Amplitude | How far points deviate from their base position |
| Frequency | Speed of the wobble oscillation |
| Per-point phase | Each point wobbles at a slightly offset phase |
| Wobble axis | Radial (in/out), tangential, or random |

### 4. Size Control
| Feature | Description |
|---------|-------------|
| Base point size | Uniform size for all points |
| Size variation | Random or index-based size differences |
| Pulse effect | Points rhythmically grow/shrink |
| Depth scaling | Points farther from camera appear smaller |

### 5. Trails
| Feature | Description |
|---------|-------------|
| Trail length | Number of historical positions to render |
| Trail opacity | Fade from solid to transparent |
| Trail style | Line, dots, or gradient ribbon |
| Trail color | Match point color or independent |

---

## Technical Approach

### Recommended: SkiaSharp + Manual 3D Projection

For this visualization, I recommend **SkiaSharp with manual 3D-to-2D projection** rather than full OpenGL. Here's why:

| Factor | SkiaSharp Approach | OpenGL Approach |
|--------|-------------------|-----------------|
| Complexity | Moderate | High |
| Uno Platform compatible | ✅ Yes | ⚠️ Requires native interop |
| Trails rendering | Easy (draw history) | Requires buffer management |
| Point interaction | Simple hit-testing | Ray casting required |
| Performance (500 points) | Excellent | Overkill |
| Learning curve | Gentle | Steep |

**When to use OpenGL instead:**
- You need 10,000+ points
- You want realistic lighting/shadows
- You're building a reusable 3D engine

### Core Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      MainWindow.xaml                     │
│  ┌─────────────────────┐  ┌───────────────────────────┐ │
│  │   SKXamlCanvas      │  │   Control Panel           │ │
│  │   (Render Surface)  │  │   - Sliders               │ │
│  │                     │  │   - Toggles               │ │
│  │                     │  │   - Color pickers         │ │
│  └─────────────────────┘  └───────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                    FibonacciSphereViewModel             │
│  - PointCount, Speed, WobbleAmplitude, TrailLength     │
│  - SelectedPoint, IsRotating                            │
│  - Commands: Reset, Randomize, Export                   │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                    SphereRenderer                        │
│  - GeneratePoints() → List<SpherePoint>                 │
│  - Update(deltaTime) → applies wobble, rotation         │
│  - Render(SKCanvas) → draws points + trails             │
│  - HitTest(screenPos) → SpherePoint?                    │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                    Camera3D                              │
│  - Position, Target, Up vector                          │
│  - ProjectToScreen(Vector3) → Vector2                   │
│  - RotateAround(axis, angle)                            │
│  - Zoom(delta)                                          │
└─────────────────────────────────────────────────────────┘
```

---

## Project Structure

```
FibonacciSphere/
├── FibonacciSphere.csproj
├── App.xaml
├── App.xaml.cs
│
├── Views/
│   ├── MainPage.xaml              # Main layout with canvas + controls
│   └── MainPage.xaml.cs
│
├── ViewModels/
│   └── SphereViewModel.cs         # All bindable properties + commands
│
├── Rendering/
│   ├── SphereRenderer.cs          # Main render loop logic
│   ├── Camera3D.cs                # 3D projection math
│   └── TrailRenderer.cs           # Trail-specific rendering
│
├── Models/
│   ├── SpherePoint.cs             # Position, velocity, color, trail history
│   └── SphereSettings.cs          # Serializable settings record
│
├── Math/
│   ├── FibonacciDistribution.cs   # Point generation algorithm
│   ├── Vector3Extensions.cs       # Helper methods
│   └── Easing.cs                  # Easing functions
│
└── Helpers/
    └── HitTesting.cs              # Screen-to-point detection
```

---

## Data Models

### SpherePoint.cs
```csharp
public class SpherePoint
{
    public int Index { get; init; }
    
    // Base position on unit sphere
    public Vector3 BasePosition { get; init; }
    
    // Current animated position
    public Vector3 CurrentPosition { get; set; }
    
    // For wobble calculation
    public float WobblePhase { get; init; }
    
    // Visual properties
    public float Size { get; set; }
    public SKColor Color { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    
    // Trail history (screen positions)
    public Queue<Vector2> TrailHistory { get; } = new();
}
```

### SphereSettings.cs
```csharp
public record SphereSettings
{
    public int PointCount { get; init; } = 200;
    public float RotationSpeed { get; init; } = 0.5f;
    public float WobbleAmplitude { get; init; } = 0.1f;
    public float WobbleFrequency { get; init; } = 2.0f;
    public float BasePointSize { get; init; } = 8f;
    public int TrailLength { get; init; } = 20;
    public float TrailOpacity { get; init; } = 0.5f;
    public bool DepthScaling { get; init; } = true;
}
```

---

## Key Algorithms

### Fibonacci Point Generation
```csharp
public static List<Vector3> GenerateFibonacciSphere(int count)
{
    var points = new List<Vector3>(count);
    float goldenRatio = (1 + MathF.Sqrt(5)) / 2;
    float goldenAngle = 2 * MathF.PI / (goldenRatio * goldenRatio);

    for (int i = 0; i < count; i++)
    {
        float y = 1 - (2f * i / (count - 1));
        float radius = MathF.Sqrt(1 - y * y);
        float theta = goldenAngle * i;

        points.Add(new Vector3(
            MathF.Cos(theta) * radius,
            y,
            MathF.Sin(theta) * radius
        ));
    }
    return points;
}
```

### Wobble Effect
```csharp
public Vector3 ApplyWobble(SpherePoint point, float time, SphereSettings settings)
{
    float wobble = MathF.Sin(time * settings.WobbleFrequency + point.WobblePhase);
    float offset = wobble * settings.WobbleAmplitude;
    
    // Radial wobble: push point in/out from center
    return point.BasePosition * (1 + offset);
}
```

### 3D to 2D Projection
```csharp
public Vector2 ProjectToScreen(Vector3 worldPos, Matrix4x4 viewProjection, Vector2 screenSize)
{
    var clip = Vector4.Transform(new Vector4(worldPos, 1), viewProjection);
    
    // Perspective divide
    var ndc = new Vector2(clip.X / clip.W, clip.Y / clip.W);
    
    // Map to screen coordinates
    return new Vector2(
        (ndc.X + 1) * 0.5f * screenSize.X,
        (1 - ndc.Y) * 0.5f * screenSize.Y  // Y is flipped
    );
}
```

---

## XAML Control Panel Layout

```xml
<Grid ColumnDefinitions="*, 300">
    <!-- Render Canvas -->
    <skia:SKXamlCanvas x:Name="Canvas" 
                       PaintSurface="OnPaintSurface"
                       PointerPressed="OnPointerPressed"
                       PointerMoved="OnPointerMoved"/>
    
    <!-- Control Panel -->
    <ScrollViewer Grid.Column="1">
        <StackPanel Padding="16" Spacing="16">
            
            <TextBlock Text="POINTS" Style="{StaticResource Header}"/>
            <Slider Minimum="50" Maximum="1000" 
                    Value="{Binding PointCount, Mode=TwoWay}"/>
            <TextBlock Text="{Binding PointCount, StringFormat='Count: {0}'}"/>
            
            <TextBlock Text="ROTATION" Style="{StaticResource Header}"/>
            <Slider Minimum="0" Maximum="2" 
                    Value="{Binding RotationSpeed, Mode=TwoWay}"/>
            <ToggleSwitch IsOn="{Binding IsRotating, Mode=TwoWay}" 
                          OnContent="Auto" OffContent="Manual"/>
            
            <TextBlock Text="WOBBLE" Style="{StaticResource Header}"/>
            <Slider Minimum="0" Maximum="0.5" 
                    Value="{Binding WobbleAmplitude, Mode=TwoWay}"/>
            <Slider Minimum="0.5" Maximum="5" 
                    Value="{Binding WobbleFrequency, Mode=TwoWay}"/>
            
            <TextBlock Text="SIZE" Style="{StaticResource Header}"/>
            <Slider Minimum="2" Maximum="20" 
                    Value="{Binding BasePointSize, Mode=TwoWay}"/>
            <ToggleSwitch IsOn="{Binding DepthScaling, Mode=TwoWay}" 
                          OnContent="Depth Scale" OffContent="Uniform"/>
            
            <TextBlock Text="TRAILS" Style="{StaticResource Header}"/>
            <Slider Minimum="0" Maximum="50" 
                    Value="{Binding TrailLength, Mode=TwoWay}"/>
            <Slider Minimum="0" Maximum="1" 
                    Value="{Binding TrailOpacity, Mode=TwoWay}"/>
            
        </StackPanel>
    </ScrollViewer>
</Grid>
```

---

## Implementation Milestones

### Phase 1: Foundation (Day 1-2)
- [ ] Create project with SkiaSharp.Views.Uno package
- [ ] Implement `FibonacciDistribution.cs` with point generation
- [ ] Implement `Camera3D.cs` with basic projection
- [ ] Render static points on canvas

### Phase 2: Rotation & Interaction (Day 3-4)
- [ ] Add mouse drag rotation
- [ ] Implement auto-rotation with speed control
- [ ] Add point hover detection (highlight nearest point)
- [ ] Add point click selection

### Phase 3: Animation Effects (Day 5-6)
- [ ] Implement wobble effect with per-point phase
- [ ] Add size pulsing animation
- [ ] Implement depth-based scaling and opacity

### Phase 4: Trails (Day 7)
- [ ] Track position history in `SpherePoint`
- [ ] Render trail lines with opacity fade
- [ ] Optimize trail rendering for performance

### Phase 5: Polish (Day 8-10)
- [ ] Build full control panel UI
- [ ] Add color themes / gradients
- [ ] Add settings persistence
- [ ] Performance optimization
- [ ] Add reset / randomize commands

---

## Dependencies

```xml
<PackageReference Include="SkiaSharp" Version="2.88.*" />
<PackageReference Include="SkiaSharp.Views.Uno.WinUI" Version="2.88.*" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />
<PackageReference Include="System.Numerics.Vectors" Version="4.5.0" />
```

---

## Performance Considerations

| Points | Expected FPS | Notes |
|--------|--------------|-------|
| 100 | 60+ | Smooth, room for more effects |
| 500 | 60 | Sweet spot for visual density |
| 1000 | 45-60 | May need to reduce trail length |
| 2000+ | 30-45 | Consider switching to OpenGL |

**Optimization strategies:**
1. Only update visible points (frustum culling)
2. Use `SKPath` batching for trails instead of individual draws
3. Reduce trail history when point count is high
4. Use `SKPaint` object pooling
5. Consider render-to-texture for static elements

---

## Future Enhancements

- **Audio reactivity**: Wobble amplitude responds to music
- **Point connections**: Draw lines between nearby points
- **Multiple spheres**: Nested or orbiting spheres
- **Export**: Save as animated GIF or video
- **VR mode**: Stereoscopic rendering for headsets
- **Particle effects**: Sparks when points collide

---

## Resources

- [Fibonacci Sphere Algorithm](https://bduvenhage.me/geometry/2019/07/31/generating-equidistant-vectors.html)
- [SkiaSharp Documentation](https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/)
- [3D Projection Math](https://www.scratchapixel.com/lessons/3d-basic-rendering/perspective-and-orthographic-projection-matrix)
- [Uno Platform SkiaSharp](https://platform.uno/docs/articles/features/skiasharp.html)
