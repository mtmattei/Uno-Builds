namespace ClaudeDash.Models.Remediation;

public class FixResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static FixResult Ok(string message) => new() { Success = true, Message = message };
    public static FixResult Fail(string message) => new() { Success = false, Message = message };
}
