using System.Collections.Generic;
using UnityEngine;

public class World : Singleton<World>
{
    public MapRuntime MapRuntime { get; set; }
    public MapData MapData { get; set; }
    public TerrainScaleSettings TerrainScaleSettings { get; set; }

    public GameObject Terrain { get; set; }
    public List<House> Houses { get; set; }
    public Square Square { get; set; }

    public List<NPC> NPCs { get; set; }
}