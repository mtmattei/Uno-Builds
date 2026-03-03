using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FieldOpsPro.Models;
using FieldOpsPro.Models.Enums;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class StatusHeroCard : UserControl
{
    public StatusHeroCard()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAppearance();
    }

    public static readonly DependencyProperty AssignmentProperty =
        DependencyProperty.Register(nameof(Assignment), typeof(CurrentAssignment), typeof(StatusHeroCard),
            new PropertyMetadata(null, OnAssignmentChanged));

    public CurrentAssignment? Assignment
    {
        get => (CurrentAssignment?)GetValue(AssignmentProperty);
        set => SetValue(AssignmentProperty, value);
    }

    private static void OnAssignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusHeroCard card)
        {
            card.UpdateAppearance();
        }
    }

    private void UpdateAppearance()
    {
        if (StatusTitle == null) return;

        if (Assignment == null)
        {
            StatusTitle.Text = "Available";
            DestinationText.Text = "No active assignment";
            EtaText.Text = "";
            return;
        }

        StatusTitle.Text = Assignment.Status switch
        {
            AgentStatus.OnRoute => "En Route to Site",
            AgentStatus.OnSite => "On Site",
            AgentStatus.Available => "Available",
            AgentStatus.Break => "On Break",
            _ => "Status Unknown"
        };

        DestinationText.Text = Assignment.Destination ?? "No destination set";
        EtaText.Text = !string.IsNullOrEmpty(Assignment.Eta) ? $"ETA: {Assignment.Eta}" : "";
    }
}
