using UnityEngine;

public class SimulationScene : MonoBehaviour
{
    private void Awake()
    {
        NoiseSettings noiseSettings = new NoiseSettings
        {
            resolution = new Vector2Int(64, 64),
            gridSize = new Vector2Int(4, 4),
            seed = Random.Range(0f, 10000f)
        };

        PerlinNoise perlinNoise = new PerlinNoise();
        perlinNoise.GenerateNoise(noiseSettings);
        perlinNoise.PrintNoise();
        DebugUtils.ShowTexture(perlinNoise.NoiseTexture);

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
        world.SceneRoot = gameObject;
        world.MapData = mapData;
        world.MapRuntime = new MapRuntime(mapData.resolution);
        world.Terrain = mapGenerator.Terrain;
        world.TerrainScaleSettings = terrainScaleSettings;
        world.Houses = new Registry<House>();
        world.Square = mapGenerator.Square;
        world.NPCs = new Registry<NPC>();
        world.Time = new GameObject("TimeSystem").AddComponent<TimeSystem>();
        world.Contacts = new ContactLog();

        // 판이 끝나면 씬 루트만 지우면 되도록 생성물을 이 오브젝트 아래로 모은다
        world.Terrain.transform.SetParent(transform);
        world.Time.transform.SetParent(transform);

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
        world.Config = simulationConfig;

        NPCSpawner npcSpawner = new NPCSpawner();
        npcSpawner.Spawn(simulationConfig.npcCount, transform);

        Light sun = GetComponentInChildren<Light>();
        DayLight dayLight = new GameObject("DayLight").AddComponent<DayLight>();
        dayLight.BindSun(sun);
        dayLight.transform.SetParent(transform);

        new GameObject("GameFlow").AddComponent<GameFlow>();   // 재시작을 담당하므로 씬 루트 밖에 둔다
    }
}
