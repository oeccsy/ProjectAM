#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// NpcMemory가 가진 정보를 인스펙터에 보여주는 디버그용 인스펙터.
/// 접촉은 날짜별로 묶고, 희생은 따로 나열한다.
/// </summary>
[CustomEditor(typeof(NpcMemory))]
public class NpcMemoryEditor : Editor
{
    // 플레이 중 기억이 늘어나는 것을 바로 보기 위해 매 프레임 다시 그린다
    public override bool RequiresConstantRepaint() => true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NpcMemory memory = (NpcMemory)target;

        EditorGUILayout.Space();
        DrawContacts(memory.ContactInfoList);

        EditorGUILayout.Space();
        DrawVictims(memory.VictimInfoList);
    }

    private void DrawContacts(IReadOnlyList<ContactInfo> contacts)
    {
        EditorGUILayout.LabelField($"접촉 ({contacts.Count})", EditorStyles.boldLabel);

        if (contacts.Count == 0)
        {
            EditorGUILayout.LabelField("아는 것 없음");
            return;
        }

        foreach (int day in CollectDays(contacts))
        {
            EditorGUILayout.LabelField($"D{day}");
            EditorGUI.indentLevel++;

            foreach (ContactInfo contact in CollectDailyContacts(contacts, day))
            {
                EditorGUILayout.LabelField($"{contact.hour}시   {contact.npcA} - {contact.npcB}");
            }

            EditorGUI.indentLevel--;
        }
    }

    private void DrawVictims(IReadOnlyList<VictimInfo> victims)
    {
        EditorGUILayout.LabelField($"희생 ({victims.Count})", EditorStyles.boldLabel);

        if (victims.Count == 0)
        {
            EditorGUILayout.LabelField("아는 것 없음");
            return;
        }

        foreach (VictimInfo victim in victims)
        {
            EditorGUILayout.LabelField($"D{victim.foundDay} 발견   {victim.victim}");
        }
    }

    private List<int> CollectDays(IReadOnlyList<ContactInfo> contacts)
    {
        List<int> days = new List<int>();

        foreach (ContactInfo contact in contacts)
        {
            if (days.Contains(contact.day)) continue;

            days.Add(contact.day);
        }

        days.Sort();

        return days;
    }

    // 전해 들은 정보가 뒤늦게 들어오므로 저장 순서가 시간순이 아니다
    private List<ContactInfo> CollectDailyContacts(IReadOnlyList<ContactInfo> contacts, int day)
    {
        List<ContactInfo> daily = new List<ContactInfo>();

        foreach (ContactInfo contact in contacts)
        {
            if (contact.day != day) continue;

            daily.Add(contact);
        }

        daily.Sort((a, b) => a.hour.CompareTo(b.hour));

        return daily;
    }
}
#endif
