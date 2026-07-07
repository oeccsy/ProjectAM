using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureConfig", menuName = "ProjectAM/StructureConfig")]
public class StructureConfig : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string assetName;
        public Vector2Int size;
        public Vector2Int originOffset;
    }

    public Entry[] structures;
}
