using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SimulationConfig", menuName = "ProjectAM/SimulationConfig")]
public class SimulationConfig : ScriptableObject
{
    public int npcCount;

    // 해질녘 추방이 일어나는 최소 의심치
    public int banishSuspicionThreshold = 3;
}