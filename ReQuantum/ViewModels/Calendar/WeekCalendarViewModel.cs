using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReQuantum.Attributes;
using ReQuantum.Controls;
using ReQuantum.Services;
using ReQuantum.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ReQuantum.ViewModels;

/// <summary>
/// 周视图日历ViewModel
/// </summary>
[AutoInject(Lifetime.Transient, RegisterTypes = [typeof(WeekCalendarViewModel)])]
public partial class WeekCalendarViewModel : ViewModelBase<WeekCalendarView>
{
    private readonly ICalendarService _calendarService;

    [ObservableProperty]
    private DateOnly _weekStartDate = DateOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    private ObservableCollection<WeekDay> _weekDays = [];

    public event EventHandler<DateOnly>? DateSelected;

    private WeekDay? _previousSelectedDay;

    public WeekCalendarViewModel(ICalendarService calendarService)
    {
        _calendarService = calendarService;
        UpdateWeek();
    }

    partial void OnWeekStartDateChanged(DateOnly value)
    {
        UpdateWeek();
    }

    partial void OnSelectedDateChanged(DateOnly value)
    {
        UpdateSelectionState(value);
    }

    private void UpdateWeek()
    {
        // 从 Service 获取纯数据
        var weekData = _calendarService.GetWeekCalendarData(WeekStartDate);
        
        // 在 ViewModel 中转换为 UI 模型
        var today = DateOnly.FromDateTime(DateTime.Now);
        
        var days = weekData.Select(dayData => new WeekDay
        {
            Date = dayData.Date,
            Day = dayData.Date.Day,
            DayOfWeek = dayData.Date.DayOfWeek.ToString()[..3],
            IsToday = dayData.Date == today,
            IsSelected = dayData.Date == SelectedDate,
            TimelineItems = CalendarItemsHelper.GenerateWeekViewItems(dayData.Todos.ToList(), dayData.Events.ToList()),
            ViewModel = this
        }).ToList();
        
        WeekDays = new ObservableCollection<WeekDay>(days);
        _previousSelectedDay = WeekDays.FirstOrDefault(d => d.IsSelected);
    }

    public void SelectDate(DateOnly date)
    {
        SelectedDate = date;
        DateSelected?.Invoke(this, date);
    }

    /// <summary>
    /// 更新指定日期的时间线事项
    /// </summary>
    public void UpdateDayItems(DateOnly date, List<WeekTimelineItem> items)
    {
        var day = WeekDays.FirstOrDefault(d => d.Date == date);
        if (day != null)
        {
            day.TimelineItems = items;
        }
    }

    /// <summary>
    /// 刷新日历数据（重新从 Service 获取）
    /// </summary>
    public void RefreshCalendarData()
    {
        UpdateWeek();
    }

    private void UpdateSelectionState(DateOnly newSelectedDate)
    {
        // 取消上一个选中的日期
        if (_previousSelectedDay != null)
        {
            _previousSelectedDay.IsSelected = false;
        }

        // 选中新日期
        var newSelectedDay = WeekDays.FirstOrDefault(d => d.Date == newSelectedDate);
        if (newSelectedDay != null)
        {
            newSelectedDay.IsSelected = true;
            _previousSelectedDay = newSelectedDay;
        }
    }
}

public partial class WeekDay : ObservableObject
{
    public DateOnly Date { get; set; }
    public int Day { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsToday { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private List<WeekTimelineItem> _timelineItems = [];

    public WeekCalendarViewModel? ViewModel { get; set; }

    private RelayCommand? _selectCommand;
    public RelayCommand Select => _selectCommand ??= new RelayCommand(() => ViewModel?.SelectDate(Date));
}
