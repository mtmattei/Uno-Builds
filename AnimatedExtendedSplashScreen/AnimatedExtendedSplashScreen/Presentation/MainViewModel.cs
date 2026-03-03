namespace AnimatedExtendedSplashScreen.Presentation;

using AnimatedExtendedSplashScreen.Models;
using System.Collections.ObjectModel;

public partial class MainViewModel : ObservableObject
{
    private INavigator _navigator;

    [ObservableProperty]
    private ObservableCollection<Movie> movies;

    public MainViewModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";

        // Initialize movie collection with sample data
        Movies = new ObservableCollection<Movie>
        {
            new Movie
            {
                Title = "The Shawshank Redemption",
                Genre = "Drama",
                Year = 1994,
                Rating = "9.3",
                ThumbnailUrl = "https://image.tmdb.org/t/p/w500/q6y0Go1tsGEsmtFryDOJo3dEmqu.jpg",
                Description = "Two imprisoned men bond over a number of years."
            },
            new Movie
            {
                Title = "The Godfather",
                Genre = "Crime",
                Year = 1972,
                Rating = "9.2",
                ThumbnailUrl = "https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsRolD1fZdja1.jpg",
                Description = "The aging patriarch of an organized crime dynasty transfers control."
            },
            new Movie
            {
                Title = "The Dark Knight",
                Genre = "Action",
                Year = 2008,
                Rating = "9.0",
                ThumbnailUrl = "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg",
                Description = "When the menace known as the Joker emerges from his mysterious past."
            },
            new Movie
            {
                Title = "Pulp Fiction",
                Genre = "Crime",
                Year = 1994,
                Rating = "8.9",
                ThumbnailUrl = "https://image.tmdb.org/t/p/w500/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg",
                Description = "The lives of two mob hitmen, a boxer, and a pair of diner bandits intertwine."
            },
            new Movie
            {
                Title = "Forrest Gump",
                Genre = "Drama",
                Year = 1994,
                Rating = "8.8",
                ThumbnailUrl = "https://image.tmdb.org/t/p/w500/arw2vcBveWOVZr6pxd9XTd1TdQa.jpg",
                Description = "The presidencies of Kennedy and Johnson unfold through the perspective of an Alabama man."
            },
            new Movie
            {
                Title = "Inception",
                Genre = "Sci-Fi",
                Year = 2010,
                Rating = "8.8",
                ThumbnailUrl = "https://image.tmdb.org/t/p/w500/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg",
                Description = "A thief who steals corporate secrets through dream-sharing technology."
            },
            new Movie
            {
                Title = "The Matrix",
                Genre = "Sci-Fi",
                Year = 1999,
                Rating = "8.7",
                ThumbnailUrl = "https://image.tmdb.org/t/p/w500/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg",
                Description = "A computer hacker learns about the true nature of his reality."
            },
            new Movie
            {
                Title = "Interstellar",
                Genre = "Sci-Fi",
                Year = 2014,
                Rating = "8.6",
                ThumbnailUrl = "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
                Description = "A team of explorers travel through a wormhole in space."
            }
        };
    }

    public string? Title { get; }
}
