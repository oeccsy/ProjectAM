using System.Collections.Generic;
using UnityEngine;

public class WorldDataStore : Singleton<WorldDataStore>
{
    public MapData MapData { get; set; }
    public TerrainScaleSettings TerrainScaleSettings { get; set; }

    public GameObject Terrain { get; set; }
    public List<House> Houses { get; set; }
    public Square Square { get; set; }
}