namespace AdTokensIDE.Models;

public partial record Advertisement(
    string Id,
    string ProductName,
    string Tagline,
    string Disclaimer,
    string IconGlyph,
    int SponsoredTokens
);
