namespace GridForm.Helpers;

public static class IsometricMath
{
	public const int TileWidth = 32;
	public const int TileHeight = 16;
	public const int VoxelHeight = 20;

	public static (float ScreenX, float ScreenY) ToIso(int gridX, int gridZ, int layer, float originX, float originY)
	{
		var sx = originX + (gridX - gridZ) * (TileWidth / 2f);
		var sy = originY + (gridX + gridZ) * (TileHeight / 2f) - layer * VoxelHeight;
		return (sx, sy);
	}

	public static (int GridX, int GridZ) FromIso(float screenX, float screenY, int layer, float originX, float originY)
	{
		var adjustedY = screenY + layer * VoxelHeight;
		var mx = screenX - originX;
		var my = adjustedY - originY;

		var gx = (int)Math.Round((mx / (TileWidth / 2f) + my / (TileHeight / 2f)) / 2f);
		var gz = (int)Math.Round((my / (TileHeight / 2f) - mx / (TileWidth / 2f)) / 2f);

		return (gx, gz);
	}

	public static bool InBounds(int gx, int gz, int width, int depth)
		=> gx >= 0 && gx < width && gz >= 0 && gz < depth;
}
