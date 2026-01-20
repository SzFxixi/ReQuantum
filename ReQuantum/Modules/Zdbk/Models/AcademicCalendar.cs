using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ReQuantum.Modules.Zdbk.Models;

/// <summary>
/// У����Ϣ
/// </summary>
public class AcademicCalendar
{
    /// <summary>
    /// ��ǰѧ�����ƣ��� "2024-2025-1" ��ʾ 2024-2025 ѧ���һѧ�ڣ�
    /// </summary>
    [JsonPropertyName("semester_name")]
    public required string SemesterName { get; set; }

    /// <summary>
    /// ѧ�ڿ�ʼ����
    /// </summary>
    [JsonPropertyName("start_date")]
    public required DateOnly StartDate { get; set; }

    /// <summary>
    /// ѧ�ڽ�������
    /// </summary>
    [JsonPropertyName("end_date")]
    public required DateOnly EndDate { get; set; }

    /// <summary>
    /// ����ʱ���б�
    /// </summary>
    [JsonPropertyName("course_adjustments")]
    public List<CourseAdjustment> CourseAdjustments { get; set; } = [];

    /// <summary>
    /// ͣ�������б�
    /// </summary>
    [JsonPropertyName("class_suspension_dates")]
    public List<DateOnly> ClassSuspensionDates { get; set; } = [];

    /// <summary>
    /// У���汾��
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    /// <summary>
    /// �Ƿ�Ϊ��ѧ�ڣ��ļ�Сѧ�ڣ�
    /// </summary>
    [JsonPropertyName("is_short_semester")]
    public bool IsShortSemester { get; set; }

    /// <summary>
    /// ѧ�꣨�� "2024-2025"��
    /// </summary>
    [JsonIgnore]
    public string AcademicYear
    {
        get
        {
            var parts = SemesterName.Split('-');
            return parts.Length >= 2 ? $"{parts[0]}-{parts[1]}" : string.Empty;
        }
    }

    /// <summary>
    /// ѧ�ڴ��루1 �� 2��
    /// </summary>
    [JsonIgnore]
    public string SemesterCode
    {
        get
        {
            var parts = SemesterName.Split('-');
            return parts.Length >= 3 ? parts[2] : "1";
        }
    }

    /// <summary>
    /// ��ȡָ�����ڶ�Ӧ���ܴΣ���1��ʼ��
    /// </summary>
    /// <param name="date">ָ������</param>
    /// <returns>�ܴΣ��������ѧ���ڷ��� null</returns>
    public int? GetWeekNumber(DateOnly date)
    {
        if (date < StartDate || date > EndDate)
        {
            return null;
        }

        // ����ԭʼ�ܴ�
        var daysDiff = date.DayNumber - StartDate.DayNumber;
        var weekNumber = (daysDiff / 7) + 1;

        return weekNumber;
    }

    /// <summary>
    /// ��ȡָ���ܴζ�Ӧ��ѧ�����ƣ���/��/��/�ģ�
    /// </summary>
    /// <param name="weekNumber">�ܴ�</param>
    /// <returns>ѧ������</returns>
    public string GetSemesterNameForWeek(int weekNumber)
    {
        if (IsShortSemester)
        {
            return "��"; // ��ѧ��ͳһΪ��
        }

        // �Ƕ�ѧ�ڣ�1-8��Ϊ��/����9-16��Ϊ��/��
        if (SemesterCode == "1") // ��һѧ��
        {
            return weekNumber <= 8 ? "��" : "��";
        }
        else // �ڶ�ѧ��
        {
            return weekNumber <= 8 ? "��" : "��";
        }
    }

    /// <summary>
    /// ���ָ�������Ƿ񱻵���
    /// </summary>
    /// <param name="date">ָ������</param>
    /// <returns>������Ϣ�����û�е��η��� null</returns>
    public CourseAdjustment? GetAdjustment(DateOnly date)
    {
        return CourseAdjustments.FirstOrDefault(a => a.OriginalDate == date);
    }

    /// <summary>
    /// ���ָ�������Ƿ�ͣ��
    /// </summary>
    /// <param name="date">ָ������</param>
    /// <returns>�Ƿ�ͣ��</returns>
    public bool IsSuspended(DateOnly date)
    {
        return ClassSuspensionDates.Contains(date);
    }

    /// <summary>
    /// ��ȡ�������ʵ���Ͽ�����
    /// </summary>
    /// <param name="date">ԭʼ����</param>
    /// <returns>ʵ���Ͽ�����</returns>
    public DateOnly GetActualCourseDate(DateOnly date)
    {
        // ����Ƿ����������ڵ�������һ��
        var adjustmentToThisDate = CourseAdjustments.FirstOrDefault(a => a.TargetDate == date);
        if (adjustmentToThisDate != null)
        {
            return adjustmentToThisDate.OriginalDate;
        }

        // �����һ���Ƿ񱻵�������������
        var adjustmentFromThisDate = CourseAdjustments.FirstOrDefault(a => a.OriginalDate == date);
        if (adjustmentFromThisDate != null)
        {
            return adjustmentFromThisDate.TargetDate;
        }

        // û�е���
        return date;
    }
}

/// <summary>
/// ������Ϣ
/// </summary>
public class CourseAdjustment
{
    /// <summary>
    /// ԭʼ���ڣ������������ڣ�
    /// </summary>
    [JsonPropertyName("original_date")]
    public required DateOnly OriginalDate { get; set; }

    /// <summary>
    /// Ŀ�����ڣ������������ڣ�
    /// </summary>
    [JsonPropertyName("target_date")]
    public required DateOnly TargetDate { get; set; }

    /// <summary>
    /// ����ԭ�򣨿�ѡ��
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

/// <summary>
/// У����Ӧ
/// </summary>
public class AcademicCalendarResponse
{
    /// <summary>
    /// �ɹ���־
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// ������Ϣ������У�
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// У������
    /// </summary>
    [JsonPropertyName("data")]
    public AcademicCalendar? Data { get; set; }
}
