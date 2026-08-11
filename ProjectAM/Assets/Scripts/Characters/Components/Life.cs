using UnityEngine;

/// <summary>
/// NPC의 생사를 관리하는 클래스.
/// 사망하면 집을 비우고 사라진다. 남은 빈 집이 나중에 희생 확인의 단서가 된다.
/// </summary>
public class Life : MonoBehaviour
{
    [SerializeField]
    private bool isAlive = true;
    [SerializeField]
    private int deathDay = 0;

    private NPC owner;
    private Conversation conversation;
    private HouseEntry houseEntry;

    public bool IsAlive => isAlive;
    public int DeathDay => deathDay;

    private void Awake()
    {
        owner = GetComponent<NPC>();
        conversation = GetComponent<Conversation>();
        houseEntry = GetComponent<HouseEntry>();
    }

    public void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        deathDay = World.Instance.Time.Day;

        if (conversation.Partner != null) Conversation.Unpair(owner, conversation.Partner);

        houseEntry.Vacate();
        gameObject.SetActive(false);
    }
}
