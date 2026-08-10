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
