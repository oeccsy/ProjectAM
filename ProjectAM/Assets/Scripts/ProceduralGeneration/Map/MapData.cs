using System.Collections.Generic;
using UnityEngine;

public class MapData
{
    public Vector2Int resolution;
    public float[,] values;
    public char[,] fieldTypes;
    public int[,] areaID;
    public List<House> houses;
    public Square square;
}