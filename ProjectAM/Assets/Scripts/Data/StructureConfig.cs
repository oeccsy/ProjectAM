using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureConfig", menuName = "ProjectAM/StructureConfig")]
public class StructureConfig : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string assetName;
        public Vector2Int size;
        public List<Vector2Int> entrances;
    }

    public Entry[] structures;
}
