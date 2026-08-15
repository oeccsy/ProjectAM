using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범인을 고발하는 기능을 다루는 클래스
/// 본인이 범인을 고발하는 경우, 누군가 고발하는 경우 모두를 다룬다.
/// </summary>
public class Accusation : MonoBehaviour
{
    // 마을에 고발은 한 번에 하나만 열린다. 청중은 이것을 보고 자기 차례를 판단한다
    public static AccusationPhase Phase { get; private set; } = AccusationPhase.None;
    public static NPC Accuser { get; private set; }
    public static NPC Accused { get; private set; }
    public static int AgreedCount => agreed.Count;

    private static readonly HashSet<NpcColor> agreed = new HashSet<NpcColor>();
    
    private const float CallDuration = 3.0f;
    private const float VoteDuration = 5.0f;
    private const float RevealDuration = 3.0f;
    private const float VoteDelayMin = 1.0f;
    private const float VoteDelayMax = 4.0f;

    private NPC owner;
    private Movement movement;
    private Coroutine accusationRoutine;

    private void Awake()
    {
        owner = GetComponent<NPC>();
        movement = GetComponent<Movement>();
    }

    public void StartAccuserRoutine(NPC accused)
    {
        Accuser = owner;
        Accused = accused;
        agreed.Clear();
        accusationRoutine = StartCoroutine(AccuserRoutine());
    }

    public void StartAudienceRoutine()
    {
        accusationRoutine = StartCoroutine(AudienceRoutine());
    } 

    private IEnumerator AccuserRoutine()
    {
        // 광장으로 이동했는지 확인
        yield return new WaitUntil(() => movement.State == MoveState.Idle);

        Phase = AccusationPhase.Calling;
        Debug.Log($"[고발] {Accuser.OwnColor} : {Accused.OwnColor}를 지목");

        // 애니메이션 진행
        yield return new WaitForSeconds(CallDuration);
        Phase = AccusationPhase.Gathering;

        // 모두 모일때까지 대기
        yield return new WaitUntil(AllGathered);
        Phase = AccusationPhase.Voting;

        yield return new WaitForSeconds(VoteDuration);
        Phase = AccusationPhase.Revealing;

        bool isExiled = AgreedCount * 2 > CountAlive();
        if (isExiled) Exile(Accused);
        Debug.Log($"[판결] {Accused.OwnColor} : {(isExiled ? "추방" : "무산")} (동의 {AgreedCount}명 / 생존 {CountAlive()}명)");

        yield return new WaitForSeconds(RevealDuration);

        Phase = AccusationPhase.None;
    }

    // 사라지는 연출이 끝나면 마을에서 내보낸다
    private void Exile(NPC exiled)
    {
        exiled.NpcAnimation.SpinWithHideAnim(() => exiled.Life.Die());

        Debug.Log($"[추방] {exiled.OwnColor} : 마을을 떠남");
    }
    
    private bool IsInsideSquare(Vector2Int tile)
    {
        Square square = World.Instance.Square;
        if (square == null) return true;

        if (tile.x < square.anchor.x || tile.x >= square.anchor.x + square.size.x) return false;
        if (tile.y < square.anchor.y || tile.y >= square.anchor.y + square.size.y) return false;

        return true;
    }

    private bool AllGathered()
    {
        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (!npc.Life.IsAlive) continue;
            if (!IsInsideSquare(npc.CurrentTile)) return false;
        }

        return true;
    }

    private int CountAlive()
    {
        int count = 0;

        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (!npc.Life.IsAlive) continue;

            count++;
        }

        return count;
    }

    private IEnumerator AudienceRoutine()
    {
        // 광장으로 이동
        yield return new WaitUntil(() => Phase == AccusationPhase.Gathering);
        movement.StartMoveTo(SelectSquareTile());

        // 도착하면 고발자를 쳐다봄
        yield return new WaitUntil(() => movement.State == MoveState.Idle);
        movement.Look = Accuser.transform.position;

        // 1~4초 부여받고 그 시간에 투표
        yield return new WaitUntil(() => Phase == AccusationPhase.Voting);
        yield return new WaitForSeconds(Random.Range(VoteDelayMin, VoteDelayMax));

        if (AgreesWithAccusation())
        {
            Agree(owner);
            movement.Look = Accused.transform.position;
        }

        yield return new WaitUntil(() => Phase == AccusationPhase.None);
    }

    // 청중 한 명의 동의를 받는다
    private static void Agree(NPC voter)
    {
        if (Phase != AccusationPhase.Voting) return;
        if (voter == null) return;
        if (!agreed.Add(voter.OwnColor)) return;

        Debug.Log($"[동의] {voter.OwnColor} : {Accuser.OwnColor}의 고발에 힘을 보탬");
    }

    // 광장 칸 중 갈 수 있으면서 가장 가까운 자리. 이미 광장 안이면 제자리를 돌려준다
    private Vector2Int SelectSquareTile()
    {
        Vector2Int from = owner.CurrentTile;

        Square square = World.Instance.Square;
        if (square == null) return from;

        Vector2Int nearest = from;
        int nearestDist = int.MaxValue;

        foreach (Vector2Int tile in square.GetApproachTiles())
        {
            if (!movement.IsMovable(tile)) continue;

            int dist = TileCoordinate.CalcManhattanDist(from, tile);
            if (dist >= nearestDist) continue;

            nearest = tile;
            nearestDist = dist;
        }

        return nearest;
    }

    // 내가 가장 의심하는 사람일 때만 동의한다. 아무 근거가 없으면(0점) 동의하지 않는다
    private bool AgreesWithAccusation()
    {
        if (Accused == null) return false;
        if (Accused == owner) return false;

        Suspicion suspicion = owner.Suspicion;

        int accusedScore = suspicion.CalculateSuspicion(Accused.OwnColor);
        if (accusedScore <= 0) return false;

        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (suspicion.CalculateSuspicion(npc.OwnColor) > accusedScore) return false;
        }

        return true;
    }
}