using Unity.Behavior;
using UnityEngine;

/// <summary>
/// NPC 객체를 표현하기 위해 필요한 여러 컴포넌트를 감싸고 있는 클래스
/// </summary>
public class NPC : MonoBehaviour, IMovable
{
    [SerializeField]
    private NpcColor ownColor = NpcColor.Count;
    [SerializeField]
    private Role role = Role.Citizen;
    private BehaviorGraphAgent behaviorGraph;
    private Movement movement;
    private Appearance appearance;
    private HouseEntry houseEntry;
    private Perception perception;
    private NpcMemory npcMemory;
    private NpcAnimation npcAnimation;
    private Conversation conversation;
    private Witness witness;
    private Life life;

    public NpcColor OwnColor => ownColor;
    public Role Role => role;
    public Life Life => life;
    public Movement Movement => movement;
    public Appearance Appearance => appearance;
    public HouseEntry HouseEntry => houseEntry;
    public Perception Perception => perception;
    public NpcMemory NpcMemory => npcMemory;
    public NpcAnimation NpcAnimation => npcAnimation;
    public Conversation Conversation => conversation;
    public Witness Witness => witness;
    public Vector2Int CurrentTile => movement.CurrentTile;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        appearance = GetComponent<Appearance>();
        houseEntry = GetComponent<HouseEntry>();
        perception = GetComponent<Perception>();
        npcMemory = GetComponent<NpcMemory>();
        npcAnimation = GetComponent<NpcAnimation>();
        conversation = GetComponent<Conversation>();
        witness = GetComponent<Witness>();
        life = GetComponent<Life>();

        behaviorGraph = GetComponent<BehaviorGraphAgent>();
        behaviorGraph.SetVariableValue<NPC>("NPC", this);
    }

    public void Init(NpcColor color)
    {
        if(ownColor != NpcColor.Count) return;
        ownColor = color;

        appearance.ApplyColor(ownColor);
    }

    public void ApplyRole(Role role)
    {
        this.role = role;
    }
}
