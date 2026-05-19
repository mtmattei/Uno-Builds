namespace Composer.Models;

public enum RuntimeKind
{
    Net10,
    Net9,
}

public static class RuntimeKindExtensions
{
    public static string DisplayName(this RuntimeKind kind) => kind switch
    {
        RuntimeKind.Net10 => ".NET 10",
        RuntimeKind.Net9  => ".NET 9",
        _                 => kind.ToString(),
    };

    public static string ShortGlyph(this RuntimeKind kind) => kind switch
    {
        RuntimeKind.Net10 => "10",
        RuntimeKind.Net9  => "9",
        _                 => string.Empty,
    };
}
