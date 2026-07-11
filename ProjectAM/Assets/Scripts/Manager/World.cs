using UnityEngine;

public class World : Singleton<World>
{
    // Scene
    public GameObject SceneRoot { get; set; }
    public SimulationConfig Config { get; set; }

    // Map
    public MapData MapData { get; set; }
    public MapRuntime MapRuntime { get; set; }
    public GameObject Terrain { get; set; }
    public TerrainScaleSettings TerrainScaleSettings { get; set; }

    // Structures
    public Registry<House> Houses { get; set; }
    public Square Square { get; set; }

    // Npcs
    public Registry<NPC> NPCs { get; set; }

    // Systems
    public TimeSystem Time { get; set; }
    public ContactLog Contacts { get; set; }
}