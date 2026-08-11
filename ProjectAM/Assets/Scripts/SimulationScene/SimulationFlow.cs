using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시뮬레이션 흐름을 관리하는 클래스.
/// 시간 단계는 TimeSystem가 정하고, 이 클래스는 단계에 맞춰 사건(투표, 희생자 처리)을 진행한다.
/// </summary>
public class SimulationFlow : MonoBehaviour
{
    [SerializeField]
    private float focusDuration = 10f;
    [SerializeField]
    private float ghostDelay = 2.5f;
    [SerializeField]
    private float noticeDuration = 5f;
    [SerializeField]
    private float ghostHeight = 2.5f;

    private readonly List<ContactInfo> dailyContacts = new List<ContactInfo>();

    private WaitUntil waitUntilDay;
    private WaitUntil waitUntilDusk;
    private WaitUntil waitUntilEvening;
    private WaitUntil waitUntilNight;
    private WaitUntil waitUntilAllGatheredOrDuskEnds;
    private WaitUntil waitUntilAllReturnedOrNightEnds;
    private Coroutine flowRoutine;
    private Observer observer;
    private NoticeUI noticeUI;

    private void Awake()
    {
        observer = FindFirstObjectByType<Observer>();
        noticeUI = NoticeUI.Create();

        waitUntilDay = new WaitUntil(() => World.Instance.Time.Phase == TimePhase.Day);
        waitUntilDusk = new WaitUntil(() => World.Instance.Time.Phase == TimePhase.Dusk);
        waitUntilEvening = new WaitUntil(() => World.Instance.Time.Phase == TimePhase.Evening);
        waitUntilNight = new WaitUntil(() => World.Instance.Time.Phase == TimePhase.Night);

        waitUntilAllGatheredOrDuskEnds = new WaitUntil(() => AllCitizensGatheredAtSquare() || World.Instance.Time.Phase != TimePhase.Dusk);
        waitUntilAllReturnedOrNightEnds = new WaitUntil(() => AllCitizensReturnedHome() || World.Instance.Time.Phase != TimePhase.Night);
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

        while (true)
        {
            yield return waitUntilDay;

            yield return waitUntilDusk;
            // yield return waitUntilAllGatheredOrDuskEnds;

            // 투표

            yield return waitUntilEvening;
            yield return waitUntilNight;
            // yield return waitUntilAllReturnedOrNightEnds;

            // 희생자 발생
            yield return KillVictim();

            dailyContacts.Clear();
        }
    }

    private void DistributeRoles()
    {
        IReadOnlyList<NPC> npcs = World.Instance.NPCs.All;
        if (npcs.Count == 0) return;

        int vampireIndex = Random.Range(0, npcs.Count);

        for (int i = 0; i < npcs.Count; i++)
        {
            Role role = (i == vampireIndex) ? Role.Vampire : Role.Citizen;
            npcs[i].ApplyRole(role);
        }
    }

    private bool AllCitizensGatheredAtSquare()
    {
        return true;
    }

    private bool AllCitizensReturnedHome()
    {
        if (World.Instance.NPCs == null) return false;

        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (!npc.Life.IsAlive) continue;
            if (!npc.HouseEntry.IsInsideHouse) return false;
        }

        return true;
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

        NPC victim = candidates[Random.Range(0, candidates.Count)];
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
