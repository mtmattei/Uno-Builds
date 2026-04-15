namespace ClaudeDash.Controls;

public sealed partial class CostBarRow : UserControl
{
    // Accent-tinted bar colors per model row
    private static readonly Color[] BarColors =
    {
        ColorHelper.FromArgb(255, 167, 139, 250), // purple  #A78BFA
        ColorHelper.FromArgb(255, 96, 165, 250),  // blue    #60A5FA
        ColorHelper.FromArgb(255, 74, 222, 128),   // green   #4ADE80
        ColorHelper.FromArgb(255, 251, 191, 36),   // yellow  #FBBF24
    };
    private static int _colorIndex;

    public static readonly DependencyProperty CostProperty =
        DependencyProperty.Register(nameof(Cost), typeof(ModelCost), typeof(CostBarRow),
            new PropertyMetadata(null, OnCostChanged));

    public ModelCost? Cost
    {
        get => (ModelCost?)GetValue(CostProperty);
        set => SetValue(CostProperty, value);
    }

    public CostBarRow()
    {
        this.InitializeComponent();
    }

    private static void OnCostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CostBarRow row || e.NewValue is not ModelCost cost) return;

        row.ModelNameText.Text = cost.ModelName.ToLowerInvariant();
        row.AmountText.Text = $"${cost.Amount:F2}";

        // Proportional fill: percentage of max
        var proportion = cost.MaxAmount > 0 ? cost.Amount / cost.MaxAmount : 0;

        var color = BarColors[_colorIndex % BarColors.Length];
        _colorIndex++;

        row.CostBarFill.Background = new SolidColorBrush(color);

        // Bind to SizeChanged to scale the fill bar proportionally
        row.CostBarFill.Tag = proportion;
        var parent = row.CostBarFill.Parent as Grid;
        if (parent != null)
        {
            parent.SizeChanged += (_, args) =>
            {
                if (row.CostBarFill.Tag is double p)
                    row.CostBarFill.Width = Math.Max(2, args.NewSize.Width * p);
            };
        }
    }
}
