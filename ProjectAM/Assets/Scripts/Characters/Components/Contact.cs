using System;
using UnityEngine;

/// <summary>
/// NPC 간 상호작용을 담당하는 클래스.
/// 접촉이 성사되면 접촉 정보를 만들어 양쪽이 기억하고, static 이벤트로 알린다.
/// </summary>
public class Contact : MonoBehaviour
{
    public static event Action<ContactInfo> OnContact;

    private NPC owner;
    private NpcMemory ownMemory;

    private void Awake()
    {
        owner = GetComponent<NPC>();
        ownMemory = GetComponent<NpcMemory>();
    }

    // 대화 => 서로 상호작용했다는 사실을 기억하고, 아는 정보도 하나씩 교환한다
    public void Talk(NPC other)
    {
        TimeSystem time = World.Instance.Time;
        ContactInfo contact = new ContactInfo(time.Day, time.Hour, owner.OwnColor, other.OwnColor);

        NpcMemory otherMemory = other.NpcMemory;
        ownMemory.Remember(contact);
        otherMemory.Remember(contact);

        ShareRandomInfo(ownMemory, otherMemory);
        ShareRandomInfo(otherMemory, ownMemory);
        
        OnContact?.Invoke(contact);
    }

    // 정보 하나를 무작위로 골라 전달
    private void ShareRandomInfo(NpcMemory src, NpcMemory dest)
    {
        int totalCount = src.ContactInfoList.Count + src.VictimInfoList.Count;
        if (totalCount == 0) return;

        int index = UnityEngine.Random.Range(0, totalCount);

        if (index < src.ContactInfoList.Count)
        {
            dest.Remember(src.ContactInfoList[index]);
        }
        else
        {
            dest.Remember(src.VictimInfoList[index - src.ContactInfoList.Count]);
        }
    }
}
