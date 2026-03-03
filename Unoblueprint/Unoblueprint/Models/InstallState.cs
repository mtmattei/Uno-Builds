namespace Unoblueprint.Models;

public enum InstallState
{
    NotInstalled,
    Installing,
    Installed
}

public class InstallStateChangedEventArgs : EventArgs
{
    public InstallState NewState { get; set; }
    public InstallState OldState { get; set; }
    public bool IsInstalled => NewState == InstallState.Installed;
}
