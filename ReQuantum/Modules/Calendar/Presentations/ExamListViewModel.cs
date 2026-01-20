using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReQuantum.Assets.I18n;
using ReQuantum.Infrastructure.Abstractions;
using ReQuantum.Infrastructure.Services;
using ReQuantum.Modules.Calendar.Entities;
using ReQuantum.Modules.Calendar.Services;
using ReQuantum.Modules.Common.Attributes;
using ReQuantum.Modules.Zdbk.Services;
using ReQuantum.Modules.ZjuSso.Services;
using ReQuantum.ViewModels;
using ReQuantum.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LocalizedText = ReQuantum.Infrastructure.Entities.LocalizedText;

namespace ReQuantum.Modules.Calendar.Presentations;

[AutoInject(Lifetime.Singleton, RegisterTypes = [typeof(ExamListViewModel), typeof(IEventHandler<CalendarSelectedDateChanged>)])]
public partial class ExamListViewModel : ViewModelBase<ExamListView>, IEventHandler<CalendarSelectedDateChanged>
{
    private readonly ICalendarService _calendarService;
    private readonly IZdbkExamService _examService;
    private readonly IZdbkCalendarConverter _converter;
    private readonly IZjuSsoService _zjuSsoService;

    /// <summary>
    /// 动态标题：考试 - 日期
    /// </summary>
    public LocalizedText ExamsTitle { get; }

    #region 数据集合

    [ObservableProperty]
    private ObservableCollection<CalendarEvent> _exams = [];

    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Now);

    #endregion

    #region 同步状态

    [ObservableProperty]
    private bool _isSyncingExams;

    [ObservableProperty]
    private string _debugInfo = string.Empty;

    [ObservableProperty]
    private bool _showDebugInfo = false;

    #endregion

    public ExamListViewModel(
        ICalendarService calendarService,
        IZdbkExamService examService,
        IZdbkCalendarConverter converter,
        IZjuSsoService zjuSsoService)
    {
        _calendarService = calendarService;
        _examService = examService;
        _converter = converter;
        _zjuSsoService = zjuSsoService;
        ExamsTitle = new LocalizedText();
        UpdateExamsTitle();
        LoadExams();

        _zjuSsoService.OnLogin += OnLoginHandler;
        _zjuSsoService.OnLogout += () => OnPropertyChanged(nameof(ShowExamSyncButton));
    }

    private async void OnLoginHandler()
    {
        OnPropertyChanged(nameof(ShowExamSyncButton));
        // 登录成功后自动同步考试信息
        await SyncExamsAsync();
    }

    #region 数据加载

    public void LoadExams()
    {
        // 加载选中日期的考试（跨越该日期的所有考试）
        var exams = _calendarService.GetEventsByDate(SelectedDate)
            .Where(e => e.IsFromZdbkExam)
            .ToList();
        Exams = new ObservableCollection<CalendarEvent>(exams);
    }

    partial void OnSelectedDateChanged(DateOnly value)
    {
        UpdateExamsTitle();
        LoadExams();
    }

    private void UpdateExamsTitle()
    {
        // 暂时使用EventsOnDate格式，后续可以通过资源文件自定义
        ExamsTitle.Set("EventsOnDate", SelectedDate.ToDateTime(TimeOnly.MinValue));
    }

    #endregion

    #region 考试管理

    [RelayCommand]
    private void DeleteExam(CalendarEvent exam)
    {
        _calendarService.DeleteEvent(exam.Id);
        Exams.Remove(exam);
    }

    #endregion

    #region 教务网考试信息同步

    public bool ShowExamSyncButton => _zjuSsoService.IsAuthenticated;

    [RelayCommand]
    private async Task SyncExamsAsync()
    {
        if (IsSyncingExams)
            return;
        IsSyncingExams = true;

        var debugLines = new List<string>();
        debugLines.Add($"===== 考试同步调试信息 =====");
        debugLines.Add($"开始时间: {DateTime.Now:HH:mm:ss}");

        try
        {
            debugLines.Add("正在获取考试信息...");
            var examsResult = await _examService.GetExamsAsync();

            if (!examsResult.IsSuccess)
            {
                debugLines.Add($"❌ 获取失败: {examsResult.Message}");
                DebugInfo = string.Join("\n", debugLines);
                ShowDebugInfo = true;
                return;
            }

            var parsedExams = examsResult.Value;
            debugLines.Add($"✅ 获取成功");
            debugLines.Add($"考试数量: {parsedExams.Count}");

            if (parsedExams.Any())
            {
                debugLines.Add("\n前3个考试详情:");
                for (int i = 0; i < Math.Min(3, parsedExams.Count); i++)
                {
                    var exam = parsedExams[i];
                    debugLines.Add($"  考试 #{i + 1}:");
                    debugLines.Add($"    课程: {exam.CourseName}");
                    debugLines.Add($"    学分: {exam.Credit}");
                    debugLines.Add($"    类型: {exam.ExamType}");
                    if (exam.StartTime.HasValue && exam.EndTime.HasValue)
                    {
                        debugLines.Add($"    时间: {exam.StartTime:yyyy-MM-dd HH:mm} - {exam.EndTime:HH:mm}");
                    }
                    if (!string.IsNullOrEmpty(exam.Location))
                    {
                        debugLines.Add($"    地点: {exam.Location}");
                    }
                    if (!string.IsNullOrEmpty(exam.RawTimeString))
                    {
                        debugLines.Add($"    原始时间: {exam.RawTimeString}");
                    }
                }
            }

            debugLines.Add("\n正在转换为日程事件...");
            var calendarEvents = _converter.ConvertExamsToCalendarEvents(parsedExams);
            debugLines.Add($"转换后事件数: {calendarEvents.Count}");

            // 标记来源为考试
            foreach (var evt in calendarEvents)
                evt.IsFromZdbkExam = true;

            // 删除旧考试，添加新考试
            var existingExamEvents = _calendarService.GetAllEvents().Where(e => e.IsFromZdbkExam).ToList();
            var newEventIds = calendarEvents.Select(e => e.Id).ToHashSet();

            debugLines.Add($"\n删除旧考试: {existingExamEvents.Count}");
            debugLines.Add($"添加新考试: {calendarEvents.Count}");

            foreach (var existingEvent in existingExamEvents.Where(e => !newEventIds.Contains(e.Id)))
                _calendarService.DeleteEvent(existingEvent.Id);

            foreach (var evt in calendarEvents)
                _calendarService.AddOrUpdateEvent(evt);

            Publisher.Publish(new CalendarSelectedDateChanged(SelectedDate));

            debugLines.Add($"\n✅ 同步完成");
            debugLines.Add($"结束时间: {DateTime.Now:HH:mm:ss}");
        }
        catch (Exception ex)
        {
            debugLines.Add($"\n❌ 异常: {ex.Message}");
            debugLines.Add($"堆栈: {ex.StackTrace}");
        }
        finally
        {
            IsSyncingExams = false;
            DebugInfo = string.Join("\n", debugLines);
            ShowDebugInfo = true;
        }
    }

    #endregion

    public void Handle(CalendarSelectedDateChanged @event)
    {
        SelectedDate = @event.Date;
    }
}
