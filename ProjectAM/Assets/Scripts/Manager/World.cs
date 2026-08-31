using UnityEngine;

public class World : Singleton<World>
{
    // Map
    public MapData MapData { get; set; }
    public MapRuntime MapRuntime { get; set; }
    public GameObject Terrain { get; set; }
    public TerrainScaleSettings TerrainScaleSettings { get; set; }

    // Structures
    public Registry<House, NpcColor> Houses { get; set; }
    public Square Square { get; set; }

    // Npcs
    public Registry<NPC, NpcColor> NPCs { get; set; }

    // Systems
    public TimeSystem Time { get; set; }
}