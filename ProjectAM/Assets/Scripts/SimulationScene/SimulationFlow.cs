using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시뮬레이션 하루 흐름을 관리하는 클래스.
/// 시간 단계는 시계(TimeSystem)가 정하고, 이 클래스는 단계에 맞춰 마을 사건(투표, 밤 사건)을 진행한다.
/// 사건 조건이 단계가 끝날 때까지 충족되지 않으면 그 사건은 조용히 건너뛴다.
/// </summary>
public class SimulationFlow : MonoBehaviour
{
    private WaitUntil waitUntilDusk;
    private WaitUntil waitUntilNight;
    private WaitUntil waitUntilDay;
    private WaitUntil waitUntilAllGatheredOrDuskEnds;
    private WaitUntil waitUntilAllReturnedOrNightEnds;
    private Coroutine flowRoutine;

    private void Awake()
    {
        waitUntilDusk = new WaitUntil(() => World.Instance.Time.Phase == TimePhase.Dusk);
        waitUntilNight = new WaitUntil(() => World.Instance.Time.Phase == TimePhase.Night);
        waitUntilDay = new WaitUntil(() => World.Instance.Time.Phase == TimePhase.Day);

        waitUntilAllGatheredOrDuskEnds = new WaitUntil(() => AllCitizensGatheredAtSquare() || World.Instance.Time.Phase != TimePhase.Dusk);
        waitUntilAllReturnedOrNightEnds = new WaitUntil(() => AllCitizensReturnedHome() || World.Instance.Time.Phase != TimePhase.Night);
    }

    public void StartSimulation()
    {
        flowRoutine = StartCoroutine(SimulationRoutine());
    }

    private IEnumerator SimulationRoutine()
    {
        DistributeRoles();

        while (true)
        {
            yield return waitUntilDay;

            yield return waitUntilDusk;
            yield return waitUntilAllGatheredOrDuskEnds;

            // 투표

            yield return waitUntilNight;
            yield return waitUntilAllReturnedOrNightEnds;

            // 희생자 발생
        }
    }

    private void DistributeRoles()
    {
        IReadOnlyList<NPC> npcs = World.Instance.NPCs.All;
        if (npcs.Count == 0) return;

        int vampireIndex = Random.Range(0, npcs.Count);

        for (int i = 0; i < npcs.Count; i++)
        {
            Role role = (i == vampireIndex) ? Role.Vampire : Role.Citizen;
            npcs[i].ApplyRole(role);
        }
    }

    private bool AllCitizensGatheredAtSquare()
    {
        return false;
    }

    private bool AllCitizensReturnedHome()
    {
        return false;
    }
}
