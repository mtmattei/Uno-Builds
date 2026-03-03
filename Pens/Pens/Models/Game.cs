namespace Pens.Models;

public partial record Game(
    string Opponent,
    string Date,
    string Time,
    string Rink,
    bool IsNext = false,
    bool IsHome = true);

public partial record GameResult(
    string HomeTeam,
    int HomeScore,
    string AwayTeam,
    int AwayScore,
    bool IsPenguinsGame = false)
{
    public bool PenguinsWon => IsPenguinsGame &&
        ((HomeTeam == "Penguins" && HomeScore > AwayScore) ||
         (AwayTeam == "Penguins" && AwayScore > HomeScore));
}
