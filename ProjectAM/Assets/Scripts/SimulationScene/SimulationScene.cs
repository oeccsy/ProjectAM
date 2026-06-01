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

        CellularAutomata cellularAutomata = new CellularAutomata();
        CellularAutomataSettings settings = new CellularAutomataSettings
        {
            resolution = new Vector2Int(64, 64),
            fillPercentage = 50,
            smoothIterations = 5,
            blurIterations = 3
        };

        cellularAutomata.GenerateCellularMap(settings);

        MapDataGenerator mapDataGenerator = new MapDataGenerator();
        MapData mapData = mapDataGenerator.GenerateMapData(perlinNoise);
        mapDataGenerator.PrintMapData();

        /*
        GameObject terrainObjectB = new GameObject("EarClipping Terrain");
        EarClippingMapGenerator marchingSquaresMapGenerator = terrainObjectB.AddComponent<EarClippingMapGenerator>();
        marchingSquaresMapGenerator.Build(mapData);
        
        GameObject terrainObjectD = new GameObject("Perlin Noise Terrain");
        MarchingSquaresTerrain terrainD = terrainObjectD.AddComponent<MarchingSquaresTerrain>();
        terrainD.Build(mapData.value);
        */

        GameObject terrainObjectE = new GameObject("Cellular Automata Terrain");
        MarchingSquaresTerrain terrainE = terrainObjectE.AddComponent<MarchingSquaresTerrain>();
        terrainE.Build(cellularAutomata.CellularMap);
    }
}
