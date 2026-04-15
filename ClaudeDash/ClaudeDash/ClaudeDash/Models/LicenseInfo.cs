namespace ClaudeDash.Models;

public record LicenseInfo(
    string Tier = "unknown",
    string Email = "",
    string ExpiryDate = "",
    bool IsExpired = false,
    bool IsSignedIn = false);
