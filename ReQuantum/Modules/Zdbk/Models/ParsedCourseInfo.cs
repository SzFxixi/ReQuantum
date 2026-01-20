using System;

namespace ReQuantum.Modules.Zdbk.Models;

/// <summary>
/// ������Ŀγ���Ϣ (�� kcb �ֶν�������)
/// </summary>
public class ParsedCourseInfo
{
    /// <summary>
    /// �γ�����
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// ��ʦ����
    /// </summary>
    public string Teacher { get; set; } = string.Empty;

    /// <summary>
    /// ���ҵص�
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// �ܴ���ʼ
    /// </summary>
    public int WeekStart { get; set; }

    /// <summary>
    /// �ܴν���
    /// </summary>
    public int WeekEnd { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    public DateTime? ExamDate { get; set; }

    /// <summary>
    /// ���Կ�ʼʱ��
    /// </summary>
    public TimeOnly? ExamStartTime { get; set; }

    /// <summary>
    /// ���Խ���ʱ��
    /// </summary>
    public TimeOnly? ExamEndTime { get; set; }

    /// <summary>
    /// ԭʼ�� kcb �ֶ�����
    /// </summary>
    public string RawInfo { get; set; } = string.Empty;
}
