using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using SalesHeatmap.Models;
using Windows.UI;

namespace SalesHeatmap.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private const int PrimaryRows = 8;
    private const int PrimaryColumns = 12;
    private const int CompactRows = 6;
    private const int CompactColumns = 8;
    private const double MinSales = 1000;
    private const double MaxSales = 50000;

    private readonly Random _random = new();
    private readonly DispatcherQueue _dispatcherQueue;

    // Primary heatmap (12x8 = 96 cells)
    public ObservableCollection<HeatmapCell> PrimaryHeatmapCells { get; } = new();

    // Compact region heatmaps (8x6 = 48 cells each)
    public ObservableCollection<HeatmapCell> RegionEastCells { get; } = new();
    public ObservableCollection<HeatmapCell> RegionWestCells { get; } = new();

    // Cities
    public ObservableCollection<CityData> Cities { get; } = new();

    // Primary KPIs
    [ObservableProperty]
    private double _monthlyTotal = 312134;

    [ObservableProperty]
    private double _yearlyTotal = 3745608;

    [ObservableProperty]
    private double _displayMonthly = 312134;

    [ObservableProperty]
    private double _displayYearly = 3745608;

    [ObservableProperty]
    private double _monthlyChange = 10.0;

    [ObservableProperty]
    private double _yearlyChange = 2.0;

    // Region KPIs
    [ObservableProperty]
    private double _regionEastTotal = 124500;

    [ObservableProperty]
    private double _regionWestTotal = 187634;

    [ObservableProperty]
    private double _displayRegionEast = 124500;

    [ObservableProperty]
    private double _displayRegionWest = 187634;

    [ObservableProperty]
    private double _regionEastChange = 8.2;

    [ObservableProperty]
    private double _regionWestChange = 12.1;

    // Dashboard meta KPIs
    [ObservableProperty]
    private double _activeCities = 3;

    [ObservableProperty]
    private double _conversionRate = 72.4;

    [ObservableProperty]
    private double _liveIndicatorOpacity = 1.0;

    private DispatcherQueueTimer? _heatmapTimer;
    private DispatcherQueueTimer? _dataTimer;
    private DispatcherQueueTimer? _interpolationTimer;

    public DashboardViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        InitializeHeatmaps();
        InitializeCities();
        StartTimers();
    }

    private void InitializeHeatmaps()
    {
        // Primary heatmap (12x8)
        int cellNumber = 1;
        for (int row = 0; row < PrimaryRows; row++)
        {
            for (int col = 0; col < PrimaryColumns; col++)
            {
                var salesValue = MinSales + _random.NextDouble() * (MaxSales - MinSales);
                var intensity = (salesValue - MinSales) / (MaxSales - MinSales);
                PrimaryHeatmapCells.Add(new HeatmapCell(row, col, cellNumber, intensity, salesValue));
                cellNumber++;
            }
        }

        // Region East (8x6)
        cellNumber = 1;
        for (int row = 0; row < CompactRows; row++)
        {
            for (int col = 0; col < CompactColumns; col++)
            {
                var salesValue = MinSales + _random.NextDouble() * (MaxSales - MinSales) * 0.7;
                var intensity = (salesValue - MinSales) / (MaxSales * 0.7 - MinSales);
                RegionEastCells.Add(new HeatmapCell(row, col, cellNumber, Math.Clamp(intensity, 0, 1), salesValue));
                cellNumber++;
            }
        }

        // Region West (8x6)
        cellNumber = 1;
        for (int row = 0; row < CompactRows; row++)
        {
            for (int col = 0; col < CompactColumns; col++)
            {
                var salesValue = MinSales + _random.NextDouble() * (MaxSales - MinSales) * 1.2;
                var intensity = (salesValue - MinSales) / (MaxSales * 1.2 - MinSales);
                RegionWestCells.Add(new HeatmapCell(row, col, cellNumber, Math.Clamp(intensity, 0, 1), salesValue));
                cellNumber++;
            }
        }
    }

    private void InitializeCities()
    {
        var isDark = Application.Current.RequestedTheme == ApplicationTheme.Dark;
        Cities.Add(new CityData("Los Angeles", 201173,
            ColorFromHex(isDark ? "#FFFFFF" : "#1A1A2E"), 12.5));
        Cities.Add(new CityData("New York", 107854,
            ColorFromHex(isDark ? "#A0A0A0" : "#606070"), -3.2));
        Cities.Add(new CityData("Montreal", 165271,
            ColorFromHex(isDark ? "#606060" : "#909098"), 7.8));
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
        _heatmapTimer.Tick += (s, e) => UpdateHeatmaps();
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

    private void UpdateHeatmaps()
    {
        foreach (var cell in PrimaryHeatmapCells)
        {
            var delta = (_random.NextDouble() - 0.5) * 8000;
            cell.SalesValue = Math.Clamp(cell.SalesValue + delta, MinSales, MaxSales);
            cell.Intensity = (cell.SalesValue - MinSales) / (MaxSales - MinSales);
        }

        foreach (var cell in RegionEastCells)
        {
            var delta = (_random.NextDouble() - 0.5) * 5000;
            cell.SalesValue = Math.Clamp(cell.SalesValue + delta, MinSales, MaxSales * 0.7);
            cell.Intensity = Math.Clamp((cell.SalesValue - MinSales) / (MaxSales * 0.7 - MinSales), 0, 1);
        }

        foreach (var cell in RegionWestCells)
        {
            var delta = (_random.NextDouble() - 0.5) * 6000;
            cell.SalesValue = Math.Clamp(cell.SalesValue + delta, MinSales, MaxSales * 1.2);
            cell.Intensity = Math.Clamp((cell.SalesValue - MinSales) / (MaxSales * 1.2 - MinSales), 0, 1);
        }
    }

    private void UpdateData()
    {
        MonthlyTotal = Math.Max(0, MonthlyTotal + (_random.NextDouble() - 0.3) * 500);
        YearlyTotal = Math.Max(0, YearlyTotal + (_random.NextDouble() - 0.3) * 200);
        MonthlyChange = Math.Round(MonthlyChange + (_random.NextDouble() - 0.5) * 0.5, 1);
        YearlyChange = Math.Round(YearlyChange + (_random.NextDouble() - 0.5) * 0.3, 1);

        RegionEastTotal = Math.Max(0, RegionEastTotal + (_random.NextDouble() - 0.35) * 300);
        RegionWestTotal = Math.Max(0, RegionWestTotal + (_random.NextDouble() - 0.35) * 400);
        RegionEastChange = Math.Round(RegionEastChange + (_random.NextDouble() - 0.5) * 0.4, 1);
        RegionWestChange = Math.Round(RegionWestChange + (_random.NextDouble() - 0.5) * 0.4, 1);

        ConversionRate = Math.Round(Math.Clamp(ConversionRate + (_random.NextDouble() - 0.5) * 0.8, 50, 95), 1);

        foreach (var city in Cities)
        {
            city.Value = Math.Max(0, city.Value + (_random.NextDouble() - 0.4) * 300);
            city.Change = Math.Round(city.Change + (_random.NextDouble() - 0.5) * 0.3, 1);
        }

        UpdateLiveIndicator();
    }

    private void UpdateLiveIndicator()
    {
        LiveIndicatorOpacity = LiveIndicatorOpacity > 0.7 ? 0.5 : 1.0;
    }

    private void InterpolateValues()
    {
        DisplayMonthly = Interpolate(DisplayMonthly, MonthlyTotal, 0.1, 10);
        DisplayYearly = Interpolate(DisplayYearly, YearlyTotal, 0.1, 10);
        DisplayRegionEast = Interpolate(DisplayRegionEast, RegionEastTotal, 0.1, 5);
        DisplayRegionWest = Interpolate(DisplayRegionWest, RegionWestTotal, 0.1, 5);

        foreach (var city in Cities)
        {
            var diff = city.Value - city.DisplayValue;
            if (Math.Abs(diff) > 5)
            {
                city.DisplayValue += Math.Sign(diff) * Math.Max(1, Math.Abs(diff) * 0.15);
            }
        }
    }

    private static double Interpolate(double current, double target, double factor, double threshold)
    {
        var diff = target - current;
        if (Math.Abs(diff) > threshold)
        {
            return current + Math.Sign(diff) * Math.Max(1, Math.Abs(diff) * factor);
        }
        return current;
    }

    public void StopTimers()
    {
        _heatmapTimer?.Stop();
        _dataTimer?.Stop();
        _interpolationTimer?.Stop();
    }
}
