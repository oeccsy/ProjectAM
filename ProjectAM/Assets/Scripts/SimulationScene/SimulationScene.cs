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
            resolution = new Vector2Int(100, 100),
            fillPercentage = 50,
            smoothIterations = 5,
            blurIterations = 3,
            neighborRange = 3
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

        WorldDataStore.Instance.MapData = mapData;
        WorldDataStore.Instance.TerrainScaleSettings = terrainScaleSettings;
        WorldDataStore.Instance.Terrain = mapGenerator.Terrain;
        WorldDataStore.Instance.Houses = mapGenerator.Houses;
        WorldDataStore.Instance.Square = mapGenerator.Square;

        NPCSpawner npcSpawner = new NPCSpawner();
        npcSpawner.Spawn(6);
    }
}
