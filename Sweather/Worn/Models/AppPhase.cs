namespace Worn.Models;

public enum AppPhase
{
    Init,
    RequestingGeo,
    FetchingWeather,
    Processing,
    Ready,
    Error
}

public enum TierId
{
    Scorcher,
    Hot,
    Warm,
    Pleasant,
    LightJacket,
    SweaterWeather,
    CoatUp,
    Bundle,
    Survival
}

public enum NecessityLevel
{
    Survival,
    NonNegotiable,
    MustHave,
    SmartPick,
    GoTo,
    SafeBet,
    EasyPick,
    NiceTouch,
    Maybe
}
