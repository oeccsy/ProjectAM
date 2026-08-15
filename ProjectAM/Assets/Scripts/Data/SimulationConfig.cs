using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SimulationConfig", menuName = "ProjectAM/SimulationConfig")]
public class SimulationConfig : ScriptableObject
{
    public int npcCount;
    public int houseType;
}