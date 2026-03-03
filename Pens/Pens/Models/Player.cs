namespace Pens.Models;

public partial record Player(
    int Id,
    string Name,
    int Number,
    string Position,
    PlayerStatus Status);

public enum PlayerStatus
{
    In,
    Out,
    Pending
}
