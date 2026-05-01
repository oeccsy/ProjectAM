using UnityEngine;

public class SimulationScene : MonoBehaviour
{
    private void Awake()
    {
        NoiseSettings noiseSettings = new NoiseSettings
        {
            resolution = new Vector2Int(512, 512),
            gridSize = new Vector2Int(4, 4),
            seed = Random.Range(0f, 10000f)
        };

        PerlinNoise perlinNoise = new PerlinNoise();
        perlinNoise.GenerateNoise(noiseSettings);

        MapDataGenerator mapDataGenerator = new MapDataGenerator();
        MapData mapData = mapDataGenerator.GenerateMapData(perlinNoise);
        mapDataGenerator.PrintMapData();

        DebugUtils.ShowTexture(perlinNoise.NoiseTexture);
    }
}
