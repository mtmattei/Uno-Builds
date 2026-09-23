using FieldCheck.ViewModels;
using FieldCheck.Views;

namespace FieldCheck.Services;

/// <summary>Frame-backed implementation of <see cref="INavigator"/> owned by the shell.</summary>
public sealed class FrameNavigator(ShellViewModel shell, Func<AssetsViewModel> assets) : INavigator
{
    private Frame? _frame;
    private Func<bool> _isWide = () => false;

    public void Attach(Frame frame, Func<bool> isWide)
    {
        _frame = frame;
        _isWide = isWide;
        _frame.Navigated += (_, e) => UpdateChrome(e.SourcePageType);
    }

    private Frame Frame => _frame ?? throw new InvalidOperationException("Navigator is not attached to a Frame.");

    public void ShowSection(AppSection section)
    {
        var page = section switch
        {
            AppSection.Assets => typeof(AssetsPage),
            AppSection.History => typeof(HistoryPage),
            _ => typeof(DashboardPage),
        };

        if (Frame.CurrentSourcePageType != page)
        {
            Frame.Navigate(page);
        }

        Frame.BackStack.Clear();
        UpdateChrome(page);
    }

    public void ShowAsset(string assetId)
    {
        if (_isWide())
        {
            assets().Select(assetId);
            ShowSection(AppSection.Assets);
        }
        else
        {
            Frame.Navigate(typeof(AssetDetailPage), assetId);
        }
    }

    public void StartInspection(string assetId) => Frame.Navigate(typeof(NewInspectionPage), assetId);

    public void ShowInspectionSuccess(string inspectionId)
    {
        Frame.Navigate(typeof(InspectionSuccessPage), inspectionId);

        // Back from the success page must not reopen the submitted form.
        var stack = Frame.BackStack;
        if (stack.Count > 0 && stack[^1].SourcePageType == typeof(NewInspectionPage))
        {
            stack.RemoveAt(stack.Count - 1);
        }
    }

    public void ReturnToAsset(string assetId)
    {
        if (_isWide())
        {
            ShowAsset(assetId);
            return;
        }

        var stack = Frame.BackStack;
        if (stack.Count > 0 && stack[^1].SourcePageType == typeof(AssetDetailPage) && Equals(stack[^1].Parameter, assetId))
        {
            Frame.GoBack();
            return;
        }

        Frame.Navigate(typeof(AssetDetailPage), assetId);
        stack.RemoveAt(stack.Count - 1);
    }

    public void GoBack()
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    /// <summary>Handles system back (Android back button/gesture). Returns false at a section root.</summary>
    public bool TryHandleSystemBack()
    {
        if (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
            return true;
        }

        return false;
    }

    private void UpdateChrome(Type page)
    {
        if (page == typeof(DashboardPage))
        {
            shell.CurrentSection = AppSection.Dashboard;
        }
        else if (page == typeof(AssetsPage))
        {
            shell.CurrentSection = AppSection.Assets;
        }
        else if (page == typeof(HistoryPage))
        {
            shell.CurrentSection = AppSection.History;
        }

        shell.IsOnSectionRoot = page == typeof(DashboardPage) || page == typeof(AssetsPage) || page == typeof(HistoryPage);
    }
}
