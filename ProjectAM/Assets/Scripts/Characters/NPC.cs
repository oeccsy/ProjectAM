using Unity.AppUI.MVVM;
using Unity.Behavior;
using UnityEngine;

public class NPC : MonoBehaviour, IMovable
{
    [SerializeField]
    private NpcColor ownColor = NpcColor.Count;

    private Animator animator;
    private BehaviorGraphAgent behaviorGraph;
    private Movement movement;
    private Appearance appearance;
    private HouseEntry houseEntry;

    public NpcColor OwnColor => ownColor;
    public Movement Movement => movement;
    public Appearance Appearance => appearance;
    public HouseEntry HouseEntry => houseEntry;
    public Vector2Int CurrentTile => movement.CurrentTile;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<Movement>();
        appearance = GetComponent<Appearance>();
        houseEntry = GetComponent<HouseEntry>();

        behaviorGraph = GetComponent<BehaviorGraphAgent>();
        behaviorGraph.SetVariableValue<NPC>("NPC", this);
    }

    private void Update()
    {
        animator.SetInteger("State", (int)movement.State);
    }

    public void Init(NpcColor color)
    {
        if(ownColor != NpcColor.Count) return;
        ownColor = color;

        appearance.ApplyColor(ownColor);
    }
}
