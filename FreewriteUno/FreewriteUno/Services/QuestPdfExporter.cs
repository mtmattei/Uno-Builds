using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FreewriteUno.Services;

public sealed class QuestPdfExporter : IPdfExporter
{
    static QuestPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> RenderAsync(string content, string title, CancellationToken ct = default)
    {
        var trimmed = (content ?? string.Empty).TrimStart('\n', '\r').Trim();
        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Freewrite entry" : title;

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(72);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(t => t.FontSize(12).LineHeight(1.6f).FontColor(Colors.Grey.Darken4));

                page.Header()
                    .PaddingBottom(16)
                    .Text(safeTitle)
                    .FontSize(14)
                    .SemiBold()
                    .FontColor(Colors.Grey.Darken3);

                page.Content()
                    .Text(trimmed)
                    .FontSize(12);

                page.Footer()
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                        t.Span(" / ").FontSize(9).FontColor(Colors.Grey.Medium);
                        t.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                    });
            });
        }).GeneratePdf();

        return Task.FromResult(bytes);
    }
}
