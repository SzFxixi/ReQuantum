using Avalonia.Controls;

namespace ReQuantum.Views;

public partial class WeekCalendarView : UserControl
{
    public WeekCalendarView()
    {
        InitializeComponent();
    }

    private void Grid_RequestBringIntoView(object? sender, RequestBringIntoViewEventArgs e)
    {
        // Cancel the bring into view request to prevent viewport jumping
        e.Handled = true;
    }
}
