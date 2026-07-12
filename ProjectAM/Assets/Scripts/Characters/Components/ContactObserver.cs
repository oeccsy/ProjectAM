using UnityEngine;

// 시야 안에서 대화 중인 NPC 쌍을 발견하면 그 사실을 메모리에 기록한다.
public class ContactObserver : MonoBehaviour
{
    private const float ScanInterval = 0.5f;

    private NPC owner;
    private float scanTimer;

    private void Awake()
    {
        owner = GetComponent<NPC>();
    }

    private void Update()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer < ScanInterval) return;
        scanTimer = 0f;

        Scan();
    }

    private void Scan()
    {
        int day = World.Instance.Time.Day;
        int hour = (int)World.Instance.Time.Hour;

        foreach (NPC seen in owner.Perception.FindVisibleNpcs())
        {
            if (seen.Interaction.State != InteractionState.Talking) continue;

            NPC partner = seen.Interaction.Partner;
            if (partner == null) continue;

            owner.Memory.Remember(new ContactClue(day, hour, seen.OwnColor, partner.OwnColor));
        }
    }
}
