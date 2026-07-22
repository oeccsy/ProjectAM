using System;
using UnityEngine;

/// <summary>
/// NPC 간 상호작용을 담당하는 클래스.
/// 접촉이 성사되면 접촉 정보를 만들어 양쪽이 기억하고, static 이벤트로 알린다.
/// </summary>
public class Conversation : MonoBehaviour
{
    private NPC owner;
    private NpcMemory ownMemory;

    public ConversationRole ConversationRole { get; private set; }
    public NPC Partner { get; private set; }
    public bool IsTalkable => ConversationRole == ConversationRole.None;    

    public static event Action<ContactInfo> OnContact;

    private void Awake()
    {
        owner = GetComponent<NPC>();
        ownMemory = GetComponent<NpcMemory>();
    }

    public static void Call(NPC caller, NPC callee)
    {
        if (!caller.Conversation.IsTalkable) return;
        if (!callee.Conversation.IsTalkable) return;

        caller.Conversation.ConversationRole = ConversationRole.Caller;
        callee.Conversation.ConversationRole = ConversationRole.Callee;
        caller.Conversation.Partner = callee;
        callee.Conversation.Partner = caller;
    }

    public static void Talk(NPC npcA, NPC npcB)
    {
        npcA.Conversation.ShareRandomInfo(npcB);
        npcB.Conversation.ShareRandomInfo(npcA);

        TimeSystem time = World.Instance.Time;
        ContactInfo contact = new ContactInfo(time.Day, time.Hour, npcA.OwnColor, npcB.OwnColor);

        NpcMemory memoryA = npcA.NpcMemory;
        NpcMemory memoryB = npcB.NpcMemory;
        memoryA.Remember(contact);
        memoryB.Remember(contact);
        
        OnContact?.Invoke(contact);
    }

    public static void Unpair(NPC npcA, NPC npcB)
    {
        if (npcA.Conversation.Partner == null || npcB.Conversation.Partner == null) return;
        if (npcA.Conversation.Partner != npcB) return;
        if (npcB.Conversation.Partner != npcA) return;

        npcA.Conversation.Partner = null;
        npcB.Conversation.Partner = null;
        npcA.Conversation.ConversationRole = ConversationRole.None;
        npcB.Conversation.ConversationRole = ConversationRole.None;
    }

    // 정보 하나를 무작위로 골라 전달
    private void ShareRandomInfo(NPC other)
    {
        int infoCount = ownMemory.ContactInfoList.Count + ownMemory.VictimInfoList.Count;
        if (infoCount == 0) return;

        int index = UnityEngine.Random.Range(0, infoCount);

        NpcMemory otherMemory = other.NpcMemory;

        if (index < ownMemory.ContactInfoList.Count)
        {
            otherMemory.Remember(ownMemory.ContactInfoList[index]);
        }
        else
        {
            int actualIndex = index - ownMemory.ContactInfoList.Count;
            otherMemory.Remember(ownMemory.VictimInfoList[actualIndex]);
        }
    }
}
