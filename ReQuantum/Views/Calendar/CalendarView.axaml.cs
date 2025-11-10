using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ReQuantum.ViewModels;
using System;

namespace ReQuantum.Views;

public partial class CalendarView : UserControl
{
    public CalendarView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // 初始加载数据
        if (DataContext is CalendarViewModel viewModel)
        {
            UpdateCalendarData(viewModel.CalendarPartViewModel);
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is CalendarViewModel viewModel)
        {
            viewModel.CalendarPartViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (DataContext is not CalendarViewModel viewModel)
            return;

        var calendarVm = viewModel.CalendarPartViewModel;

        // 当视图类型变化时，更新日历数据
        if (e.PropertyName == nameof(CalendarPartViewModel.CurrentViewType))
        {
            // 延迟执行，确保视图已经渲染
            Dispatcher.UIThread.Post(() =>
            {
                UpdateCalendarData(calendarVm);
            }, DispatcherPriority.Loaded);
        }
        // 当年月、选中日期、周起始日期变化时，更新日历数据
        else if (e.PropertyName == nameof(CalendarPartViewModel.SelectedYear) ||
            e.PropertyName == nameof(CalendarPartViewModel.SelectedMonth) ||
            e.PropertyName == nameof(CalendarPartViewModel.SelectedDate) ||
            e.PropertyName == nameof(CalendarPartViewModel.WeekStartDate))
        {
            UpdateCalendarData(calendarVm);
        }
    }

    private void UpdateCalendarData(CalendarPartViewModel viewModel)
    {
        // 直接刷新 ViewModels，它们会从 Service 获取最新数据
        viewModel.MonthCalendarViewModel.RefreshCalendarData();
        viewModel.WeekCalendarViewModel.RefreshCalendarData();
    }
}
