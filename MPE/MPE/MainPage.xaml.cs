using MPE.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MPE
{
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel { get; }

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = new MainPageViewModel();
        }

        private void OnWatchCourseClick(object sender, RoutedEventArgs e)
        {
            myMediaPlayer.MediaPlayer?.Play();
        }
    }
}

