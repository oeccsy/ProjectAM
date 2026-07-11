using System;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public static readonly HashSet<char> MovableTypes = new HashSet<char> { 'A', 'B', 'S', 'Y' };

    private const float MoveSpeed = 1.0f;
    private const float RotationSpeed = 270f;
    private const float ArriveThreshold = 0.02f;

    private IMovable owner;
    private Rigidbody rigidbody;
    [field: SerializeField]
    private MoveState moveState = MoveState.Idle;

    private Astar astar;
    private List<Vector2Int> path;
    private int pathIndex;
    private Vector2Int curTile;
    private Vector2Int nextTile;
    private bool stopRequested;


    public MoveState State => moveState;
    public Vector2Int CurrentTile => curTile;

#if UNITY_EDITOR
    private NpcPathDebugRenderer pathDebug;
#endif

    private void Awake()
    {
        owner = GetComponent<IMovable>();
        rigidbody = GetComponent<Rigidbody>();

        curTile = TileCoordinate.WorldToTile(transform.position);

        bool isEmpty = World.Instance.MapRuntime.IsEmpty(curTile);
        if (isEmpty) World.Instance.MapRuntime.Reserve(owner, curTile);

        MapData mapData = World.Instance.MapData;
        astar = new Astar(IsMovable, Astar.HeuristicType.Manhattan);

#if UNITY_EDITOR
        pathDebug = GetComponent<NpcPathDebugRenderer>();
#endif
    }

    private void FixedUpdate()
    {
        if (moveState == MoveState.Waiting) WaitUntilMovable();
        if (moveState == MoveState.Moving) FollowPath();
    }

    public void StartMoveTo(Vector2Int destTile)
    {
        if (moveState != MoveState.Idle) return;
        if (destTile == curTile) return;

        astar.FindPath(curTile, destTile);
        if (astar.Path.Count <= 1) return;

        path = astar.Path;
        pathIndex = 1;
        nextTile = path[pathIndex];

#if UNITY_EDITOR
        if (pathDebug != null) pathDebug.RegisterPath(path);
#endif

        WaitUntilMovable();
    }

    private void WaitUntilMovable()
    {
        if (IsMovable(nextTile))
        {
            moveState = MoveState.Moving;
            World.Instance.MapRuntime.Reserve(owner, nextTile);
        }
        else
        {
            moveState = MoveState.Waiting;
        }
    }

    private void FollowPath()
    {
        Vector3 curPos = transform.position;
        Vector3 destPos = TileCoordinate.TileToWorld(nextTile);
        destPos.y = curPos.y;

        Vector3 nextPos = Vector3.MoveTowards(curPos, destPos, MoveSpeed * Time.fixedDeltaTime);
        
        Vector3 dir = destPos - curPos;
        dir.y = 0f;

        // 회전
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            rigidbody.rotation = Quaternion.RotateTowards(transform.rotation, look, RotationSpeed * Time.fixedDeltaTime);
        }

        // 이동
        if (Vector3.Distance(nextPos, destPos) <= ArriveThreshold)
        {   
            rigidbody.MovePosition(destPos);
            
            World.Instance.MapRuntime.Release(owner, curTile);
            curTile = nextTile;
        }
        else
        {
            rigidbody.MovePosition(nextPos);
        }

        // 목표지점 재설정
        if (curTile == nextTile)
        {
            if (stopRequested)
            {
                Finish();
                return;
            }

            pathIndex++;
            if (pathIndex >= path.Count)
            {
                Finish();
                return;
            }

            nextTile = path[pathIndex];
            WaitUntilMovable();
        }
    }

    private bool IsMovable(Vector2Int tile)
    {
        MapData mapData = World.Instance.MapData;

        if (tile.x < 0 || tile.x >= mapData.resolution.x) return false;
        if (tile.y < 0 || tile.y >= mapData.resolution.y) return false;

        if (!MovableTypes.Contains(mapData.fieldTypes[tile.y, tile.x])) return false;
        if (!World.Instance.MapRuntime.IsEmpty(tile)) return false;

        return true;
    }

    // 시작된 걸음은 원자적이므로, 진행 중인 걸음을 마친 뒤 멈춘다.
    public void RequestStop()
    {
        if (moveState == MoveState.Idle) return;
        if (moveState == MoveState.Waiting)
        {
            Finish();
            return;
        }

        stopRequested = true;
    }

    public void Finish()
    {
        moveState = MoveState.Idle;
        stopRequested = false;

#if UNITY_EDITOR
        if (pathDebug != null) pathDebug.ClearPath();
#endif
    }    
}
