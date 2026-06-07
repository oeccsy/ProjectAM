using UnityEngine;

public class MapGenerator
{
    [SerializeField]
    private float tileSize = 1.0f;
    [SerializeField]
    private float topHeight = 1.0f;
    [SerializeField]
    private float bottomHeight = 0.0f;

    public void GenerateMap(MapData mapData)
    {
        GenerateTerrain(mapData);
        GenerateBridges(mapData);
        GenerateHouses(mapData);
        GenerateSquare(mapData);
    }

    private void GenerateTerrain(MapData mapData)
    {
        GameObject terrainObject = new GameObject("Marching Squares Terrain");
        MarchingSquaresTerrain terrainE = terrainObject.AddComponent<MarchingSquaresTerrain>();
        terrainE.Build(mapData);

        MapGridDebugRenderer gridDebug = terrainObject.AddComponent<MapGridDebugRenderer>();
        gridDebug.BindMapData(mapData);
    }

    private void GenerateBridges(MapData mapData)
    {
        if (mapData.houses == null) return;

        GameObject container = new GameObject("Bridges");

        for(int row = 0; row < mapData.resolution.y; row++)
        {
            for(int col = 0; col < mapData.resolution.x; col++)
            {
                if(mapData.fieldTypes[row, col] != 'B') continue;

                GameObject prefab = Resources.Load<GameObject>("Prefabs/Bridge");
                if (prefab == null)
                {
                    Debug.LogWarning($"Structure asset not found: Prefabs/Bridge");
                    continue;
                }

                float worldX = col * tileSize;
                float worldZ = row * tileSize;
                Vector3 worldPos = new Vector3(worldX, topHeight, -worldZ);
                Object.Instantiate(prefab, worldPos, Quaternion.identity, container.transform);
            }            
        }
    }

    private void GenerateHouses(MapData mapData)
    {
        if (mapData.houses == null || mapData.houses.Count == 0) return;

        GameObject container = new GameObject("Houses");

        foreach (House house in mapData.houses)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/House");
            if (prefab == null)
            {
                Debug.LogWarning($"Structure asset not found: Prefabs/House");
                continue;
            }

            float worldX = house.origin.x * tileSize;
            float worldZ = house.origin.y * tileSize;
            Vector3 worldPos = new Vector3(worldX, topHeight, -worldZ);

            Quaternion rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 4), 0f);
            Object.Instantiate(prefab, worldPos, rotation, container.transform);
        }
    }

    private void GenerateSquare(MapData mapData)
    {
        if (mapData.houses == null) return;

        Square square = mapData.square;
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Square");
        if (prefab == null)
        {
            Debug.LogWarning($"Structure asset not found: Prefabs/Square");
            return;
        }

        float worldX = square.origin.x * tileSize;
        float worldZ = square.origin.y * tileSize;
        Vector3 worldPos = new Vector3(worldX, topHeight, -worldZ);

        Object.Instantiate(prefab, worldPos, Quaternion.identity);
    }
}
