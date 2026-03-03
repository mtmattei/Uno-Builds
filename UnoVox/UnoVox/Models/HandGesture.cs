namespace UnoVox.Models;

public enum GestureType
{
    None,
    OpenPalm,
    ClosedFist,
    Pinch,
    Point,
    ThumbsUp
}

public record HandGesture(GestureType Type, float Confidence)
{
    public static readonly HandGesture None = new(GestureType.None, 0f);
}
