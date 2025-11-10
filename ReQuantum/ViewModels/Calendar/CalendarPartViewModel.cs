using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReQuantum.Attributes;
using ReQuantum.Infrastructure;
using ReQuantum.Resources.I18n;
using ReQuantum.Views;
using System;

namespace ReQuantum.ViewModels;

/// <summary>
/// 日历视图类型
/// </summary>
public enum CalendarViewType
{
    Week,   // 周视图
    Month   // 月视图
}

[AutoInject(Lifetime.Transient, RegisterTypes = [typeof(CalendarPartViewModel)])]
public partial class CalendarPartViewModel : ViewModelBase<CalendarPartView>
{
    /// <summary>
    /// 动态年月文本：2025年11月 / 2025/11
    /// </summary>
    public LocalizedText YearMonthText { get; }

    #region 视图状态

    [ObservableProperty]
    private CalendarViewType _currentViewType;

    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Now);

    #endregion

    /// <summary>
    /// 月视图ViewModel
    /// </summary>
    public MonthCalendarViewModel MonthCalendarViewModel { get; }

    /// <summary>
    /// 周视图ViewModel
    /// </summary>
    public WeekCalendarViewModel WeekCalendarViewModel { get; }

    public event EventHandler<DateOnly>? DateSelected;

    public CalendarPartViewModel(MonthCalendarViewModel monthCalendarViewModel, WeekCalendarViewModel weekCalendarViewModel)
    {
        YearMonthText = new LocalizedText(Localizer);

        // 初始化日历ViewModels
        CurrentViewType = CalendarViewType.Month;
        MonthCalendarViewModel = monthCalendarViewModel;
        WeekCalendarViewModel = weekCalendarViewModel;

        // 订阅日期选择事件
        MonthCalendarViewModel.DateSelected += OnMonthCalendarDateSelected;
        WeekCalendarViewModel.DateSelected += OnWeekCalendarDateSelected;

        YearMonthText.Set(nameof(UIText.YearMonthFormat), SelectedDate.Year, SelectedDate.Month);
    }

    partial void OnSelectedDateChanged(DateOnly value)
    {
        DateSelected?.Invoke(this, value);
    }

    private void OnMonthCalendarDateSelected(object? sender, DateOnly date)
    {
        SelectedDate = date;
        WeekCalendarViewModel.SelectedDate = date;
    }

    private void OnWeekCalendarDateSelected(object? sender, DateOnly date)
    {
        SelectedDate = date;
        MonthCalendarViewModel.SelectedDate = date;
    }

    #region 视图切换

    [RelayCommand]
    private void SwitchToWeekView()
    {
        CurrentViewType = CalendarViewType.Week;
        YearMonthText.Set(string.Empty);
        WeekCalendarViewModel.WeekStartDate = GetWeekStartDate(MonthCalendarViewModel.SelectedDate);
    }

    [RelayCommand]
    private void SwitchToMonthView()
    {
        CurrentViewType = CalendarViewType.Month;
        YearMonthText.Set(nameof(UIText.YearMonthFormat), SelectedDate.Year, SelectedDate.Month);
    }

    #endregion

    #region 日期导航

    /// <summary>
    /// 向前导航（根据当前视图类型）
    /// </summary>
    [RelayCommand]
    private void GoToPrevious()
    {
        switch (CurrentViewType)
        {
            case CalendarViewType.Month:
                var month = MonthCalendarViewModel.Month;
                var year = MonthCalendarViewModel.Year;
                month--;
                if (month == 0)
                {
                    month = 12;
                    year--;
                }
                MonthCalendarViewModel.Month = month;
                MonthCalendarViewModel.Year = year;
                YearMonthText.Set(nameof(UIText.YearMonthFormat), year, month);
                break;
            case CalendarViewType.Week:
                var newWeekStart = WeekCalendarViewModel.WeekStartDate.AddDays(-7);
                WeekCalendarViewModel.WeekStartDate = newWeekStart;
                WeekCalendarViewModel.SelectedDate = newWeekStart;
                break;
        }
    }

    /// <summary>
    /// 向后导航（根据当前视图类型）
    /// </summary>
    [RelayCommand]
    private void GoToNext()
    {
        switch (CurrentViewType)
        {
            case CalendarViewType.Month:
                var month = MonthCalendarViewModel.Month;
                var year = MonthCalendarViewModel.Year;
                month++;
                if (month == 13)
                {
                    month = 1;
                    year++;
                }

                MonthCalendarViewModel.Year = year;
                MonthCalendarViewModel.Month = month;

                YearMonthText.Set(nameof(UIText.YearMonthFormat), year, month);
                break;
            case CalendarViewType.Week:
                var newWeekStart = WeekCalendarViewModel.WeekStartDate.AddDays(7);
                WeekCalendarViewModel.WeekStartDate = newWeekStart;
                WeekCalendarViewModel.SelectedDate = newWeekStart;
                break;
        }
    }

    /// <summary>
    /// 回到今天
    /// </summary>
    [RelayCommand]
    private void GoToToday()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        SelectedDate = today;

        // 更新子 ViewModel 的年月、周起始日期和选中日期
        MonthCalendarViewModel.Year = today.Year;
        MonthCalendarViewModel.Month = today.Month;
        MonthCalendarViewModel.SelectedDate = today;

        WeekCalendarViewModel.WeekStartDate = GetWeekStartDate(today);
        WeekCalendarViewModel.SelectedDate = today;

        if (CurrentViewType == CalendarViewType.Month)
        {
            YearMonthText.Set(nameof(UIText.YearMonthFormat), today.Year, today.Month);
        }
    }

    /// <summary>
    /// 获取指定日期所在周的起始日期（星期日）
    /// </summary>
    private static DateOnly GetWeekStartDate(DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        return date.AddDays(-dayOfWeek);
    }

    #endregion
}
