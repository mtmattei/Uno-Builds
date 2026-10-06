namespace FreewriteUno.Models;

public sealed partial record Entry(
    Guid Id,
    DateTimeOffset CreatedAt,
    string FileName,
    string Preview)
{
    public bool IsEmpty => string.IsNullOrWhiteSpace(Preview);
}
