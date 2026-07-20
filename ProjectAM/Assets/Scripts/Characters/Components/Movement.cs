using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 캐릭터의 타일 기반 이동을 처리하는 클래스.
/// A* 경로 탐색을 통해 목적지까지의 경로를 계산하고, 타일 점유 상황에 따라 대기/이동 상태를 전환한다.
/// </summary>
public class Movement : MonoBehaviour
{
    public static readonly HashSet<char> MovableTypes = new HashSet<char> { 'A', 'B', 'S', 'Y' };

    private const float MoveSpeed = 1.0f;
    private const float RotationSpeed = 270f;
    private const float ArriveThreshold = 0.02f;

    private IMovable owner;
    private Rigidbody rigidbody;
   [SerializeField]
    private MoveState moveState = MoveState.Idle;

    private Coroutine followPathRoutine;
    private WaitUntil waitUntilMovable;
    private WaitUntil waitUntilArrived;

    private Astar astar;
    private List<Vector2Int> path;
    private int pathIndex;
    private Vector2Int prevTile;
    private Vector2Int curTile;
    private Vector2Int nextTile;

    public Vector3 Look { get; set; }

    public MoveState State => moveState;
    public Vector2Int CurrentTile => curTile;
    public List<Vector2Int> Path => astar.Path;

    private void Awake()
    {
        owner = GetComponent<IMovable>();
        rigidbody = GetComponent<Rigidbody>();

        waitUntilMovable = new WaitUntil(() => IsMovable(path[pathIndex]));
        waitUntilArrived = new WaitUntil(() => curTile == nextTile);

        astar = new Astar(IsMovable, Astar.HeuristicType.Manhattan);
        curTile = TileCoordinate.WorldToTile(transform.position);

        bool isEmpty = World.Instance.MapRuntime.IsEmpty(curTile);
        if (isEmpty) World.Instance.MapRuntime.Reserve(owner, curTile);
    }

    private void FixedUpdate()
    {
        Rotate(Look);
        Move(nextTile);
    }

    public void StartMoveTo(Vector2Int destTile)
    {
        if (moveState != MoveState.Idle) return;
        if (destTile == curTile) return;

        astar.FindPath(curTile, destTile);
        if (astar.Path.Count <= 1) return;

        path = astar.Path;
        pathIndex = 1;

        followPathRoutine = StartCoroutine(FollowPathRoutine(destTile));
    }

    public void StopMoving()
    {
        if (followPathRoutine == null) return;
        
        if (moveState == MoveState.Waiting) StopCoroutine(followPathRoutine);
        if (moveState == MoveState.Moving) path.Clear(); // 이 다음 경로를 제거하여 현재 이동 마무리 후 중단
    }

    private IEnumerator FollowPathRoutine(Vector2Int destTile)
    {
        while(curTile != destTile)
        {
            Vector2Int pathTile = path[pathIndex];
            
            if (!IsMovable(pathTile))
            {
                moveState = MoveState.Waiting;
                yield return waitUntilMovable;
            }

            nextTile = pathTile;
            World.Instance.MapRuntime.Reserve(owner, nextTile);
            
            Vector3 curTilePos = TileCoordinate.TileToWorld(curTile);
            Vector3 nextTilePos = TileCoordinate.TileToWorld(nextTile);
            Look = nextTilePos - curTilePos;

            moveState = MoveState.Moving;
     
            yield return waitUntilArrived;

            World.Instance.MapRuntime.Release(owner, prevTile);
            
            pathIndex++;
            if(pathIndex >= path.Count)
            {
                moveState = MoveState.Idle;
                yield break;
            }
        }
    }

    private void Rotate(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir);
        rigidbody.rotation = Quaternion.RotateTowards(transform.rotation, look, RotationSpeed * Time.fixedDeltaTime);
    }

    private void Move(Vector2Int nextTile)
    {
        if (moveState != MoveState.Moving) return;
        if (curTile == nextTile) return;

        Vector3 startPos = transform.position;
        Vector3 endPos = TileCoordinate.TileToWorld(nextTile);
        endPos.y = startPos.y;
        
        Vector3 stepPos = Vector3.MoveTowards(startPos, endPos, MoveSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(stepPos, endPos) <= ArriveThreshold)
        {   
            rigidbody.MovePosition(endPos);
            prevTile = curTile;
            curTile = nextTile;
        }
        else
        {
            rigidbody.MovePosition(stepPos);
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
}
