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
        GenerateHouses(mapData);
    }

    private void GenerateTerrain(MapData mapData)
    {
        GameObject terrainObject = new GameObject("Marching Squares Terrain");
        MarchingSquaresTerrain terrainE = terrainObject.AddComponent<MarchingSquaresTerrain>();
        terrainE.Build(mapData);

        MapGridDebugRenderer gridDebug = terrainObject.AddComponent<MapGridDebugRenderer>();
        gridDebug.BindMapData(mapData);
    }

    private void GenerateHouses(MapData mapData)
    {
        if (mapData.houses == null || mapData.houses.Count == 0) return;

        GameObject container = new GameObject("Houses");

        foreach (House house in mapData.houses)
        {
            GameObject prefab = Resources.Load<GameObject>("Structure/" + house.assetType);
            if (prefab == null)
            {
                Debug.LogWarning($"Structure asset not found: Structure/{house.assetType}");
                continue;
            }

            float worldX = house.origin.x * tileSize;
            float worldZ = house.origin.y * tileSize;
            Vector3 worldPos = new Vector3(worldX, topHeight, -worldZ);

            Quaternion rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 4), 0f);
            GameObject instance = Object.Instantiate(prefab, worldPos, rotation, container.transform);
            instance.name = house.assetType;
        }
    }
}
