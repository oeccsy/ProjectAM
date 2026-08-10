using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 간 상호작용을 담당하는 클래스.
/// 접촉이 성사되면 접촉 정보를 만들어 양쪽이 기억하고, static 이벤트로 알린다.
/// </summary>
public class Conversation : MonoBehaviour
{
    private NPC owner;
    private NpcMemory ownMemory;

    [field: SerializeField]
    public ConversationState ConversationState { get; private set; }
    [field: SerializeField]
    public ConversationRole ConversationRole { get; private set; } 
    [field: SerializeField]
    public NPC Partner { get; private set; }

    public bool IsTalkable => ConversationState == ConversationState.Talkable;

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

        caller.Conversation.ConversationState = ConversationState.Paired;
        callee.Conversation.ConversationState = ConversationState.Paired;
        caller.Conversation.ConversationRole = ConversationRole.Caller;
        callee.Conversation.ConversationRole = ConversationRole.Callee;
        caller.Conversation.Partner = callee;
        callee.Conversation.Partner = caller;
    }

    public static void Talk(NPC npcA, NPC npcB)
    {
        TimeSystem time = World.Instance.Time;
        ContactInfo contact = new ContactInfo(time.Day, time.Hour, npcA.OwnColor, npcB.OwnColor);

        Debug.Log($"[대화] D{time.Day} {time.Hour}시 {npcA.OwnColor} ↔ {npcB.OwnColor}");

        npcA.Conversation.ShareRandomInfo(npcB);
        npcB.Conversation.ShareRandomInfo(npcA);

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
        npcA.Conversation.ConversationState = ConversationState.Cooldown;
        npcB.Conversation.ConversationState = ConversationState.Cooldown;

        npcA.Conversation.StartCoroutine(npcA.Conversation.TalkCooldown());
        npcB.Conversation.StartCoroutine(npcB.Conversation.TalkCooldown());
    }

    public void StartCallerRoutine()
    {
        StartCoroutine(CallerRoutine());
    }

    public void StartCalleeRoutine()
    {
        StartCoroutine(CalleeRoutine());
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
            ContactInfo info = ownMemory.ContactInfoList[index];
            int beforeCount = otherMemory.ContactInfoList.Count;

            otherMemory.Remember(info);
            LogShare(other, info.ToString(), otherMemory.ContactInfoList.Count != beforeCount);
        }
        else
        {
            int actualIndex = index - ownMemory.ContactInfoList.Count;
            VictimInfo info = ownMemory.VictimInfoList[actualIndex];
            int beforeCount = otherMemory.VictimInfoList.Count;

            otherMemory.Remember(info);
            LogShare(other, info.ToString(), otherMemory.VictimInfoList.Count != beforeCount);
        }
    }

    private void LogShare(NPC other, string info, bool isNew)
    {
        string result = isNew ? "신규" : "이미 앎";

        Debug.Log($"[교환] {owner.OwnColor} → {other.OwnColor} : {info} ({result})");
    }

    private IEnumerator CallerRoutine()
    {
        yield return new WaitUntil(() => (owner.Movement.State == MoveState.Idle) && (Partner.Movement.State == MoveState.Idle));

        int partnerRow = Partner.CurrentTile.y;
        int partnerCol = Partner.CurrentTile.x;
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int row = partnerRow - 1; row <= partnerRow + 1; row++)
        {
            for (int col = partnerCol - 1; col <= partnerCol + 1; col++)
            {
                Vector2Int dest = new Vector2Int(col, row);
                if (owner.Movement.IsMovable(dest)) candidates.Add(dest);
            }
        }
        
        if(candidates.Count <= 0)
        {
            Debug.Log("CallerRoutine Fail");
            yield break;
        }

        Utils.Shuffle(candidates);
        owner.Movement.StartMoveTo(candidates[0]);

        yield return new WaitUntil(() => TileCoordinate.CalcChebyshevDist(owner.CurrentTile, Partner.CurrentTile) <= 1 && owner.Movement.State == MoveState.Idle);

        ConversationState = ConversationState.Talking;
        owner.Movement.Look = Partner.transform.position;
        Talk(owner, Partner);

        yield return new WaitForSeconds(3f);

        Unpair(owner, Partner);
    }

    private IEnumerator CalleeRoutine()
    {
        yield return new WaitUntil(() => owner.Movement.State == MoveState.Idle);
        
        while(true)
        {
            owner.Movement.Look = Partner.transform.position;
            if (Partner.Conversation.ConversationState == ConversationState.Talking) break;
            yield return null;
        }

        owner.Movement.Look = Partner.transform.position;
        ConversationState = ConversationState.Talking;

        yield return new WaitForSeconds(3f);
    }

    private IEnumerator TalkCooldown()
    {
        yield return new WaitForSeconds(30f);
        ConversationState = ConversationState.Talkable;
    }
}
