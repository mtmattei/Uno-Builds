using ClaudeDash.Services;
using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeDash.Views;

public sealed partial class ChatPage : Page
{
    private readonly IChatService _chatService;

    public ChatPage()
    {
        this.InitializeComponent();

        var host = App.Current.Host!.Services;
        DataContext = ActivatorUtilities.CreateInstance<BindableChatModel>(host);
        _chatService = host.GetRequiredService<IChatService>();

        this.Loaded += ChatPage_Loaded;
    }

    private void ChatPage_Loaded(object sender, RoutedEventArgs e)
    {
        var isConfigured = _chatService.IsConfigured;
        ApiKeyPanel.Visibility = isConfigured ? Visibility.Collapsed : Visibility.Visible;
        MessagesScroller.Visibility = isConfigured ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Send_Click(object sender, RoutedEventArgs e)
    {
        var text = InputBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        InputBox.Text = string.Empty;

        if (DataContext is BindableChatModel vm)
        {
            vm.SendMessage.Execute(text);
        }
    }

    private void SetApiKey_Click(object sender, RoutedEventArgs e)
    {
        var key = ApiKeyBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(key)) return;

        _chatService.SetApiKey(key);
        ApiKeyBox.Text = string.Empty;

        ApiKeyPanel.Visibility = Visibility.Collapsed;
        MessagesScroller.Visibility = Visibility.Visible;
    }

    private void InputBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            Send_Click(sender, new RoutedEventArgs());
        }
    }
}
