using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media;
using Wellmetrix.Models;
using Wellmetrix.Services;

namespace Wellmetrix.Presentation;

public class BodyExplorerViewModel : INotifyPropertyChanged
{
    private readonly IHealthDataService _healthDataService;
    private Organ? _selectedOrgan;
    private Brush? _currentAccentBrush;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<Organ> Organs { get; }

    public Organ? SelectedOrgan
    {
        get => _selectedOrgan;
        set
        {
            if (_selectedOrgan != value)
            {
                _selectedOrgan = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedOrgan));
                UpdateAccentBrush();
            }
        }
    }

    public bool HasSelectedOrgan => SelectedOrgan != null;

    public Brush? CurrentAccentBrush
    {
        get => _currentAccentBrush;
        private set
        {
            if (_currentAccentBrush != value)
            {
                _currentAccentBrush = value;
                OnPropertyChanged();
            }
        }
    }

    public BodyExplorerViewModel() : this(new HealthDataService())
    {
    }

    public BodyExplorerViewModel(IHealthDataService healthDataService)
    {
        _healthDataService = healthDataService;
        Organs = _healthDataService.GetOrgans();

        // Select heart by default
        if (Organs.Count > 0)
        {
            SelectedOrgan = Organs[0];
        }
    }

    public void SelectOrgan(string organId)
    {
        var organ = Organs.FirstOrDefault(o => o.Id == organId);
        if (organ != null)
        {
            SelectedOrgan = organ;
        }
    }

    private void UpdateAccentBrush()
    {
        if (SelectedOrgan != null && !string.IsNullOrEmpty(SelectedOrgan.AccentColorKey))
        {
            if (Microsoft.UI.Xaml.Application.Current.Resources.TryGetValue(
                SelectedOrgan.AccentColorKey, out var brush) && brush is Brush b)
            {
                CurrentAccentBrush = b;
            }
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
