using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// 하루 루프의 오케스트레이터.
// 시간 단계 전환을 받아 해질녘 추방, 밤 사건을 순서대로(겹치지 않게) 진행한다.
public class GameFlow : MonoBehaviour
{
    private const float StopTimeout = 5f;
    private const float GatherTimeout = 15f;
    private const float PointDuration = 1.5f;
    private const float ResultDuration = 5f;
    private const string ScenePrefabPath = "Prefabs/SimulationScene";

    private readonly NightEvent nightEvent = new NightEvent();
    private readonly BanishmentEvent banishmentEvent = new BanishmentEvent();
    private readonly Queue<IEnumerator> pendingEvents = new Queue<IEnumerator>();
    private bool eventRunning;
    private bool gameEnded;

    private void Start()
    {
        World.Instance.Time.PhaseChanged += OnPhaseChanged;
    }

    private void OnDestroy()
    {
        if (World.Instance.Time != null)
        {
            World.Instance.Time.PhaseChanged -= OnPhaseChanged;
        }
    }

    private void OnPhaseChanged(TimePhase phase)
    {
        if (gameEnded) return;

        if (phase == TimePhase.Dusk) EnqueueEvent(RunBanishment());
        if (phase == TimePhase.Night) EnqueueEvent(RunNightEvent());
    }

    // 이벤트 연출이 서로 겹치지 않도록 큐로 직렬화한다
    private void EnqueueEvent(IEnumerator gameEvent)
    {
        pendingEvents.Enqueue(gameEvent);
        if (!eventRunning) StartCoroutine(RunPendingEvents());
    }

    private IEnumerator RunPendingEvents()
    {
        eventRunning = true;

        while (pendingEvents.Count > 0)
        {
            yield return StartCoroutine(pendingEvents.Dequeue());
        }

        eventRunning = false;
    }

    // ---- 밤 사건 ----

    private IEnumerator RunNightEvent()
    {
        NPC victim = nightEvent.SelectVictim();
        World.Instance.Contacts.Clear();   // 접촉 기록은 하루짜리

        if (victim == null) yield break;   // 조용한 밤

        // 하던 일을 멈추고 마지막 걸음이 끝나기를 기다린다
        victim.SetBrainActive(false);
        victim.Movement.RequestStop();
        while (victim.Movement.State != MoveState.Idle) yield return null;

        // 집 밖이라면 사라짐 연출을 보여준다
        if (victim.HouseEntry.CurrentHouse == null)
        {
            bool vanished = false;
            victim.GetComponent<NpcAnimation>().PlayVanish(() => vanished = true);
            while (!vanished) yield return null;
        }

        victim.Vanish(LifeState.Victim);
        Debug.Log($"[GameFlow] Night {World.Instance.Time.Day} : {victim.OwnColor} disappeared.");

        TryEndGame();
    }

    // ---- 해질녘 추방 ----

    private IEnumerator RunBanishment()
    {
        NPC accused = banishmentEvent.SelectAccused(World.Instance.Config.banishSuspicionThreshold);
        if (accused == null) yield break;

        List<NPC> villagers = FindAliveNpcs();

        yield return StopAllVillagers(villagers);
        ExitHouseOccupants(villagers);

        // 광장 소집
        List<Vector2Int> gatherTiles = CollectGatherTiles(World.Instance.Square.origin, villagers.Count);
        for (int i = 0; i < villagers.Count && i < gatherTiles.Count; i++)
        {
            villagers[i].Movement.StartMoveTo(gatherTiles[i]);
        }
        yield return WaitUntilAllIdle(villagers, GatherTimeout);

        // 늦은 인원은 제자리에 세운다
        foreach (NPC npc in villagers)
        {
            npc.Movement.RequestStop();
        }
        yield return WaitUntilAllIdle(villagers, StopTimeout);

        // 지목: 모두가 대상을 바라본다
        foreach (NPC npc in villagers)
        {
            if (npc == accused) continue;
            FaceTo(npc.transform, accused.transform.position);
        }
        yield return new WaitForSeconds(PointDuration);

        // 퇴장
        bool vanished = false;
        accused.GetComponent<NpcAnimation>().PlayVanish(() => vanished = true);
        while (!vanished) yield return null;

        accused.Vanish(LifeState.Banished);
        Debug.Log($"[GameFlow] Day {World.Instance.Time.Day} : {accused.OwnColor} was banished. (culprit: {accused.Role == Role.Culprit})");

        TryEndGame();
        if (gameEnded) yield break;   // 판이 끝났으면 일상을 재개하지 않는다

        foreach (NPC npc in villagers)
        {
            if (npc == accused) continue;
            npc.SetBrainActive(true);
        }
    }

    // ---- 승패 & 재시작 ----

    private void TryEndGame()
    {
        if (gameEnded) return;

        GameResult? result = WinCondition.Evaluate();
        if (result == null) return;

        gameEnded = true;
        pendingEvents.Clear();

        StartCoroutine(RunGameEnd(result.Value));
    }

    private IEnumerator RunGameEnd(GameResult result)
    {
        foreach (NPC npc in FindAliveNpcs())
        {
            npc.SetBrainActive(false);
            npc.Movement.RequestStop();
        }

        string message = (result == GameResult.CitizensWin)
            ? "시민 승리!\n범인이 마을에서 추방되었습니다."
            : "범인 승리...\n마을이 조용해졌습니다.";

        ResultUI.Show(message, transform);
        Debug.Log($"[GameFlow] Game over : {result}");

        yield return new WaitForSeconds(ResultDuration);

        // 새 판 시작: 이 판의 산출물을 모두 지우고 씬 프리팹을 다시 연다
        Destroy(World.Instance.SceneRoot);
        yield return null;

        Instantiate(Resources.Load<GameObject>(ScenePrefabPath));
        Destroy(gameObject);
    }

    // 전원 정지. 집 출입 연출(Busy) 중인 NPC는 연출이 끝난 뒤 세운다.
    private IEnumerator StopAllVillagers(List<NPC> villagers)
    {
        List<NPC> stopped = new List<NPC>();
        float elapsed = 0f;

        while (stopped.Count < villagers.Count && elapsed < StopTimeout)
        {
            foreach (NPC npc in villagers)
            {
                if (stopped.Contains(npc)) continue;
                if (npc.Interaction.Busy) continue;

                npc.SetBrainActive(false);
                npc.Interaction.Release();
                npc.Movement.RequestStop();
                stopped.Add(npc);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return WaitUntilAllIdle(villagers, StopTimeout);
    }

    private void ExitHouseOccupants(List<NPC> villagers)
    {
        foreach (NPC npc in villagers)
        {
            if (npc.HouseEntry.CurrentHouse == null) continue;

            npc.HouseEntry.Exit();
            npc.transform.localScale = Vector3.one;   // 입장 연출로 줄어든 스케일 복원
        }
    }

    private IEnumerator WaitUntilAllIdle(List<NPC> npcs, float timeout)
    {
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            bool allIdle = true;
            foreach (NPC npc in npcs)
            {
                if (npc.Movement.State != MoveState.Idle) allIdle = false;
            }

            if (allIdle) yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // 광장 중심에서 바깥으로 링을 넓혀 가며 모일 타일을 사람 수만큼 모은다
    private List<Vector2Int> CollectGatherTiles(Vector2Int center, int amount)
    {
        MapData mapData = World.Instance.MapData;
        List<Vector2Int> tiles = new List<Vector2Int>();

        for (int radius = 1; radius < 12 && tiles.Count < amount; radius++)
        {
            for (int row = center.y - radius; row <= center.y + radius; row++)
            {
                for (int col = center.x - radius; col <= center.x + radius; col++)
                {
                    if (tiles.Count >= amount) break;

                    int ringDistance = Mathf.Max(Mathf.Abs(row - center.y), Mathf.Abs(col - center.x));
                    if (ringDistance != radius) continue;

                    if (row < 0 || col < 0 || row >= mapData.resolution.y || col >= mapData.resolution.x) continue;
                    if (!Movement.MovableTypes.Contains(mapData.fieldTypes[row, col])) continue;

                    tiles.Add(new Vector2Int(col, row));
                }
            }
        }

        return tiles;
    }

    private List<NPC> FindAliveNpcs()
    {
        List<NPC> alive = new List<NPC>();

        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (npc.IsAlive) alive.Add(npc);
        }

        return alive;
    }

    private void FaceTo(Transform actor, Vector3 worldPosition)
    {
        Vector3 direction = worldPosition - actor.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        actor.DORotateQuaternion(Quaternion.LookRotation(direction), 0.4f);
    }
}
