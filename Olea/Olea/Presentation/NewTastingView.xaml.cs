namespace Olea.Presentation;

public sealed partial class NewTastingView : UserControl
{
    private int _currentRating;
    private readonly FontIcon[] _stars;

    public int CurrentRating => _currentRating;
    public string OilName => NameInput.Text?.Trim() ?? string.Empty;
    public string OilOrigin => OriginInput.Text?.Trim() ?? string.Empty;
    public string OilCultivar => CultivarInput.Text?.Trim() ?? string.Empty;
    public string HarvestDate => HarvestDateInput.Text?.Trim() ?? string.Empty;
    public string TastingDateText => TastingDateInput.Text?.Trim() ?? string.Empty;
    public string OilNotes => NotesInput.Text?.Trim() ?? string.Empty;

    public event EventHandler<TastingEntry>? TastingSaved;

    public NewTastingView()
    {
        this.InitializeComponent();
        _stars = new[] { Star1, Star2, Star3, Star4, Star5 };
    }

    private void OnStarTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is FontIcon icon && icon.Tag is string tagStr && int.TryParse(tagStr, out var rating))
        {
            _currentRating = rating;
            UpdateStars();
        }
    }

    private void UpdateStars()
    {
        var activeBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Windows.UI.Color.FromArgb(255, 196, 164, 74));
        var inactiveBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Windows.UI.Color.FromArgb(255, 229, 221, 208));

        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].Foreground = i < _currentRating ? activeBrush : inactiveBrush;
        }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameInput.Text))
        {
            NameInput.Focus(FocusState.Programmatic);
            return;
        }

        // Get selected flavors from the FlavorWheelView sibling
        // TODO: Wire up flavor selection from FlavorWheelView
        var flavors = ImmutableList<FlavorNote>.Empty;
        var intensities = new IntensityProfile(5, 3, 4);

        var entry = new TastingEntry(
            Id: Guid.NewGuid().ToString(),
            Name: OilName,
            Origin: string.IsNullOrEmpty(OilOrigin) ? "Unknown" : OilOrigin,
            Cultivar: OilCultivar,
            HarvestDate: HarvestDate,
            TastingDate: string.IsNullOrEmpty(TastingDateText) ? DateTime.Today.ToString("yyyy-MM-dd") : TastingDateText,
            Rating: _currentRating,
            Flavors: flavors,
            Intensities: intensities,
            Notes: OilNotes);

        TastingSaved?.Invoke(this, entry);
        ResetForm();
    }

    private void ResetForm()
    {
        NameInput.Text = string.Empty;
        OriginInput.Text = string.Empty;
        CultivarInput.Text = string.Empty;
        NotesInput.Text = string.Empty;
        _currentRating = 0;
        UpdateStars();
    }
}
