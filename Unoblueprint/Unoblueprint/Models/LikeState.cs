namespace Unoblueprint.Models;

public enum PurchaseState
{
    Normal,
    Purchased
}

public class PurchaseStateChangedEventArgs : EventArgs
{
    public bool IsPurchased { get; set; }
    public PurchaseState NewState { get; set; }
    public PurchaseState OldState { get; set; }
}

// Keep old names for backward compatibility
public enum LikeState
{
    Normal,
    Liked
}

public class LikeStateChangedEventArgs : EventArgs
{
    public bool IsLiked { get; set; }
    public LikeState NewState { get; set; }
    public LikeState OldState { get; set; }
}
