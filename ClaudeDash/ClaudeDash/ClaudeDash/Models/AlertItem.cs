namespace ClaudeDash.Models;

public enum AlertType { Warning, Error, Info }

public record AlertItem(
    AlertType Type = AlertType.Info,
    string Message = "",
    string NavigationTarget = "");
