using System.Collections.ObjectModel;
using AdTokensIDE.Models;
using AdTokensIDE.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdTokensIDE.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAIService _aiService;
    private readonly IAdRotationService _adRotationService;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages = new();

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private int _totalTokensUsed;

    [ObservableProperty]
    private int _currentAdIndex;

    [ObservableProperty]
    private string _currentResponse = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SolutionItem> _solutionItems = new();

    [ObservableProperty]
    private int _lastResponseTokensEarned;

    [ObservableProperty]
    private bool _showTokensEarned;

    [ObservableProperty]
    private int _totalSponsoredTokens;

    [ObservableProperty]
    private int _displayedTokenCount;

    public int NetTokensUsed => Math.Max(0, TotalTokensUsed - TotalSponsoredTokens);

    public IReadOnlyList<Advertisement> Ads => _adRotationService.Ads;

    public Advertisement? CurrentAd => Ads.Count > CurrentAdIndex ? Ads[CurrentAdIndex] : null;

    public MainViewModel(IAIService aiService, IAdRotationService adRotationService)
    {
        _aiService = aiService;
        _adRotationService = adRotationService;
        InitializeSolutionExplorer();
    }

    private void InitializeSolutionExplorer()
    {
        SolutionItems = new ObservableCollection<SolutionItem>
        {
            new SolutionItem
            {
                Name = "SponsoredAI",
                IconGlyph = "\uE8B7",
                IsFolder = true,
                IsExpanded = true,
                Children = new ObservableCollection<SolutionItem>
                {
                    new SolutionItem
                    {
                        Name = "Controls",
                        IconGlyph = "\uE8B7",
                        IsFolder = true,
                        Children = new ObservableCollection<SolutionItem>
                        {
                            new SolutionItem { Name = "ChatPanel.xaml", IconGlyph = "\uE943" },
                            new SolutionItem { Name = "ChatPanel.xaml.cs", IconGlyph = "\uE943" },
                            new SolutionItem { Name = "AdCarousel.xaml", IconGlyph = "\uE943" }
                        }
                    },
                    new SolutionItem
                    {
                        Name = "ViewModels",
                        IconGlyph = "\uE8B7",
                        IsFolder = true,
                        Children = new ObservableCollection<SolutionItem>
                        {
                            new SolutionItem { Name = "ChatViewModel.cs", IconGlyph = "\uE943" },
                            new SolutionItem { Name = "AdViewModel.cs", IconGlyph = "\uE943" }
                        }
                    },
                    new SolutionItem
                    {
                        Name = "Services",
                        IconGlyph = "\uE8B7",
                        IsFolder = true,
                        Children = new ObservableCollection<SolutionItem>
                        {
                            new SolutionItem { Name = "FakeAIService.cs", IconGlyph = "\uE943" }
                        }
                    },
                    new SolutionItem { Name = "MainPage.xaml", IconGlyph = "\uE943" },
                    new SolutionItem { Name = "appsettings.json", IconGlyph = "\uE90F" }
                }
            }
        };
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        var userMessage = new ChatMessage(
            Guid.NewGuid().ToString(),
            InputText,
            IsUser: true,
            DateTime.Now);

        Messages.Add(userMessage);
        var prompt = InputText;
        InputText = string.Empty;

        IsGenerating = true;
        CurrentResponse = string.Empty;
        ShowTokensEarned = false;
        LastResponseTokensEarned = 0;
        _cts = new CancellationTokenSource();

        _adRotationService.StartRotation(index =>
        {
            CurrentAdIndex = index;
            OnPropertyChanged(nameof(CurrentAd));
        });

        try
        {
            var responseBuilder = new System.Text.StringBuilder();
            await foreach (var chunk in _aiService.StreamResponseAsync(prompt, _cts.Token))
            {
                responseBuilder.Append(chunk);
                CurrentResponse = responseBuilder.ToString();
                TotalTokensUsed += _aiService.EstimateTokenCount(chunk);
                DisplayedTokenCount = TotalTokensUsed; // Roll up during generation
                OnPropertyChanged(nameof(NetTokensUsed));
            }

            var responseTokens = _aiService.EstimateTokenCount(responseBuilder.ToString());
            var aiMessage = new ChatMessage(
                Guid.NewGuid().ToString(),
                responseBuilder.ToString(),
                IsUser: false,
                DateTime.Now,
                CurrentAd?.ProductName,
                responseTokens);

            Messages.Add(aiMessage);
            LastResponseTokensEarned = responseTokens;
            TotalSponsoredTokens += responseTokens;
            OnPropertyChanged(nameof(NetTokensUsed));
            DisplayedTokenCount = NetTokensUsed; // Roll down to show savings
            ShowTokensEarned = true;
        }
        finally
        {
            _adRotationService.StopRotation();
            IsGenerating = false;
            CurrentResponse = string.Empty;
        }
    }

    [RelayCommand]
    private void CancelGeneration()
    {
        _cts?.Cancel();
    }
}
