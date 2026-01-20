using System;
using System.Collections.Generic;

namespace ReQuantum.Modules.Zdbk.Constants;

/// <summary>
/// �ڴ�ʱ��� (�����㽭��ѧ��׼��Ϣʱ��)
/// </summary>
public static class ClassTimeTable
{
    /// <summary>
    /// �ڴ�ʱ��ӳ�� (�ڴ� -> (��ʼʱ��, ����ʱ��))
    /// </summary>
    public static readonly Dictionary<int, (TimeOnly Start, TimeOnly End)> SectionTimeMap = new()
    {
        { 1, (new TimeOnly(8, 0), new TimeOnly(8, 45)) },    // ��1�� 08:00-08:45
        { 2, (new TimeOnly(8, 50), new TimeOnly(9, 35)) },   // ��2�� 08:50-09:35
        { 3, (new TimeOnly(10, 0), new TimeOnly(10, 45)) },  // ��3�� 10:00-10:45
        { 4, (new TimeOnly(10, 50), new TimeOnly(11, 35)) }, // ��4�� 10:50-11:35
        { 5, (new TimeOnly(11, 40), new TimeOnly(12, 25)) }, // ��5�� 11:40-12:25
        { 6, (new TimeOnly(13, 25), new TimeOnly(14, 10)) }, // ��6�� 13:25-14:10
        { 7, (new TimeOnly(14, 15), new TimeOnly(15, 0)) },  // ��7�� 14:15-15:00
        { 8, (new TimeOnly(15, 5), new TimeOnly(15, 50)) },  // ��8�� 15:05-15:50
        { 9, (new TimeOnly(16, 15), new TimeOnly(17, 00)) }, // ��9�� 16:10-16:55
        { 10, (new TimeOnly(17, 5), new TimeOnly(17, 50)) }, // ��10�� 17:00-17:45
        { 11, (new TimeOnly(18, 50), new TimeOnly(19, 35)) }, // ��11�� 18:50-19:35
        { 12, (new TimeOnly(19, 40), new TimeOnly(20, 25)) },  // ��12�� 19:40-20:25
        { 13, (new TimeOnly(20, 30), new TimeOnly(21, 15)) }  // ��13�� 20:30-21:15
    };

    /// <summary>
    /// ������ʼ�ڴκͳ������ȼ����Ͽ�ʱ�䷶Χ
    /// </summary>
    /// <param name="startSection">��ʼ�ڴ� (1-13)</param>
    /// <param name="duration">�������� (1-5)</param>
    /// <returns>��ʼʱ��ͽ���ʱ��</returns>
    public static (TimeOnly Start, TimeOnly End) GetClassTime(int startSection, int duration)
    {
        if (!SectionTimeMap.TryGetValue(startSection, out var startTime))
        {
            throw new ArgumentException($"Invalid start section: {startSection}");
        }

        var endSection = startSection + duration - 1;
        if (!SectionTimeMap.TryGetValue(endSection, out var endTime))
        {
            throw new ArgumentException($"Invalid end section: {endSection}");
        }

        return (startTime.Start, endTime.End);
    }
}
