using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using SalesHeatmap.Models;
using Windows.UI;

namespace SalesHeatmap.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const int Rows = 8;
    private const int Columns = 12;
    private const int TotalCells = 96;
    private const double MinSales = 1000;
    private const double MaxSales = 50000;
    private readonly Random _random = new();
    private readonly DispatcherQueue _dispatcherQueue;

    private DispatcherQueueTimer? _heatmapTimer;
    private DispatcherQueueTimer? _dataTimer;
    private DispatcherQueueTimer? _interpolationTimer;

    public ObservableCollection<HeatmapCell> HeatmapCells { get; } = new();
    public ObservableCollection<CityData> Cities { get; } = new();

    [ObservableProperty]
    private double _monthlyTotal = 312134;

    [ObservableProperty]
    private double _yearlyTotal = 312134;

    [ObservableProperty]
    private double _displayMonthly = 312134;

    [ObservableProperty]
    private double _displayYearly = 312134;

    [ObservableProperty]
    private double _monthlyChange = 10.0;

    [ObservableProperty]
    private double _yearlyChange = 2.0;

    [ObservableProperty]
    private double _liveIndicatorOpacity = 1.0;

    public MainViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        InitializeHeatmap();
        InitializeCities();
        StartTimers();
    }

    private void InitializeHeatmap()
    {
        int cellNumber = 1;
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var salesValue = MinSales + _random.NextDouble() * (MaxSales - MinSales);
                var intensity = (salesValue - MinSales) / (MaxSales - MinSales);
                HeatmapCells.Add(new HeatmapCell(row, col, cellNumber, intensity, salesValue));
                cellNumber++;
            }
        }
    }

    private void InitializeCities()
    {
        var isDark = Application.Current.RequestedTheme == ApplicationTheme.Dark;
        Cities.Add(new CityData("Los Angeles", 201173, ColorFromHex(isDark ? "#FFFFFF" : "#1A1A2E")));
        Cities.Add(new CityData("New York", 107854, ColorFromHex(isDark ? "#A0A0A0" : "#606070")));
        Cities.Add(new CityData("Montreal", 165271, ColorFromHex(isDark ? "#606060" : "#909098")));
    }

    private static Color ColorFromHex(string hex)
    {
        hex = hex.TrimStart('#');
        return Color.FromArgb(255,
            Convert.ToByte(hex.Substring(0, 2), 16),
            Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16));
    }

    private void StartTimers()
    {
        _heatmapTimer = _dispatcherQueue.CreateTimer();
        _heatmapTimer.Interval = TimeSpan.FromMilliseconds(150);
        _heatmapTimer.Tick += (s, e) => UpdateHeatmap();
        _heatmapTimer.Start();

        _dataTimer = _dispatcherQueue.CreateTimer();
        _dataTimer.Interval = TimeSpan.FromMilliseconds(2000);
        _dataTimer.Tick += (s, e) => UpdateData();
        _dataTimer.Start();

        _interpolationTimer = _dispatcherQueue.CreateTimer();
        _interpolationTimer.Interval = TimeSpan.FromMilliseconds(16);
        _interpolationTimer.Tick += (s, e) => InterpolateValues();
        _interpolationTimer.Start();
    }

    private void UpdateHeatmap()
    {
        foreach (var cell in HeatmapCells)
        {
            var delta = (_random.NextDouble() - 0.5) * 8000;
            cell.SalesValue = Math.Clamp(cell.SalesValue + delta, MinSales, MaxSales);
            cell.Intensity = (cell.SalesValue - MinSales) / (MaxSales - MinSales);
        }
    }

    private void UpdateData()
    {
        MonthlyTotal = Math.Max(0, MonthlyTotal + (_random.NextDouble() - 0.3) * 500);
        YearlyTotal = Math.Max(0, YearlyTotal + (_random.NextDouble() - 0.3) * 200);
        MonthlyChange = Math.Round(MonthlyChange + (_random.NextDouble() - 0.5) * 0.5, 1);
        YearlyChange = Math.Round(YearlyChange + (_random.NextDouble() - 0.5) * 0.3, 1);

        foreach (var city in Cities)
        {
            city.Value = Math.Max(0, city.Value + (_random.NextDouble() - 0.4) * 300);
        }

        UpdateLiveIndicator();
    }

    private void UpdateLiveIndicator()
    {
        LiveIndicatorOpacity = LiveIndicatorOpacity > 0.7 ? 0.5 : 1.0;
    }

    private void InterpolateValues()
    {
        var monthlyDiff = MonthlyTotal - DisplayMonthly;
        if (Math.Abs(monthlyDiff) > 10)
        {
            DisplayMonthly += Math.Sign(monthlyDiff) * Math.Max(1, Math.Abs(monthlyDiff) * 0.1);
        }

        var yearlyDiff = YearlyTotal - DisplayYearly;
        if (Math.Abs(yearlyDiff) > 10)
        {
            DisplayYearly += Math.Sign(yearlyDiff) * Math.Max(1, Math.Abs(yearlyDiff) * 0.1);
        }

        foreach (var city in Cities)
        {
            var diff = city.Value - city.DisplayValue;
            if (Math.Abs(diff) > 5)
            {
                city.DisplayValue += Math.Sign(diff) * Math.Max(1, Math.Abs(diff) * 0.15);
            }
        }
    }

    public void StopTimers()
    {
        _heatmapTimer?.Stop();
        _dataTimer?.Stop();
        _interpolationTimer?.Stop();
    }
}
