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

[AutoInject(Lifetime.Transient, RegisterTypes = [typeof(MonthCalendarViewModel)])]
public partial class MonthCalendarViewModel : ViewModelBase<MonthCalendarView>
{
    private readonly ICalendarService _calendarService;

    [ObservableProperty]
    private int _year = DateTime.Now.Year;

    [ObservableProperty]
    private int _month = DateTime.Now.Month;

    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    private ObservableCollection<CalendarDay> _calendarDays = [];

    public event EventHandler<DateOnly>? DateSelected;

    private CalendarDay? _previousSelectedDay;

    public MonthCalendarViewModel(ICalendarService calendarService)
    {
        _calendarService = calendarService;
        UpdateCalendar();
    }

    partial void OnYearChanged(int value)
    {
        UpdateCalendar();
    }

    partial void OnMonthChanged(int value)
    {
        UpdateCalendar();
    }

    partial void OnSelectedDateChanged(DateOnly value)
    {
        UpdateSelectionState(value);
    }

    private void UpdateCalendar()
    {
        var days = new List<CalendarDay>();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var firstDay = new DateOnly(Year, Month, 1);
        var daysInMonth = DateTime.DaysInMonth(Year, Month);
        var firstDayOfWeek = (int)firstDay.DayOfWeek;

        // 添加上个月的日期（前置填充）
        if (firstDayOfWeek > 0)
        {
            var prevMonth = Month == 1 ? 12 : Month - 1;
            var prevYear = Month == 1 ? Year - 1 : Year;
            var daysInPrevMonth = DateTime.DaysInMonth(prevYear, prevMonth);

            for (var i = firstDayOfWeek - 1; i >= 0; i--)
            {
                var day = daysInPrevMonth - i;
                var date = new DateOnly(prevYear, prevMonth, day);
                var dayData = _calendarService.GetCalendarDayData(date);
                
                days.Add(new CalendarDay
                {
                    Date = date,
                    Day = day,
                    IsCurrentMonth = false,
                    IsToday = date == today,
                    IsSelected = date == SelectedDate,
                    Items = CalendarItemsHelper.GenerateMonthViewItems(dayData.Todos.ToList(), dayData.Events.ToList()),
                    ViewModel = this
                });
            }
        }

        // 添加本月的日期
        var monthData = _calendarService.GetMonthCalendarData(Year, Month);
        foreach (var dayData in monthData)
        {
            days.Add(new CalendarDay
            {
                Date = dayData.Date,
                Day = dayData.Date.Day,
                IsCurrentMonth = true,
                IsToday = dayData.Date == today,
                IsSelected = dayData.Date == SelectedDate,
                Items = CalendarItemsHelper.GenerateMonthViewItems(dayData.Todos.ToList(), dayData.Events.ToList()),
                ViewModel = this
            });
        }

        // 添加下个月的日期（后置填充，确保总共42天）
        var remainingDays = 42 - days.Count;
        var nextMonth = Month == 12 ? 1 : Month + 1;
        var nextYear = Month == 12 ? Year + 1 : Year;

        for (var day = 1; day <= remainingDays; day++)
        {
            var date = new DateOnly(nextYear, nextMonth, day);
            var dayData = _calendarService.GetCalendarDayData(date);
            
            days.Add(new CalendarDay
            {
                Date = date,
                Day = day,
                IsCurrentMonth = false,
                IsToday = date == today,
                IsSelected = date == SelectedDate,
                Items = CalendarItemsHelper.GenerateMonthViewItems(dayData.Todos.ToList(), dayData.Events.ToList()),
                ViewModel = this
            });
        }

        CalendarDays = new ObservableCollection<CalendarDay>(days);
        _previousSelectedDay = CalendarDays.FirstOrDefault(d => d.IsSelected);
    }

    public void SelectDate(DateOnly date)
    {
        SelectedDate = date;
        DateSelected?.Invoke(this, date);
    }

    /// <summary>
    /// 更新指定日期的事项列表
    /// </summary>
    public void UpdateDayItems(DateOnly date, List<CalendarDayItem> items)
    {
        var day = CalendarDays.FirstOrDefault(d => d.Date == date);
        if (day != null)
        {
            day.Items = items;
        }
    }

    /// <summary>
    /// 刷新日历数据（重新从 Service 获取）
    /// </summary>
    public void RefreshCalendarData()
    {
        UpdateCalendar();
    }

    private void UpdateSelectionState(DateOnly newSelectedDate)
    {
        // 取消上一个选中的日期
        if (_previousSelectedDay != null)
        {
            _previousSelectedDay.IsSelected = false;
        }

        // 选中新日期
        var newSelectedDay = CalendarDays.FirstOrDefault(d => d.Date == newSelectedDate);
        if (newSelectedDay != null)
        {
            newSelectedDay.IsSelected = true;
            _previousSelectedDay = newSelectedDay;
        }
    }
}

public partial class CalendarDay : ObservableObject
{
    public DateOnly Date { get; set; }
    public int Day { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private List<CalendarDayItem> _items = [];

    public MonthCalendarViewModel? ViewModel { get; set; }

    private RelayCommand? _selectCommand;
    public RelayCommand Select => _selectCommand ??= new RelayCommand(() => ViewModel?.SelectDate(Date));
}
