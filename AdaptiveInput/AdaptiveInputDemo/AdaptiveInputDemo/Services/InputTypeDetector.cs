using System.Text.RegularExpressions;

namespace AdaptiveInputDemo.Services;

/// <summary>
/// Represents the detected type of input.
/// </summary>
public enum DetectedInputType
{
    None,
    Mention,
    Color,
    Tag,
    NumberRange,
    Date,
    Url,
    Email
}

/// <summary>
/// Result of input type detection.
/// </summary>
public record InputDetectionResult(DetectedInputType Type, string Value, string? ParsedValue = null);

/// <summary>
/// Service for detecting semantic input types from user text.
/// Follows priority order: Mention > Hex Color > Color Keyword > Tag > Number Range > Date > URL > Email
/// </summary>
public partial class InputTypeDetector
{
    // Priority 1: Mention - @ followed by characters
    [GeneratedRegex(@"^@.+", RegexOptions.IgnoreCase)]
    private static partial Regex MentionPattern();

    // Priority 2: Hex Color - # followed by 1-6 hex digits only
    [GeneratedRegex(@"^#[0-9a-fA-F]{1,6}$")]
    private static partial Regex HexColorPattern();

    // Priority 3: Color Keywords
    private static readonly HashSet<string> ColorKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "red", "blue", "green", "yellow", "purple", "orange", "pink", "cyan",
        "black", "white", "gray", "grey", "navy", "teal", "coral", "salmon",
        "gold", "silver", "maroon", "olive", "lime", "aqua", "fuchsia",
        "indigo", "violet"
    };

    [GeneratedRegex(@"^(rgb|hsl)\s*\(", RegexOptions.IgnoreCase)]
    private static partial Regex ColorFunctionPattern();

    // Priority 4: Tag - # followed by word containing at least one non-hex letter (g-z) or underscore
    [GeneratedRegex(@"^#[a-zA-Z_][a-zA-Z0-9_-]*$")]
    private static partial Regex TagPatternSimple();

    [GeneratedRegex(@"^#[0-9a-fA-F]*[g-zG-Z_][a-zA-Z0-9_-]*$")]
    private static partial Regex TagPatternMixed();

    // Priority 5: Number Range
    [GeneratedRegex(@"^\d+\s*(-|–|to|through)\s*\d+$", RegexOptions.IgnoreCase)]
    private static partial Regex NumberRangePattern();

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex StandaloneNumberPattern();

    // Priority 6: Date patterns
    [GeneratedRegex(@"^(today|tomorrow|yesterday|next\s+|last\s+|this\s+)", RegexOptions.IgnoreCase)]
    private static partial Regex DateNaturalPattern();

    [GeneratedRegex(@"^(mon|tue|wed|thu|fri|sat|sun)(day)?$", RegexOptions.IgnoreCase)]
    private static partial Regex DateDayPattern();

    [GeneratedRegex(@"^(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)", RegexOptions.IgnoreCase)]
    private static partial Regex DateMonthPattern();

    [GeneratedRegex(@"^\d{1,2}[\/\-]\d{1,2}([\/\-]\d{2,4})?$")]
    private static partial Regex DateFormatPattern();

    // Priority 7: URL
    [GeneratedRegex(@"^(https?://|www\.)", RegexOptions.IgnoreCase)]
    private static partial Regex UrlPattern();

    // Priority 8: Email
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailPattern();

    /// <summary>
    /// Detects the semantic type of the input text.
    /// </summary>
    public InputDetectionResult Detect(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new InputDetectionResult(DetectedInputType.None, input ?? string.Empty);
        }

        var trimmed = input.Trim();

        // Priority 1: Mention
        if (MentionPattern().IsMatch(trimmed))
        {
            return new InputDetectionResult(DetectedInputType.Mention, trimmed, trimmed[1..]);
        }

        // Priority 2: Hex Color (must be # followed by ONLY hex digits)
        if (trimmed.StartsWith('#') && HexColorPattern().IsMatch(trimmed))
        {
            return new InputDetectionResult(DetectedInputType.Color, trimmed, NormalizeHexColor(trimmed));
        }

        // Priority 3: Color Keyword
        if (ColorKeywords.Contains(trimmed) || ColorFunctionPattern().IsMatch(trimmed))
        {
            return new InputDetectionResult(DetectedInputType.Color, trimmed);
        }

        // Priority 4: Tag (# followed by word with non-hex characters)
        if (trimmed.StartsWith('#') && trimmed.Length > 1)
        {
            var afterHash = trimmed[1..];
            // Check if it contains any non-hex character (g-z, G-Z, or _)
            if (TagPatternSimple().IsMatch(trimmed) || TagPatternMixed().IsMatch(trimmed))
            {
                // Additional check: ensure it's not a valid hex color
                if (!HexColorPattern().IsMatch(trimmed))
                {
                    return new InputDetectionResult(DetectedInputType.Tag, trimmed, afterHash);
                }
            }
        }

        // Priority 5: Number Range
        if (NumberRangePattern().IsMatch(trimmed))
        {
            return new InputDetectionResult(DetectedInputType.NumberRange, trimmed, ParseNumberRange(trimmed));
        }

        if (StandaloneNumberPattern().IsMatch(trimmed))
        {
            return new InputDetectionResult(DetectedInputType.NumberRange, trimmed, trimmed);
        }

        // Priority 6: Date
        if (DateNaturalPattern().IsMatch(trimmed) ||
            DateDayPattern().IsMatch(trimmed) ||
            DateMonthPattern().IsMatch(trimmed) ||
            DateFormatPattern().IsMatch(trimmed))
        {
            return new InputDetectionResult(DetectedInputType.Date, trimmed);
        }

        // Priority 7: URL
        if (UrlPattern().IsMatch(trimmed))
        {
            return new InputDetectionResult(DetectedInputType.Url, trimmed);
        }

        // Priority 8: Email
        if (EmailPattern().IsMatch(trimmed))
        {
            return new InputDetectionResult(DetectedInputType.Email, trimmed);
        }

        return new InputDetectionResult(DetectedInputType.None, trimmed);
    }

    private static string NormalizeHexColor(string hex)
    {
        // Normalize 3-digit hex to 6-digit
        if (hex.Length == 4) // #RGB
        {
            return $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}".ToUpperInvariant();
        }
        return hex.ToUpperInvariant();
    }

    private static string ParseNumberRange(string input)
    {
        // Extract min and max from range patterns
        var match = Regex.Match(input, @"^(\d+)\s*(?:-|–|to|through)\s*(\d+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"{match.Groups[1].Value}-{match.Groups[2].Value}";
        }
        return input;
    }

    /// <summary>
    /// Gets display information for a detected type.
    /// </summary>
    public static (string Icon, string Label, string ColorKey) GetTypeDisplayInfo(DetectedInputType type)
    {
        return type switch
        {
            DetectedInputType.Mention => ("\uE77B", "Mention", "MentionBadgeColor"),      // Person icon
            DetectedInputType.Color => ("\uE790", "Color", "ColorBadgeColor"),            // Color icon
            DetectedInputType.Tag => ("\uE8EC", "Tag", "TagBadgeColor"),                  // Tag icon
            DetectedInputType.NumberRange => ("\uE9E9", "Range", "RangeBadgeColor"),      // Slider icon
            DetectedInputType.Date => ("\uE787", "Date", "DateBadgeColor"),               // Calendar icon
            DetectedInputType.Url => ("\uE71B", "Link", "UrlBadgeColor"),                 // Link icon
            DetectedInputType.Email => ("\uE715", "Email", "EmailBadgeColor"),            // Mail icon
            _ => ("\uE734", "Auto", "AutoBadgeColor")                                      // Sparkle/asterisk icon
        };
    }
}
