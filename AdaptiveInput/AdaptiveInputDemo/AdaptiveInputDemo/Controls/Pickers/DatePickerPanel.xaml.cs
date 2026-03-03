using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace AdaptiveInputDemo.Controls;

public sealed partial class DatePickerPanel : UserControl
{
    private DateTime _displayedMonth;
    private DateTime _selectedDate;
    private readonly List<Button> _dayButtons = new(42); // Max 6 weeks x 7 days

    public event EventHandler<DateTime>? DateSelected;

    public DatePickerPanel()
    {
        InitializeComponent();
        _displayedMonth = DateTime.Today;
        _selectedDate = DateTime.Today;
        UpdateCalendar();
    }

    private void OnQuickPickClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            var date = tag switch
            {
                "today" => DateTime.Today,
                "tomorrow" => DateTime.Today.AddDays(1),
                "nextweek" => DateTime.Today.AddDays(7),
                _ => DateTime.Today
            };

            DateSelected?.Invoke(this, date);
        }
    }

    private void OnPrevMonthClick(object sender, RoutedEventArgs e)
    {
        _displayedMonth = _displayedMonth.AddMonths(-1);
        UpdateCalendar();
    }

    private void OnNextMonthClick(object sender, RoutedEventArgs e)
    {
        _displayedMonth = _displayedMonth.AddMonths(1);
        UpdateCalendar();
    }

    private void UpdateCalendar()
    {
        MonthYearLabel.Text = _displayedMonth.ToString("MMMM yyyy");

        // Clear existing day buttons from grid
        foreach (var button in _dayButtons)
        {
            CalendarGrid.Children.Remove(button);
        }
        _dayButtons.Clear();

        var firstDay = new DateTime(_displayedMonth.Year, _displayedMonth.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(_displayedMonth.Year, _displayedMonth.Month);
        var startDayOfWeek = (int)firstDay.DayOfWeek;

        // Add day buttons directly to the grid with proper row/column positions
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_displayedMonth.Year, _displayedMonth.Month, day);
            var position = startDayOfWeek + day - 1;
            var row = (position / 7) + 1; // +1 because row 0 is headers
            var column = position % 7;

            var button = CreateDayButton(day, date);
            Grid.SetRow(button, row);
            Grid.SetColumn(button, column);
            CalendarGrid.Children.Add(button);
            _dayButtons.Add(button);
        }
    }

    private Button CreateDayButton(int day, DateTime date)
    {
        var isToday = date == DateTime.Today;
        var isSelected = date == _selectedDate;

        var button = new Button
        {
            Content = day.ToString(),
            Width = 36,
            Height = 36,
            MinWidth = 36,
            MinHeight = 36,
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Tag = date,
            Style = (Style)Application.Current.Resources["TextButtonStyle"]
        };

        if (isToday)
        {
            button.BorderBrush = (Brush)Application.Current.Resources["PrimaryBrush"];
            button.BorderThickness = new Thickness(2);
        }

        if (isSelected)
        {
            button.Background = (Brush)Application.Current.Resources["PrimaryBrush"];
            button.Foreground = (Brush)Application.Current.Resources["OnPrimaryBrush"];
        }

        button.Click += OnDayClick;

        return button;
    }

    private void OnDayClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DateTime date)
        {
            _selectedDate = date;
            DateSelected?.Invoke(this, date);
        }
    }
}
