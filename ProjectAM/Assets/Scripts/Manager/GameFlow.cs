using System.Collections;
using UnityEngine;

// 하루 루프의 오케스트레이터. 시간 단계 전환을 받아 밤 사건 등을 순서대로 진행한다.
public class GameFlow : MonoBehaviour
{
    private readonly NightEvent nightEvent = new NightEvent();

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
        if (phase == TimePhase.Night) StartCoroutine(RunNightEvent());
    }

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
    }
}
