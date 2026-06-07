using System.Collections.Generic;
using UnityEngine;

public class MapDataStore : Singleton<MapDataStore>
{
    public MapData MapData { get; set; }
    public GameObject Terrain { get; set; }
    public List<House> Houses { get; set; }
    public Square Square { get; set; }
}