using MPE.Models;
using System.Collections.ObjectModel;

namespace MPE.ViewModels;

public class MainPageViewModel
{
    public Course FeaturedCourse { get; }
    public ObservableCollection<TechBite> TechBites { get; }

    public MainPageViewModel()
    {
        FeaturedCourse = new Course(
            Title: "Intro to Uno",
            Category: "Uno Platform Development",
            Instructor: "Tim Corey",
            VideoSource: "ms-appx:///Assets/timCuno.webm",
            Emoji: "👨‍💻"
        );

        TechBites = new ObservableCollection<TechBite>
        {
            new("Getting Started with MediaPlayerElement", "🎬", "#FFFFE4E1"),
            new("XAML Styling Fundamentals", "🎨", "#FFE8F4FD"),
            new("Cross-Platform Navigation", "🧭", "#FFF0F8E8"),
            new("Data Binding Deep Dive", "🔗", "#FFF8F0FF"),
            new("Performance Optimization Tips", "⚡", "#FFFEF7E8")
        };
    }
}
