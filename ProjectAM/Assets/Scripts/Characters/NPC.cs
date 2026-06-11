using Unity.Behavior;
using UnityEngine;

public class NPC : MonoBehaviour, IMovable
{
    private Animator animator;
    private BehaviorGraphAgent behaviorGraph;
    private Movement movement;

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
}
