using Unity.AppUI.MVVM;
using Unity.Behavior;
using UnityEngine;

public class NPC : MonoBehaviour, IMovable
{
    [SerializeField]
    private NpcColor ownColor = NpcColor.Count;
    [SerializeField]
    private Role role = Role.Citizen;
    [SerializeField]
    private LifeState lifeState = LifeState.Alive;

    private Animator animator;
    private BehaviorGraphAgent behaviorGraph;
    private Movement movement;
    private Appearance appearance;
    private HouseEntry houseEntry;
    private Perception perception;
    private Interaction interaction;
    private NpcMemory memory;

    public NpcColor OwnColor => ownColor;
    public Role Role => role;
    public LifeState LifeState => lifeState;
    public bool IsAlive => lifeState == LifeState.Alive;
    public Movement Movement => movement;
    public Appearance Appearance => appearance;
    public HouseEntry HouseEntry => houseEntry;
    public Perception Perception => perception;
    public Interaction Interaction => interaction;
    public NpcMemory Memory => memory;
    public Vector2Int CurrentTile => movement.CurrentTile;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<Movement>();
        appearance = GetComponent<Appearance>();
        houseEntry = GetComponent<HouseEntry>();
        perception = GetComponent<Perception>();
        interaction = GetComponent<Interaction>();
        memory = GetComponent<NpcMemory>();

        behaviorGraph = GetComponent<BehaviorGraphAgent>();
        behaviorGraph.SetVariableValue<NPC>("NPC", this);
    }

    private void Update()
    {
        animator.SetInteger("State", (int)movement.State);
    }

    // 두뇌(행동 트리)를 잠시 멈추거나 재개한다. 접촉·이벤트 연출 중 사용.
    public void SetBrainActive(bool active)
    {
        behaviorGraph.enabled = active;
    }

    // 희생/추방으로 마을에서 완전히 사라진다. 연출이 끝난 뒤 호출한다.
    public void Vanish(LifeState cause)
    {
        lifeState = cause;

        interaction.Release();
        SetBrainActive(false);

        if (houseEntry.CurrentHouse != null)
        {
            houseEntry.CurrentHouse.guests.Remove(ownColor);
        }
        else
        {
            World.Instance.MapRuntime.Release(this, movement.CurrentTile);
        }

        gameObject.SetActive(false);
    }

    public void Init(NpcColor color, Role assignedRole)
    {
        if(ownColor != NpcColor.Count) return;
        ownColor = color;
        role = assignedRole;

        appearance.ApplyColor(ownColor);
    }
}
