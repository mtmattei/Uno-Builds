using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;

namespace VoxelWarehouse.Presentation;

public sealed partial class OrdersPage : Page
{
    /// <summary>Fires when a PO row is selected, Shell listens to update right panel.</summary>
    public static event Action<PurchaseOrder?>? OrderSelected;

    private static readonly SolidColorBrush AccentBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xD4, 0xAA));
    private static readonly SolidColorBrush AmberBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xE8, 0xA2, 0x30));
    private static readonly SolidColorBrush RedBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xE8, 0x48, 0x55));
    private static readonly SolidColorBrush BlueBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x4A, 0x9E, 0xF5));
    private static readonly SolidColorBrush RaisedBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x18, 0x1B, 0x22));
    private static readonly SolidColorBrush BorderBrush_ = new(Windows.UI.Color.FromArgb(0xFF, 0x1F, 0x23, 0x2D));
    private static readonly SolidColorBrush T1Brush = new(Windows.UI.Color.FromArgb(0xFF, 0xE8, 0xE6, 0xE1));
    private static readonly SolidColorBrush T2Brush = new(Windows.UI.Color.FromArgb(0xFF, 0x93, 0x98, 0xA3));
    private static readonly SolidColorBrush T3Brush = new(Windows.UI.Color.FromArgb(0xFF, 0x5D, 0x63, 0x70));
    private static readonly SolidColorBrush T4Brush = new(Windows.UI.Color.FromArgb(0xFF, 0x3A, 0x3F, 0x4B));
    private static readonly SolidColorBrush TransparentBrush = new(Microsoft.UI.Colors.Transparent);
    private static readonly FontFamily MonoFont = new("Cascadia Code, Cascadia Mono, Consolas, monospace");

    private Border? _selectedRow;
    private PurchaseOrder? _selectedOrder;

    public OrdersPage()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BuildTable();
    }

    private void BuildTable()
    {
        OrderRows.Children.Clear();
        var orders = SeedDataService.GetPurchaseOrders();

        foreach (var po in orders)
        {
            var row = CreateRow(po);
            OrderRows.Children.Add(row);
        }
    }

    private Border CreateRow(PurchaseOrder po)
    {
        var row = new Border
        {
            Background = TransparentBrush,
            BorderBrush = BorderBrush_,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(16, 10),
            Tag = po
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });

        // PO#
        var poId = MakeText(po.Id, 9, FontWeights.Medium, T1Brush);
        Grid.SetColumn(poId, 0);

        // Vendor
        var vendor = MakeText(po.VendorName, 8, FontWeights.Light, T2Brush);
        Grid.SetColumn(vendor, 1);

        // Amount
        var amount = MakeText($"${po.Amount:N0}", 9, FontWeights.Medium, T1Brush);
        amount.HorizontalAlignment = HorizontalAlignment.Right;
        Grid.SetColumn(amount, 2);

        // Status badge
        var (statusColor, statusBg) = po.Status switch
        {
            POStatus.Pending => (AmberBrush, DimBrush(AmberBrush)),
            POStatus.Approved => (AccentBrush, DimBrush(AccentBrush)),
            POStatus.Review => (BlueBrush, DimBrush(BlueBrush)),
            POStatus.Flagged => (RedBrush, DimBrush(RedBrush)),
            _ => (T3Brush, TransparentBrush)
        };
        var statusBadge = MakeBadge(po.Status.ToString().ToUpper(), statusColor, statusBg);
        statusBadge.HorizontalAlignment = HorizontalAlignment.Center;
        Grid.SetColumn(statusBadge, 3);

        // Risk badge
        var riskColor = po.Risk switch
        {
            RiskLevel.High => RedBrush,
            RiskLevel.Med => AmberBrush,
            _ => T4Brush
        };
        var risk = MakeText(po.Risk.ToString().ToUpper(), 7, FontWeights.Medium, riskColor);
        risk.HorizontalAlignment = HorizontalAlignment.Center;
        Grid.SetColumn(risk, 4);

        // AI chip
        UIElement aiElement;
        if (po.AiAlertType is not null)
        {
            var aiColor = po.AiAlertType switch
            {
                "alert" => RedBrush,
                "warn" => AmberBrush,
                _ => BlueBrush
            };
            aiElement = MakeBadge(po.AiAlertText ?? po.AiAlertType.ToUpper(), aiColor, DimBrush(aiColor));
        }
        else
        {
            aiElement = MakeText("\u2014", 8, FontWeights.Light, T4Brush);
        }
        if (aiElement is FrameworkElement fe) fe.HorizontalAlignment = HorizontalAlignment.Center;
        Grid.SetColumn(aiElement, 5);

        // Submitted
        var submitted = MakeText(po.SubmittedAgo, 8, FontWeights.Light, T4Brush);
        submitted.HorizontalAlignment = HorizontalAlignment.Right;
        Grid.SetColumn(submitted, 6);

        grid.Children.Add(poId);
        grid.Children.Add(vendor);
        grid.Children.Add(amount);
        grid.Children.Add(statusBadge);
        grid.Children.Add(risk);
        grid.Children.Add(aiElement);
        grid.Children.Add(submitted);

        row.Child = grid;
        row.PointerPressed += (s, _) => SelectRow(row, po);
        return row;
    }

    private void SelectRow(Border row, PurchaseOrder po)
    {
        if (_selectedRow is not null)
            _selectedRow.Background = TransparentBrush;

        _selectedRow = row;
        _selectedOrder = po;
        row.Background = RaisedBrush;

        OrderSelected?.Invoke(po);
    }

    private static TextBlock MakeText(string text, double size, Windows.UI.Text.FontWeight weight, SolidColorBrush color) =>
        new()
        {
            Text = text, FontFamily = MonoFont, FontSize = size,
            FontWeight = weight, CharacterSpacing = 60,
            Foreground = color, VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

    private static Border MakeBadge(string text, SolidColorBrush fg, SolidColorBrush bg) =>
        new()
        {
            Background = bg, CornerRadius = new CornerRadius(3),
            Padding = new Thickness(6, 2),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = text, FontFamily = MonoFont, FontSize = 7,
                FontWeight = FontWeights.Medium, CharacterSpacing = 100,
                Foreground = fg, TextTrimming = TextTrimming.CharacterEllipsis
            }
        };

    private static SolidColorBrush DimBrush(SolidColorBrush source) =>
        new(Windows.UI.Color.FromArgb(0x1A, source.Color.R, source.Color.G, source.Color.B));
}
