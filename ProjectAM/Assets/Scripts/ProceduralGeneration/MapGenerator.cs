using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    private TerrainScaleSettings terrainSizeData;

    private GameObject terrain;
    private List<House> houses = new List<House>();
    private Square square;

    public TerrainScaleSettings TerrainSizeData => terrainSizeData; 
    public GameObject Terrain => terrain;
    public List<House> Houses => houses;
    public Square Square => square;

    public void GenerateMap(MapData mapData, TerrainScaleSettings terrainSizeData)
    {
        this.terrainSizeData = terrainSizeData;

        GenerateTerrain(mapData);
        GenerateBridges(mapData);
        GenerateHouses(mapData);
        GenerateSquare(mapData);
    }

    private void GenerateTerrain(MapData mapData)
    {
        terrain = new GameObject("Marching Squares Terrain");
        MarchingSquaresTerrain terrainComp = terrain.AddComponent<MarchingSquaresTerrain>();
        terrainComp.Build(mapData);

        MapGridDebugRenderer gridDebug = terrain.AddComponent<MapGridDebugRenderer>();
        gridDebug.BindMapData(mapData);
    }

    private void GenerateBridges(MapData mapData)
    {
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

                float worldX = col * terrainSizeData.tileSize;
                float worldZ = row * terrainSizeData.tileSize;
                Vector3 worldPos = new Vector3(worldX, terrainSizeData.topHeight, -worldZ);
                Object.Instantiate(prefab, worldPos, Quaternion.identity, container.transform);
            }            
        }
    }

    private void GenerateHouses(MapData mapData)
    {
        if (mapData.houseAnchors.Count == 0) return;

        StructureConfig config = Resources.Load<StructureConfig>("Data/StructureConfig");
        if (config == null)
        {
            Debug.LogWarning("StructureConfig not found at Resources/Data/StructureConfig.");
            return;
        }

        StructureConfig.Entry houseInfo = config.structures[0];
        GameObject container = new GameObject("Houses");

        foreach (Vector2Int houseAnchor in mapData.houseAnchors)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/House");
            if (prefab == null)
            {
                Debug.LogWarning($"Structure asset not found: Prefabs/House");
                continue;
            }

            Vector2Int houseOrigin = houseAnchor + houseInfo.originOffset;
            float worldX = houseOrigin.x * terrainSizeData.tileSize;
            float worldZ = houseOrigin.y * terrainSizeData.tileSize;
            Vector3 worldPos = new Vector3(worldX, terrainSizeData.topHeight, -worldZ);

            Quaternion rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 4), 0f);
            GameObject newObject = Object.Instantiate(prefab, worldPos, rotation, container.transform);
            House newHouse = newObject.GetComponent<House>();
            newHouse.anchor = houseAnchor;
            newHouse.origin = houseOrigin;
            newHouse.size = houseInfo.size;

            houses.Add(newHouse);
        }

        List<NpcColor> npcColors = ColorUtils.GetNpcColorList();
        if (npcColors.Count < houses.Count)
        {
            Debug.LogError($"Need More NpcColor");
            return;
        }

        for (int i = 0; i < houses.Count; i++)
        {
            houses[i].owner = npcColors[i];
            houses[i].ApplyRoofColor(npcColors[i]);
        }
    }

    private void GenerateSquare(MapData mapData)
    {
        StructureConfig config = Resources.Load<StructureConfig>("Data/StructureConfig");
        if (config == null)
        {
            Debug.LogWarning("StructureConfig not found at Resources/Data/StructureConfig.");
            return;
        }

        StructureConfig.Entry squareInfo = config.structures[2];

        GameObject prefab = Resources.Load<GameObject>("Prefabs/Square");
        if (prefab == null)
        {
            Debug.LogWarning($"Structure asset not found: Prefabs/Square");
            return;
        }

        Vector2Int squareOrigin = mapData.squareAnchor + squareInfo.originOffset;
        float worldX = squareOrigin.x * terrainSizeData.tileSize;
        float worldZ = squareOrigin.y * terrainSizeData.tileSize;
        Vector3 worldPos = new Vector3(worldX, terrainSizeData.topHeight, -worldZ);

        GameObject newObject = Object.Instantiate(prefab, worldPos, Quaternion.identity);
        square = newObject.GetComponent<Square>();
        square.origin = squareOrigin;
        square.size = squareInfo.size;
    }
}
