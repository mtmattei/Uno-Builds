using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Caffe.Models;

namespace Caffe.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<EspressoItem> EspressoOptions { get; } = new()
    {
        new EspressoItem("Espresso", 30, "Pure, concentrated, bold"),
        new EspressoItem("Doppio", 60, "Double the intensity"),
        new EspressoItem("Ristretto", 20, "Short, sweet, powerful"),
        new EspressoItem("Lungo", 50, "Long pull, smooth finish")
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(BrewButtonText))]
    [NotifyPropertyChangedFor(nameof(GrindFirstLetter))]
    [NotifyCanExecuteChangedFor(nameof(BrewCommand))]
    private EspressoItem? _selectedEspresso;

    [ObservableProperty]
    private int _temperature = 93;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GrindLabel))]
    [NotifyPropertyChangedFor(nameof(GrindHint))]
    [NotifyPropertyChangedFor(nameof(GrindFirstLetter))]
    [NotifyPropertyChangedFor(nameof(GrindParticleCount))]
    [NotifyPropertyChangedFor(nameof(GrindParticleSize))]
    private GrindLevel _grindLevel = GrindLevel.Fine;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExtractionTimeProgress))]
    private int _extractionTime = 27;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBrewing))]
    private bool _isBrewing;

    [ObservableProperty]
    private double _brewProgress;

    public bool HasSelection => SelectedEspresso is not null;
    public bool IsNotBrewing => !IsBrewing;

    public string BrewButtonText => SelectedEspresso is null
        ? "Select your espresso"
        : $"Brew {SelectedEspresso.Name}";

    public string GrindLabel => GrindLevel.GetLabel();
    public string GrindHint => GrindLevel.GetHint();
    public string GrindFirstLetter => GrindLevel.GetFirstLetter();
    public int GrindParticleCount => GrindLevel.GetParticleCount();
    public double GrindParticleSize => GrindLevel.GetParticleSize();

    public int MinTemperature => 88;
    public int MaxTemperature => 96;
    public int MinExtractionTime => 20;
    public int MaxExtractionTime => 35;

    // Progress value for extraction time (0-100)
    public double ExtractionTimeProgress => (ExtractionTime - 20) / 15.0 * 100;

    [RelayCommand]
    private void SelectEspresso(EspressoItem? item)
    {
        if (item is not null)
        {
            SelectedEspresso = item;
        }
    }

    [RelayCommand]
    private void SetGrindLevel(GrindLevel level)
    {
        GrindLevel = level;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task BrewAsync()
    {
        if (SelectedEspresso is null || IsBrewing) return;

        IsBrewing = true;
        BrewProgress = 0;

        // Animate brew progress over 2.5 seconds
        var startTime = DateTime.Now;
        var duration = TimeSpan.FromMilliseconds(2500);

        while (DateTime.Now - startTime < duration)
        {
            var elapsed = DateTime.Now - startTime;
            BrewProgress = Math.Min(1.0, elapsed.TotalMilliseconds / duration.TotalMilliseconds);
            await Task.Delay(16); // ~60fps
        }

        BrewProgress = 1.0;
        await Task.Delay(300);

        IsBrewing = false;
        BrewProgress = 0;
    }
}
