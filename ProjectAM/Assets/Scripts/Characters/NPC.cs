using Unity.Behavior;
using UnityEngine;

public class NPC : MonoBehaviour, IMovable
{
    [SerializeField]
    private NpcColor ownColor = NpcColor.Count;

    private Animator animator;
    private BehaviorGraphAgent behaviorGraph;
    private Movement movement;

    public NpcColor OwnColor => ownColor;
    public Movement Movement => movement;
    public Vector2Int CurrentTile => movement.CurrentTile;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<Movement>();

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
    }
}
