using GridForm.Helpers;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace GridForm.Controls;

public class IsometricCanvas : SKCanvasElement
{
	private Dictionary<VoxelKey, AssetType>? _voxelGrid;
	private Dictionary<(int, int), ZoneType>? _zoneGrid;
	private int _currentLayer;
	private AssetType _currentAsset = AssetType.Pallet;
	private WarehouseTool _toolMode = WarehouseTool.Place;
	private WarehouseMode _buildMode = WarehouseMode.Build;
	private (int GX, int GZ)? _cursorPos;

	public Dictionary<VoxelKey, AssetType>? VoxelGrid
	{
		get => _voxelGrid;
		set { _voxelGrid = value; Invalidate(); }
	}

	public Dictionary<(int, int), ZoneType>? ZoneGrid
	{
		get => _zoneGrid;
		set { _zoneGrid = value; Invalidate(); }
	}

	public int CurrentLayer
	{
		get => _currentLayer;
		set { _currentLayer = value; Invalidate(); }
	}

	public AssetType CurrentAsset
	{
		get => _currentAsset;
		set { _currentAsset = value; Invalidate(); }
	}

	public WarehouseTool ToolMode
	{
		get => _toolMode;
		set { _toolMode = value; }
	}

	public WarehouseMode BuildMode
	{
		get => _buildMode;
		set { _buildMode = value; }
	}

	public event EventHandler<VoxelClickEventArgs>? VoxelClicked;
	public event EventHandler<(int GX, int GZ)>? CursorMoved;

	public IsometricCanvas()
	{
		PointerMoved += OnPointerMoved;
		PointerPressed += OnPointerPressed;
	}

	protected override void RenderOverride(SKCanvas canvas, Size area)
	{
		var info = new SKImageInfo((int)area.Width, (int)area.Height);
		IsometricRenderer.Draw(canvas, info, _voxelGrid, _zoneGrid, _currentLayer, _cursorPos, _currentAsset, _buildMode);
	}

	private void OnPointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(this);
		var ox = (float)ActualWidth / 2f;
		var oy = (float)(ActualHeight * 0.38);

		var (gx, gz) = IsometricMath.FromIso((float)point.Position.X, (float)point.Position.Y, _currentLayer, ox, oy);

		if (IsometricMath.InBounds(gx, gz, WarehouseState.GridWidth, WarehouseState.GridDepth))
		{
			_cursorPos = (gx, gz);
			CursorMoved?.Invoke(this, (gx, gz));
		}
		else
		{
			_cursorPos = null;
		}

		Invalidate();
	}

	private void OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		if (_cursorPos is { } pos)
		{
			var isRightButton = e.GetCurrentPoint(this).Properties.IsRightButtonPressed;
			var tool = isRightButton ? WarehouseTool.Erase : _toolMode;
			VoxelClicked?.Invoke(this, new VoxelClickEventArgs(pos.GX, pos.GZ, _currentLayer, tool, _buildMode, _currentAsset));
		}
	}
}

public record VoxelClickEventArgs(int GX, int GZ, int Layer, WarehouseTool Tool, WarehouseMode Mode, AssetType Asset);
