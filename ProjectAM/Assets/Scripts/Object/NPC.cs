using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public static readonly HashSet<char> MovableTypes = new HashSet<char> { 'A', 'B', 'S' };

    private const float MoveSpeed = 2.0f;
    private const float RotationSpeed = 540f;
    private const float ArriveThreshold = 0.02f;

    private Animator animator;
    private BehaviorGraphAgent behaviorGraph;

    private NpcState state = NpcState.Idle;
    private Vector2Int currentTile;

    private readonly List<Vector2Int> path = new List<Vector2Int>();
    private int pathIndex;

    public NpcState State => state;

    public Vector2Int CurrentTile => TileCoordinate.WorldToTile(transform.position);

    private void Awake()
    {
        animator = GetComponent<Animator>();

        behaviorGraph = GetComponent<BehaviorGraphAgent>();
        behaviorGraph.SetVariableValue<NPC>("NPC", this);
    }

    private void Update()
    {
        if (state == NpcState.Walking) FollowPath();
    }

    public void MoveAlong(List<Vector2Int> tilePath)
    {
        path.Clear();
        if (tilePath != null) path.AddRange(tilePath);

        pathIndex = 1;
        state = (path.Count > 1) ? NpcState.Walking : NpcState.Idle;
    }

    public void StopMove()
    {
        state = NpcState.Idle;
        path.Clear();
    }

    private void FollowPath()
    {
        Vector3 targetPos = TileCoordinate.TileToWorld(path[pathIndex]);

        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, targetPos, MoveSpeed * Time.deltaTime);
        transform.position = next;

        Vector3 direction = targetPos - current;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, RotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(next, targetPos) <= ArriveThreshold)
        {
            currentTile = path[pathIndex];
            pathIndex++;
            if (pathIndex >= path.Count) StopMove();
        }
    }
}
