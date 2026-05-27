using UnityEngine;

public class MapData
{
    public Vector2Int resolution;
    public char[,] map;
    public float[,] value;
    public PerlinNoise perlinNoise;
}
