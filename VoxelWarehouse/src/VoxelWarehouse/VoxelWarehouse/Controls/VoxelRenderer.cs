using SkiaSharp;

namespace VoxelWarehouse.Controls;

public static class VoxelRenderer
{
    private const int HW = GridConstants.HalfWidth;
    private const int HH = GridConstants.HalfHeight;
    private const int Depth = GridConstants.VoxelDepth;

    #region Asset Colors

    // Wider brightness spread + stronger accent tints for visual distinction
    private static readonly (byte Top, byte Left, byte Right, byte AR, byte AG, byte AB)[] AssetColors =
    [
        (190, 145, 100, 210, 185, 140),  // Pallet — warm amber tint
        (165, 125,  88, 120, 160, 200),  // Rack — cool blue-steel tint
        (175, 130,  90, 180, 165, 145),  // Container — warm neutral
        (130, 100,  68, 200, 140, 130),  // Equipment — reddish-dark
        ( 65,  48,  35,  70,  75,  80),  // Aisle — very dark, cool
    ];

    #endregion

    #region Zone Colors

    private static readonly (byte R, byte G, byte B)[] ZoneFillColors =
    [
        (0, 0, 0), (200, 210, 220), (220, 215, 200), (210, 200, 220), (200, 220, 210),
    ];

    private static readonly (byte R, byte G, byte B)[] ZoneBorderColors =
    [
        (0, 0, 0), (180, 200, 220), (220, 210, 180), (200, 180, 220), (180, 220, 200),
    ];

    #endregion

    public static void DrawGrid(SKCanvas canvas, float originX, float originY, int gridSize, int rotation = 0, float alpha = 1f)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, (byte)(10 * alpha)),
            StrokeWidth = 0.5f, IsAntialias = true, Style = SKPaintStyle.Stroke
        };

        for (int i = 0; i <= gridSize; i++)
        {
            var (x1, y1) = IsoMath.GridToScreen(i, 0, 0, originX, originY, rotation);
            var (x2, y2) = IsoMath.GridToScreen(i, gridSize, 0, originX, originY, rotation);
            canvas.DrawLine(x1, y1 + HH, x2, y2 + HH, paint);

            var (x3, y3) = IsoMath.GridToScreen(0, i, 0, originX, originY, rotation);
            var (x4, y4) = IsoMath.GridToScreen(gridSize, i, 0, originX, originY, rotation);
            canvas.DrawLine(x3, y3 + HH, x4, y4 + HH, paint);
        }
    }

    public static void DrawZones(SKCanvas canvas, float originX, float originY,
        IImmutableDictionary<(int X, int Z), ZoneType> zones, int rotation = 0)
    {
        foreach (var (key, zone) in zones)
        {
            if (zone == ZoneType.None) continue;
            var idx = (int)zone;
            var fill = ZoneFillColors[idx];
            var border = ZoneBorderColors[idx];

            var (sx, sy) = IsoMath.GridToScreen(key.X, key.Z, 0, originX, originY, rotation);
            using var diamond = MakeDiamond(sx, sy + HH);

            using var fillPaint = new SKPaint { Color = new SKColor(fill.R, fill.G, fill.B, 40), Style = SKPaintStyle.Fill, IsAntialias = true };
            using var borderPaint = new SKPaint { Color = new SKColor(border.R, border.G, border.B, 80), Style = SKPaintStyle.Stroke, StrokeWidth = 1.0f, IsAntialias = true };

            canvas.DrawPath(diamond, fillPaint);
            canvas.DrawPath(diamond, borderPaint);
        }
    }

    public static void DrawGroundShadows(SKCanvas canvas, float originX, float originY,
        IImmutableDictionary<(int X, int Y, int Z), AssetType> cells, int rotation = 0)
    {
        using var shadowPaint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };

        foreach (var (key, _) in cells)
        {
            if (key.Y <= 0) continue;
            float fog = IsoMath.FogFactor(key.X, key.Z, rotation);
            shadowPaint.Color = new SKColor(0, 0, 0, (byte)(23 * fog));

            var (sx, sy) = IsoMath.GridToScreen(key.X, key.Z, 0, originX, originY, rotation);
            using var diamond = MakeDiamond(sx, sy + HH);
            canvas.DrawPath(diamond, shadowPaint);
        }
    }

    public static void DrawVoxel(SKCanvas canvas, float sx, float sy,
        AssetType asset, float aoTop, float aoLeft, float aoRight, float fog)
    {
        var colors = AssetColors[(int)asset];

        // Stronger accent blend (0.30) so asset types are visually distinct
        byte Compute(byte baseVal, byte accent, float ao) =>
            (byte)Math.Clamp((baseVal * ao * fog * 0.7f + accent * 0.30f), 0, 255);

        var topR = Compute(colors.Top, colors.AR, aoTop);
        var topG = Compute(colors.Top, colors.AG, aoTop);
        var topB = Compute(colors.Top, colors.AB, aoTop);
        var leftR = Compute(colors.Left, colors.AR, aoLeft);
        var leftG = Compute(colors.Left, colors.AG, aoLeft);
        var leftB = Compute(colors.Left, colors.AB, aoLeft);
        var rightR = Compute(colors.Right, colors.AR, aoRight);
        var rightG = Compute(colors.Right, colors.AG, aoRight);
        var rightB = Compute(colors.Right, colors.AB, aoRight);

        using var topPath = new SKPath();
        topPath.MoveTo(sx, sy); topPath.LineTo(sx + HW, sy + HH);
        topPath.LineTo(sx, sy + 2 * HH); topPath.LineTo(sx - HW, sy + HH); topPath.Close();

        using var leftPath = new SKPath();
        leftPath.MoveTo(sx - HW, sy + HH); leftPath.LineTo(sx, sy + 2 * HH);
        leftPath.LineTo(sx, sy + 2 * HH + Depth); leftPath.LineTo(sx - HW, sy + HH + Depth); leftPath.Close();

        using var rightPath = new SKPath();
        rightPath.MoveTo(sx + HW, sy + HH); rightPath.LineTo(sx, sy + 2 * HH);
        rightPath.LineTo(sx, sy + 2 * HH + Depth); rightPath.LineTo(sx + HW, sy + HH + Depth); rightPath.Close();

        using var topPaint = new SKPaint { Color = new SKColor(topR, topG, topB), Style = SKPaintStyle.Fill, IsAntialias = true };
        using var leftPaint = new SKPaint { Color = new SKColor(leftR, leftG, leftB), Style = SKPaintStyle.Fill, IsAntialias = true };
        using var rightPaint = new SKPaint { Color = new SKColor(rightR, rightG, rightB), Style = SKPaintStyle.Fill, IsAntialias = true };

        canvas.DrawPath(rightPath, rightPaint);
        canvas.DrawPath(leftPath, leftPaint);
        canvas.DrawPath(topPath, topPaint);

        using var outlinePaint = new SKPaint { Color = new SKColor(10, 10, 14, (byte)(89 * fog)), Style = SKPaintStyle.Stroke, StrokeWidth = 0.7f, IsAntialias = true };
        canvas.DrawPath(topPath, outlinePaint);
        canvas.DrawPath(leftPath, outlinePaint);
        canvas.DrawPath(rightPath, outlinePaint);

        using var edgePaint = new SKPaint { Color = new SKColor(255, 255, 255, (byte)(51 * fog)), Style = SKPaintStyle.Stroke, StrokeWidth = 1.1f, IsAntialias = true };
        canvas.DrawLine(sx, sy, sx - HW, sy + HH, edgePaint);
        canvas.DrawLine(sx, sy, sx + HW, sy + HH, edgePaint);

        using var contactPaint = new SKPaint { Color = new SKColor(0, 0, 0, (byte)(36 * fog)), Style = SKPaintStyle.Stroke, StrokeWidth = 1.0f, IsAntialias = true };
        canvas.DrawLine(sx - HW, sy + HH + Depth, sx, sy + 2 * HH + Depth, contactPaint);
        canvas.DrawLine(sx + HW, sy + HH + Depth, sx, sy + 2 * HH + Depth, contactPaint);
    }

    public static void DrawAssetLabel(SKCanvas canvas, float sx, float sy, AssetType asset, float fog)
    {
        var label = GridConstants.GetLabel(asset);
        if (string.IsNullOrEmpty(label)) return;

        using var font = new SKFont(SKTypeface.FromFamilyName("Cascadia Code", SKFontStyle.Bold), 8);
        using var paint = new SKPaint { Color = new SKColor(0, 0, 0, (byte)(120 * fog)), IsAntialias = true };
        canvas.DrawText(label, sx, sy + HH + 4, SKTextAlign.Center, font, paint);
    }

    public static void DrawGhostCursor(SKCanvas canvas, float sx, float sy, AssetType asset, float fog)
    {
        var colors = AssetColors[(int)asset];
        byte fillAlpha = (byte)(50 * fog);
        byte strokeAlpha = (byte)(130 * fog);

        // Top face (filled + outlined)
        using var topPath = new SKPath();
        topPath.MoveTo(sx, sy); topPath.LineTo(sx + HW, sy + HH);
        topPath.LineTo(sx, sy + 2 * HH); topPath.LineTo(sx - HW, sy + HH); topPath.Close();

        // Left face
        using var leftPath = new SKPath();
        leftPath.MoveTo(sx - HW, sy + HH); leftPath.LineTo(sx, sy + 2 * HH);
        leftPath.LineTo(sx, sy + 2 * HH + Depth); leftPath.LineTo(sx - HW, sy + HH + Depth); leftPath.Close();

        // Right face
        using var rightPath = new SKPath();
        rightPath.MoveTo(sx + HW, sy + HH); rightPath.LineTo(sx, sy + 2 * HH);
        rightPath.LineTo(sx, sy + 2 * HH + Depth); rightPath.LineTo(sx + HW, sy + HH + Depth); rightPath.Close();

        // Translucent fills
        using var topFill = new SKPaint { Color = new SKColor(colors.AR, colors.AG, colors.AB, (byte)(fillAlpha * 1.2f)), Style = SKPaintStyle.Fill, IsAntialias = true };
        using var leftFill = new SKPaint { Color = new SKColor(colors.AR, colors.AG, colors.AB, (byte)(fillAlpha * 0.7f)), Style = SKPaintStyle.Fill, IsAntialias = true };
        using var rightFill = new SKPaint { Color = new SKColor(colors.AR, colors.AG, colors.AB, (byte)(fillAlpha * 0.5f)), Style = SKPaintStyle.Fill, IsAntialias = true };

        canvas.DrawPath(rightPath, rightFill);
        canvas.DrawPath(leftPath, leftFill);
        canvas.DrawPath(topPath, topFill);

        // Wireframe outline on all faces
        using var outline = new SKPaint { Color = new SKColor(colors.AR, colors.AG, colors.AB, strokeAlpha), Style = SKPaintStyle.Stroke, StrokeWidth = 1.0f, IsAntialias = true };
        canvas.DrawPath(topPath, outline);
        canvas.DrawPath(leftPath, outline);
        canvas.DrawPath(rightPath, outline);

        // Crown highlight (top ridgeline)
        using var crown = new SKPaint { Color = new SKColor(255, 255, 255, (byte)(80 * fog)), Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f, IsAntialias = true };
        canvas.DrawLine(sx, sy, sx - HW, sy + HH, crown);
        canvas.DrawLine(sx, sy, sx + HW, sy + HH, crown);

        // Asset label on ghost cursor (larger, white)
        var label = GridConstants.GetLabel(asset);
        if (!string.IsNullOrEmpty(label))
        {
            using var labelFont = new SKFont(SKTypeface.FromFamilyName("Cascadia Code", SKFontStyle.Bold), 10);
            using var labelPaint = new SKPaint { Color = new SKColor(255, 255, 255, (byte)(180 * fog)), IsAntialias = true };
            canvas.DrawText(label, sx, sy + HH + 4, SKTextAlign.Center, labelFont, labelPaint);
        }
    }

    public static void DrawCrosshair(SKCanvas canvas, float sx, float sy, float width, float height)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 13), StrokeWidth = 0.5f,
            Style = SKPaintStyle.Stroke, IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash([6f, 8f], 0)
        };
        canvas.DrawLine(sx, 0, sx, height, paint);
        canvas.DrawLine(0, sy + HH, width, sy + HH, paint);
    }

    public static void DrawStackingPreview(SKCanvas canvas, float originX, float originY,
        int gx, int gz, int activeLayer, int rotation = 0)
    {
        if (activeLayer <= 0) return;

        using var guidePaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 15), StrokeWidth = 0.5f,
            Style = SKPaintStyle.Stroke, IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash([2f, 4f], 0)
        };

        var (sx0, sy0) = IsoMath.GridToScreen(gx, gz, 0, originX, originY, rotation);
        var (sxA, syA) = IsoMath.GridToScreen(gx, gz, activeLayer, originX, originY, rotation);

        canvas.DrawLine(sx0 - HW, sy0 + HH, sxA - HW, syA + HH, guidePaint);
        canvas.DrawLine(sx0 + HW, sy0 + HH, sxA + HW, syA + HH, guidePaint);
        canvas.DrawLine(sx0, sy0 + 2 * HH, sxA, syA + 2 * HH, guidePaint);

        using var wirePaint = new SKPaint
        {
            StrokeWidth = 0.5f, Style = SKPaintStyle.Stroke, IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash([3f, 5f], 0)
        };

        for (int y = 0; y < activeLayer; y++)
        {
            float alpha = 0.025f + (y / (float)activeLayer) * 0.04f;
            wirePaint.Color = new SKColor(255, 255, 255, (byte)(255 * alpha));
            var (wx, wy) = IsoMath.GridToScreen(gx, gz, y, originX, originY, rotation);
            using var diamond = MakeDiamond(wx, wy + HH);
            canvas.DrawPath(diamond, wirePaint);
        }
    }

    #region Ambient Occlusion

    public static float ComputeAO_Top(IImmutableDictionary<(int X, int Y, int Z), AssetType> cells, int x, int y, int z)
    {
        if (cells.ContainsKey((x, y + 1, z))) return 0.35f;
        float occ = 0f;
        if (cells.ContainsKey((x - 1, y + 1, z))) occ += 0.12f;
        if (cells.ContainsKey((x + 1, y + 1, z))) occ += 0.12f;
        if (cells.ContainsKey((x, y + 1, z - 1))) occ += 0.12f;
        if (cells.ContainsKey((x, y + 1, z + 1))) occ += 0.12f;
        if (cells.ContainsKey((x - 1, y, z))) occ += 0.06f;
        if (cells.ContainsKey((x, y, z - 1))) occ += 0.06f;
        return MathF.Max(0f, 1f - occ);
    }

    public static float ComputeAO_Left(IImmutableDictionary<(int X, int Y, int Z), AssetType> cells, int x, int y, int z)
    {
        if (cells.ContainsKey((x - 1, y, z))) return 0.30f;
        float occ = 0f;
        if (cells.ContainsKey((x - 1, y + 1, z))) occ += 0.15f;
        if (cells.ContainsKey((x - 1, y - 1, z))) occ += 0.15f;
        if (cells.ContainsKey((x, y, z + 1))) occ += 0.08f;
        if (cells.ContainsKey((x, y - 1, z))) occ += 0.06f;
        return MathF.Max(0f, 1f - occ);
    }

    public static float ComputeAO_Right(IImmutableDictionary<(int X, int Y, int Z), AssetType> cells, int x, int y, int z)
    {
        if (cells.ContainsKey((x, y, z + 1))) return 0.30f;
        float occ = 0f;
        if (cells.ContainsKey((x, y + 1, z + 1))) occ += 0.15f;
        if (cells.ContainsKey((x, y - 1, z + 1))) occ += 0.15f;
        if (cells.ContainsKey((x + 1, y, z))) occ += 0.08f;
        if (cells.ContainsKey((x, y - 1, z))) occ += 0.06f;
        return MathF.Max(0f, 1f - occ);
    }

    #endregion

    private static SKPath MakeDiamond(float cx, float cy)
    {
        var path = new SKPath();
        path.MoveTo(cx, cy - HH); path.LineTo(cx + HW, cy);
        path.LineTo(cx, cy + HH); path.LineTo(cx - HW, cy); path.Close();
        return path;
    }
}
