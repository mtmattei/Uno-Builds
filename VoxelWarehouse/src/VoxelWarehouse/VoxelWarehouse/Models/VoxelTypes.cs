namespace VoxelWarehouse.Models;

public enum AssetType
{
    Pallet,
    Rack,
    Container,
    Equipment,
    Aisle
}

public enum ZoneType
{
    None,
    Receiving,
    Storage,
    Staging,
    Shipping
}

public enum ToolMode
{
    Place,
    Erase
}

public enum EditorMode
{
    Build,
    Zone
}

public enum GestureType
{
    None,
    Open,
    Pinch,
    Point,
    Fist
}

public enum HandLandmarkId
{
    Wrist = 0,
    ThumbCmc = 1, ThumbMcp = 2, ThumbIp = 3, ThumbTip = 4,
    IndexMcp = 5, IndexPip = 6, IndexDip = 7, IndexTip = 8,
    MiddleMcp = 9, MiddlePip = 10, MiddleDip = 11, MiddleTip = 12,
    RingMcp = 13, RingPip = 14, RingDip = 15, RingTip = 16,
    PinkyMcp = 17, PinkyPip = 18, PinkyDip = 19, PinkyTip = 20,
}
