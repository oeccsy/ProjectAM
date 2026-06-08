using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner
{
    public void Spawn(int amount)
    {
        MapData mapData = WorldDataStore.Instance.MapData;
        TerrainScaleSettings scaleConfig = WorldDataStore.Instance.TerrainScaleSettings;

        GameObject prefab = Resources.Load<GameObject>("Prefabs/NPC");
        if (prefab == null)
        {
            Debug.LogWarning($"NPC asset not found: Prefabs/NPC");
            return;
        }

        int height = mapData.resolution.y;
        int width = mapData.resolution.x;

        Vector2Int squareOrigin = WorldDataStore.Instance.Square.origin;
        List<Vector2Int> spawnable = new List<Vector2Int>();
        
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (row == squareOrigin.y && col == squareOrigin.x) continue;

                if (mapData.fieldTypes[row, col] == 'S') spawnable.Add(new Vector2Int(col, row));
            }
        }

        Utils.Shuffle(spawnable);

        int spawnCount = Mathf.Min(amount, spawnable.Count);
        GameObject container = new GameObject("NPCs");

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2Int spawnTarget = spawnable[i];

            float worldX = spawnTarget.x * scaleConfig.tileSize;
            float worldZ = spawnTarget.y * scaleConfig.tileSize;
            Vector3 worldPos = new Vector3(worldX, scaleConfig.topHeight, -worldZ);

            GameObject instance = Object.Instantiate(prefab, worldPos, Quaternion.identity, container.transform);
            instance.name = prefab.name;
        }
    }
}
