using System.Collections;
using UnityEngine;

/// <summary>
/// 시야 안에서 벌어지는 남의 대화를 목격해 자신의 메모리에 기록하는 클래스.
/// Perception에게 무엇이 보이는지 묻고, NpcMemory에게 기억하라고 시킨다.
/// </summary>
public class Witness : MonoBehaviour
{
    [SerializeField]
    private float observeInterval = 0.5f;

    private NPC owner;
    private Perception perception;
    private NpcMemory ownMemory;
    private Coroutine observeRoutine;

    private void Awake()
    {
        owner = GetComponent<NPC>();
        perception = GetComponent<Perception>();
        ownMemory = GetComponent<NpcMemory>();
    }

    private void OnEnable()
    {
        observeRoutine = StartCoroutine(ObserveRoutine());
    }

    private void OnDisable()
    {
        if (observeRoutine == null) return;

        StopCoroutine(observeRoutine);
        observeRoutine = null;
    }

    // 찾아간 집의 주인이 무사한지 확인한다.
    public void ObserveHouse(House house)
    {
        if (house == null) return;
        if (ownMemory == null) return;
        if (World.Instance.NPCs == null) return;

        TimeSystem time = World.Instance.Time;
        if (time == null) return;

        NPC houseOwner = World.Instance.NPCs.Get(house.owner);
        if (houseOwner == null) return;

        if (houseOwner.Life.IsAlive)
        {
            Debug.Log($"[방문] {owner.OwnColor} : {house.owner}는 잠깐 집을 비운 것 같다");
            return;
        }

        VictimInfo victim = new VictimInfo(house.owner, time.Day);

        int beforeCount = ownMemory.VictimInfoList.Count;
        ownMemory.Remember(victim);

        if (ownMemory.VictimInfoList.Count == beforeCount) return;
        Debug.Log($"[발견] {owner.OwnColor} : {victim}");
    }

    private IEnumerator ObserveRoutine()
    {
        WaitForSeconds waitForInterval = new WaitForSeconds(observeInterval);

        while (true)
        {
            yield return waitForInterval;
            RecordVisibleContacts();
        }
    }

    private void RecordVisibleContacts()
    {
        if (perception == null) return;
        if (ownMemory == null) return;
        if (World.Instance.NPCs == null) return;

        TimeSystem time = World.Instance.Time;
        if (time == null) return;

        foreach (NPC npc in perception.FindVisibleNpcs())
        {
            Conversation conversation = npc.Conversation;
            if (conversation.ConversationState != ConversationState.Talking) continue;

            NPC partner = conversation.Partner;
            if (partner == null) continue;
            if (partner == owner) continue;

            ContactInfo contact = new ContactInfo(time.Day, time.Hour, npc.OwnColor, partner.OwnColor);

            int beforeCount = ownMemory.ContactInfoList.Count;
            ownMemory.Remember(contact);

            if (ownMemory.ContactInfoList.Count == beforeCount) continue;
            Debug.Log($"[목격] {owner.OwnColor} : {contact}");
        }
    }
}
