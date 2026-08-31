using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시뮬레이션 흐름을 관리하는 클래스.
/// 하루를 단계별로 진행시키고, 각 단계에서 벌어지는 사건(희생자 처리)을 다룬다.
/// TimeSystem은 값을 보관할 뿐이고, 언제 넘길지는 이 클래스가 정한다.
/// </summary>
public class SimulationFlow : MonoBehaviour
{
    private const float SecondsPerHour = 10f;
    private const float NightSecondsPerHour = 2f;

    [SerializeField]
    private float focusDuration = 10f;
    [SerializeField]
    private float ghostDelay = 2.5f;
    [SerializeField]
    private float noticeDuration = 5f;
    [SerializeField]
    private float ghostHeight = 2.5f;

    private readonly List<ContactInfo> dailyContacts = new List<ContactInfo>();

    private Coroutine flowRoutine;
    private Observer observer;
    private NoticeUI noticeUI;

    private void Awake()
    {
        observer = FindFirstObjectByType<Observer>();
        noticeUI = NoticeUI.Create();
    }

    private void OnEnable()
    {
        Conversation.OnContact += RecordContact;
    }

    private void OnDisable()
    {
        Conversation.OnContact -= RecordContact;
    }

    public void StartSimulation()
    {
        flowRoutine = StartCoroutine(SimulationRoutine());
    }

    private IEnumerator SimulationRoutine()
    {
        DistributeRoles();

        TimeSystem time = World.Instance.Time;

        while (true)
        {
            yield return time.FlowHourUntil(8);

            time.ApplyPhase(TimePhase.Day);
            time.ApplySecondsPerHour(SecondsPerHour);
            yield return time.FlowHourUntil(17);

            time.ApplyPhase(TimePhase.Dusk);
            yield return time.FlowHourUntil(19);
            yield return new WaitUntil(IsAccusationClosed);

            time.ApplyPhase(TimePhase.Evening);
            yield return time.FlowHourUntil(21);
            yield return new WaitUntil(AllReturnedHome);

            time.ApplyPhase(TimePhase.Night);
            time.ApplySecondsPerHour(NightSecondsPerHour);
            yield return KillVictim();
            
            yield return time.FlowHourUntil(24);
            time.NextDay();
            dailyContacts.Clear();
        }
    }

    private bool IsAccusationClosed()
    {
        return Accusation.Phase == AccusationPhase.None;
    }

    private bool AllReturnedHome()
    {
        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (!npc.Life.IsAlive) continue;
            if (!npc.HouseEntry.IsInsideHouse) return false;
        }

        return true;
    }

    private void DistributeRoles()
    {
        IReadOnlyList<NPC> npcs = World.Instance.NPCs.All;
        if (npcs.Count == 0) return;

        int vampireIndex = UnityEngine.Random.Range(0, npcs.Count);

        for (int i = 0; i < npcs.Count; i++)
        {
            Role role = (i == vampireIndex) ? Role.Vampire : Role.Citizen;
            npcs[i].ApplyRole(role);
        }
    }

    // 낮에 범인과 접촉한 사람 중 1명이 사라진다. 접촉이 없었다면 조용한 밤
    private IEnumerator KillVictim()
    {
        NPC culprit = FindVampire();
        if (culprit == null) yield break;

        int day = World.Instance.Time.Day;
        List<NPC> candidates = FindContactedNpcs(culprit);

        if (candidates.Count == 0)
        {
            Debug.Log($"[조용한 밤] D{day} 범인과 접촉한 사람이 없었다");
            yield break;
        }

        NPC victim = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        victim.Life.Die();

        Debug.Log($"[사망] D{day} {victim.OwnColor} (범인 {culprit.OwnColor} 접촉자 {candidates.Count}명 중)");

        yield return PlayDeathScene(victim.OwnColor);
    }

    // 카메라가 빈 집을 비추고, 유령이 떠오르고, 상황을 한 줄로 알린다
    private IEnumerator PlayDeathScene(NpcColor victimColor)
    {
        House house = World.Instance.Houses.Get(victimColor);
        if (house == null) yield break;

        if (observer != null) observer.FocusOn(house.transform, focusDuration);

        yield return new WaitForSeconds(ghostDelay);

        GhostIcon.Spawn(house.transform.position + Vector3.up * ghostHeight);
        noticeUI.Show($"{victimColor} vanished in the night", noticeDuration);

        yield return new WaitForSeconds(focusDuration - ghostDelay);
    }

    private NPC FindVampire()
    {
        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (!npc.Life.IsAlive) continue;
            if (npc.Role != Role.Vampire) continue;

            return npc;
        }

        return null;
    }

    private List<NPC> FindContactedNpcs(NPC culprit)
    {
        HashSet<NpcColor> contactedColors = new HashSet<NpcColor>();

        foreach (ContactInfo contact in dailyContacts)
        {
            if (!contact.Involves(culprit.OwnColor)) continue;

            contactedColors.Add(contact.OtherOf(culprit.OwnColor));
        }

        List<NPC> candidates = new List<NPC>();

        foreach (NpcColor color in contactedColors)
        {
            NPC npc = World.Instance.NPCs.Get(color);
            if (npc == null) continue;
            if (!npc.Life.IsAlive) continue;

            candidates.Add(npc);
        }

        return candidates;
    }

    private void RecordContact(ContactInfo contact)
    {
        dailyContacts.Add(contact);
    }
}
