using ClaudeDash.Models.Search;
using ClaudeDash.ViewModels;
using Microsoft.UI.Xaml.Input;

namespace ClaudeDash.Controls;

public sealed partial class SearchOverlay : UserControl
{
    public SearchOverlayViewModel ViewModel { get; set; } = null!;

    public SearchOverlay()
    {
        this.InitializeComponent();
    }

    public void Initialize(SearchOverlayViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;

        ViewModel.RequestFocusInput += () =>
        {
            this.Visibility = Visibility.Visible;
            _ = FocusInputAsync();
        };

        ViewModel.RequestClose += () =>
        {
            this.Visibility = Visibility.Collapsed;
        };
    }

    private async Task FocusInputAsync()
    {
        // Small delay to allow visibility change to propagate
        await Task.Delay(50);
        SearchInput.Focus(FocusState.Programmatic);
    }

    private void SearchInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Escape:
                ViewModel.Close();
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Up:
                ViewModel.MoveSelectionUp();
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Down:
                ViewModel.MoveSelectionDown();
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Enter:
                ViewModel.ConfirmSelection();
                e.Handled = true;
                break;
        }
    }

    private void Backdrop_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.Close();
    }

    private void ResultsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SearchResult result)
        {
            ViewModel.NavigateToResult(result);
        }
    }
}
