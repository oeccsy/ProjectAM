using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureConfig", menuName = "ProjectAM/StructureConfig")]
public class StructureConfig : ScriptableObject
{
    [Serializable]
    public struct Info
    {
        public string assetName;
        public Vector2Int tileSize;
        public Vector2Int origin;
    }

    public Info[] structures;
}
