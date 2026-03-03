using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ListHold.Models;

namespace ListHold.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ListItemViewModel> _items;

    public MainViewModel()
    {
        _items = new ObservableCollection<ListItemViewModel>(GenerateSampleData());
    }

    private static IEnumerable<ListItemViewModel> GenerateSampleData()
    {
        yield return new ListItemViewModel(new ListItemModel(
            Id: "1",
            Title: "Project Aurora",
            Preview: "Next-gen design system",
            Details: "A comprehensive design system built for scalability and consistency across all platforms. Features include dynamic theming, responsive components, and accessibility-first approach.",
            Meta: new Dictionary<string, string>
            {
                { "Status", "Active" },
                { "Team", "Design" },
                { "Priority", "High" }
            },
            Actions: new List<string> { "View Details", "Edit", "Archive" }
        ));

        yield return new ListItemViewModel(new ListItemModel(
            Id: "2",
            Title: "Backend Migration",
            Preview: "Cloud infrastructure upgrade",
            Details: "Migrating legacy services to modern cloud-native architecture. Includes containerization, microservices decomposition, and improved CI/CD pipelines.",
            Meta: new Dictionary<string, string>
            {
                { "Status", "In Progress" },
                { "Team", "Platform" },
                { "Due", "Q1 2025" }
            },
            Actions: new List<string> { "View Timeline", "Resources", "Report" }
        ));

        yield return new ListItemViewModel(new ListItemModel(
            Id: "3",
            Title: "User Research Study",
            Preview: "Customer feedback analysis",
            Details: "Comprehensive study of user behavior patterns and pain points. Gathering qualitative and quantitative data to inform product decisions and roadmap priorities.",
            Meta: new Dictionary<string, string>
            {
                { "Participants", "150+" },
                { "Method", "Mixed" },
                { "Phase", "Analysis" }
            },
            Actions: new List<string> { "View Insights", "Export Data", "Schedule" }
        ));

        yield return new ListItemViewModel(new ListItemModel(
            Id: "4",
            Title: "Mobile App v3.0",
            Preview: "Major feature release",
            Details: "Complete redesign of the mobile experience with new navigation patterns, offline-first architecture, and performance optimizations for low-end devices.",
            Meta: new Dictionary<string, string>
            {
                { "Platform", "iOS/Android" },
                { "Version", "3.0.0" },
                { "Beta", "Active" }
            },
            Actions: new List<string> { "Test Build", "Release Notes", "Feedback" }
        ));

        yield return new ListItemViewModel(new ListItemModel(
            Id: "5",
            Title: "Security Audit",
            Preview: "Annual compliance review",
            Details: "Third-party security assessment covering infrastructure, application code, and operational procedures. Ensuring compliance with SOC 2 and GDPR requirements.",
            Meta: new Dictionary<string, string>
            {
                { "Auditor", "SecureCo" },
                { "Scope", "Full" },
                { "Status", "Scheduled" }
            },
            Actions: new List<string> { "View Scope", "Documents", "Contact" }
        ));
    }
}
