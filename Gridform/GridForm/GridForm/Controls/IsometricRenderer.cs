using GridForm.Helpers;
using SkiaSharp;

namespace GridForm.Controls;

public static class IsometricRenderer
{
	private static readonly SKColor BackgroundColor = new(0x13, 0x15, 0x19);

	private static readonly Dictionary<AssetType, SKColor> AssetColors = new()
	{
		[AssetType.Pallet] = new SKColor(0x5F, 0xB8, 0x9E),
		[AssetType.Rack] = new SKColor(0xD4, 0x95, 0x6A),
		[AssetType.Container] = new SKColor(0x6B, 0x9F, 0xC8),
		[AssetType.Equipment] = new SKColor(0xD4, 0xA6, 0x4E),
		[AssetType.Aisle] = new SKColor(0x70, 0x6D, 0x64),
	};

	private static readonly Dictionary<ZoneType, SKColor> ZoneColors = new()
	{
		[ZoneType.Receiving] = new SKColor(0x6B, 0x9F, 0xC8, 0x30),
		[ZoneType.Storage] = new SKColor(0x5F, 0xB8, 0x9E, 0x25),
		[ZoneType.Staging] = new SKColor(0xD4, 0xA6, 0x4E, 0x30),
		[ZoneType.Shipping] = new SKColor(0xD4, 0x95, 0x6A, 0x30),
	};

	private static readonly Dictionary<AssetType, string> AssetLabels = new()
	{
		[AssetType.Pallet] = "P",
		[AssetType.Rack] = "R",
		[AssetType.Container] = "C",
		[AssetType.Equipment] = "E",
		[AssetType.Aisle] = "\u00B7",
	};

	// --- Pooled SKPaint objects (reused every frame) ---

	private static readonly SKPaint ZoneFillPaint = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Fill
	};

	private static readonly SKPaint ZoneStrokePaint = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Stroke,
		StrokeWidth = 1
	};

	private static readonly SKPaint GridLinePaint = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Stroke,
		StrokeWidth = 1
	};

	private static readonly SKPaint GroundShadowPaint = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Fill,
		Color = new SKColor(0, 0, 0, 40)
	};

	private static readonly SKPaint TopFacePaint = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Fill
	};

	private static readonly SKPaint LeftFacePaint = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Fill
	};

	private static readonly SKPaint RightFacePaint = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Fill
	};

	private static readonly SKPaint EdgeHighlightPaint = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Stroke,
		StrokeWidth = 1
	};

	private static readonly SKPaint TextLabelPaint = new()
	{
		IsAntialias = true
	};

	private static readonly SKFont LabelFont = new(SKTypeface.Default, 8);

	public static void Draw(
		SKCanvas canvas,
		SKImageInfo info,
		Dictionary<VoxelKey, AssetType>? voxels,
		Dictionary<(int, int), ZoneType>? zones,
		int currentLayer,
		(int GX, int GZ)? cursor,
		AssetType currentAsset,
		WarehouseMode mode)
	{
		canvas.Clear(BackgroundColor);

		var ox = info.Width / 2f;
		var oy = info.Height * 0.38f;
		var gridW = WarehouseState.GridWidth;
		var gridD = WarehouseState.GridDepth;

		DrawZoneTiles(canvas, zones, ox, oy, gridW, gridD);
		DrawGrid(canvas, 0, ox, oy, gridW, gridD, 0.15f);

		if (currentLayer > 0)
			DrawGrid(canvas, currentLayer, ox, oy, gridW, gridD, 0.08f);

		if (voxels != null)
		{
			DrawGroundShadows(canvas, voxels, ox, oy);

			var sorted = voxels.OrderBy(v => v.Key.Y).ThenBy(v => v.Key.X + v.Key.Z).ToList();
			foreach (var (key, asset) in sorted)
			{
				var fog = 1f - (float)(key.X + key.Z) / (gridW + gridD) * 0.35f;
				DrawVoxel(canvas, key.X, key.Z, key.Y, asset, ox, oy, fog, false);
			}
		}

		if (cursor is { } c && mode == WarehouseMode.Build)
		{
			DrawGhostCursor(canvas, c.GX, c.GZ, currentLayer, currentAsset, ox, oy);
		}
	}

	private static void DrawZoneTiles(SKCanvas canvas, Dictionary<(int, int), ZoneType>? zones, float ox, float oy, int gridW, int gridD)
	{
		if (zones == null) return;

		foreach (var (pos, zone) in zones)
		{
			if (!ZoneColors.TryGetValue(zone, out var color)) continue;

			var (sx, sy) = IsometricMath.ToIso(pos.Item1, pos.Item2, 0, ox, oy);
			var path = CreateDiamond(sx, sy);

			ZoneFillPaint.Color = color;
			canvas.DrawPath(path, ZoneFillPaint);

			ZoneStrokePaint.Color = color.WithAlpha((byte)(color.Alpha + 20));
			canvas.DrawPath(path, ZoneStrokePaint);
		}
	}

	private static void DrawGrid(SKCanvas canvas, int layer, float ox, float oy, int gridW, int gridD, float alpha)
	{
		GridLinePaint.Color = new SKColor(0xEA, 0xE7, 0xDF, (byte)(alpha * 255));

		for (var x = 0; x <= gridW; x++)
		{
			var (sx1, sy1) = IsometricMath.ToIso(x, 0, layer, ox, oy);
			var (sx2, sy2) = IsometricMath.ToIso(x, gridD, layer, ox, oy);
			canvas.DrawLine(sx1, sy1, sx2, sy2, GridLinePaint);
		}

		for (var z = 0; z <= gridD; z++)
		{
			var (sx1, sy1) = IsometricMath.ToIso(0, z, layer, ox, oy);
			var (sx2, sy2) = IsometricMath.ToIso(gridW, z, layer, ox, oy);
			canvas.DrawLine(sx1, sy1, sx2, sy2, GridLinePaint);
		}
	}

	private static void DrawGroundShadows(SKCanvas canvas, Dictionary<VoxelKey, AssetType> voxels, float ox, float oy)
	{
		foreach (var key in voxels.Keys.Where(k => k.Y > 0))
		{
			var (sx, sy) = IsometricMath.ToIso(key.X, key.Z, 0, ox, oy);
			canvas.DrawPath(CreateDiamond(sx, sy), GroundShadowPaint);
		}
	}

	private static void DrawVoxel(SKCanvas canvas, int gx, int gz, int layer, AssetType asset, float ox, float oy, float fog, bool ghost)
	{
		if (!AssetColors.TryGetValue(asset, out var baseColor)) return;

		var (sx, sy) = IsometricMath.ToIso(gx, gz, layer, ox, oy);
		var tw = IsometricMath.TileWidth / 2f;
		var th = IsometricMath.TileHeight / 2f;
		var vh = IsometricMath.VoxelHeight;

		var alpha = ghost ? (byte)45 : (byte)(255 * fog);
		var color = baseColor.WithAlpha(alpha);

		// Top face
		TopFacePaint.Color = color;
		var topPath = new SKPath();
		topPath.MoveTo(sx, sy - vh);
		topPath.LineTo(sx + tw, sy + th - vh);
		topPath.LineTo(sx, sy + th * 2 - vh);
		topPath.LineTo(sx - tw, sy + th - vh);
		topPath.Close();
		canvas.DrawPath(topPath, TopFacePaint);

		// Left face (darker)
		LeftFacePaint.Color = DarkenColor(color, 0.7f);
		var leftPath = new SKPath();
		leftPath.MoveTo(sx - tw, sy + th - vh);
		leftPath.LineTo(sx, sy + th * 2 - vh);
		leftPath.LineTo(sx, sy + th * 2);
		leftPath.LineTo(sx - tw, sy + th);
		leftPath.Close();
		canvas.DrawPath(leftPath, LeftFacePaint);

		// Right face (slightly darker)
		RightFacePaint.Color = DarkenColor(color, 0.85f);
		var rightPath = new SKPath();
		rightPath.MoveTo(sx + tw, sy + th - vh);
		rightPath.LineTo(sx, sy + th * 2 - vh);
		rightPath.LineTo(sx, sy + th * 2);
		rightPath.LineTo(sx + tw, sy + th);
		rightPath.Close();
		canvas.DrawPath(rightPath, RightFacePaint);

		// Edge highlights on top face
		EdgeHighlightPaint.Color = new SKColor(255, 255, 255, (byte)(45 * fog));
		canvas.DrawPath(topPath, EdgeHighlightPaint);

		// Asset label on top face
		if (!ghost && AssetLabels.TryGetValue(asset, out var label))
		{
			TextLabelPaint.Color = new SKColor(0, 0, 0, 76);
			canvas.DrawText(label, sx, sy + th - vh + 4, SKTextAlign.Center, LabelFont, TextLabelPaint);
		}
	}

	private static void DrawGhostCursor(SKCanvas canvas, int gx, int gz, int layer, AssetType asset, float ox, float oy)
	{
		DrawVoxel(canvas, gx, gz, layer, asset, ox, oy, 1f, true);
	}

	private static SKPath CreateDiamond(float cx, float cy)
	{
		var tw = IsometricMath.TileWidth / 2f;
		var th = IsometricMath.TileHeight / 2f;
		var path = new SKPath();
		path.MoveTo(cx, cy);
		path.LineTo(cx + tw, cy + th);
		path.LineTo(cx, cy + th * 2);
		path.LineTo(cx - tw, cy + th);
		path.Close();
		return path;
	}

	private static SKColor DarkenColor(SKColor color, float factor)
	{
		return new SKColor(
			(byte)(color.Red * factor),
			(byte)(color.Green * factor),
			(byte)(color.Blue * factor),
			color.Alpha);
	}
}
