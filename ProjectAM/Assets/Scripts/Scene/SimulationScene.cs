using UnityEngine;

/// <summary>
/// SimulationScene의 진입점 역할을 하는 클래스
/// </summary>
public class SimulationScene : MonoBehaviour
{
    private void Awake()
    {
        CellularAutomataSettings settings = new CellularAutomataSettings
        {
            resolution = new Vector2Int(64, 64),
            fillPercentage = 50,
            smoothIterations = 5,
            blurIterations = 3,
            neighborRange = 2
        };

        CellularAutomata cellularAutomata = new CellularAutomata();
        cellularAutomata.GenerateCellularMap(settings);

        MapDataGenerator mapDataGenerator = new MapDataGenerator();
        MapData mapData = mapDataGenerator.GenerateMapData(cellularAutomata);
        mapDataGenerator.PrintMapData();

        TerrainScaleSettings terrainScaleSettings = new TerrainScaleSettings
        {
            tileSize = 1.0f,
            topHeight = 1.0f,
            bottomHeight = 0.0f
        };

        MapGenerator mapGenerator = new MapGenerator();
        mapGenerator.GenerateMap(mapData, terrainScaleSettings);

        World world = World.Instance;
        world.TerrainScaleSettings = terrainScaleSettings;
        world.MapData = mapData;
        world.MapRuntime = new MapRuntime(mapData.resolution);
        world.Terrain = mapGenerator.Terrain;
        world.Houses = new Registry<House, NpcColor>();
        world.Square = mapGenerator.Square;
        world.NPCs = new Registry<NPC, NpcColor>();
        world.Time = new GameObject("TimeSystem").AddComponent<TimeSystem>();

        foreach (House house in mapGenerator.Houses)
        {
            World.Instance.Houses.Register(house.owner, house);
        }

        SimulationConfig simulationConfig = Resources.Load<SimulationConfig>("Data/SimulationConfig");
        if (simulationConfig == null)
        {
            Debug.LogWarning("SimulationConfig not found at Resources/Data/SimulationConfig.");
            return;
        }

        NPCSpawner npcSpawner = new NPCSpawner();
        npcSpawner.Spawn(simulationConfig.npcCount);
        
        Light sun = GetComponentInChildren<Light>();
        DayLight dayLight = new GameObject("DayLight").AddComponent<DayLight>();
        dayLight.BindSun(sun);

        SimulationFlow simulationFlow = new GameObject("SimulationFlow").AddComponent<SimulationFlow>();
        simulationFlow.StartSimulation();
    }
}
