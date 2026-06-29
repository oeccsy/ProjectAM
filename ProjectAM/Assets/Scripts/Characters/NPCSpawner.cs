using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner
{
    public void Spawn(int amount)
    {
        List<Vector3> spawnable = FindSpawnablePosition(amount);
        List<GameObject> prefabs = SelectRandomNpcPrefab(amount);
        List<NpcColor> npcColors = ColorUtils.GetNpcColorList();
        
        int actualSpawnCount = Mathf.Min(amount, spawnable.Count);
        GameObject container = new GameObject("NPCs");

        for (int i = 0; i < actualSpawnCount; i++)
        {
            GameObject instance = Object.Instantiate(prefabs[i], spawnable[i], Quaternion.identity, container.transform);

            NPC npc = instance.GetComponent<NPC>();
            npc.Init(npcColors[i]);
            
            World.Instance.NPCs.Add(npc);
        }
    }

    private List<Vector3> FindSpawnablePosition(int amount)
    {
        MapData mapData = World.Instance.MapData;
        TerrainScaleSettings scaleConfig = World.Instance.TerrainScaleSettings;

        int height = mapData.resolution.y;
        int width = mapData.resolution.x;

        Vector2Int squareOrigin = World.Instance.Square.origin;
        List<Vector3> spawnable = new List<Vector3>();
        
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (row == squareOrigin.y && col == squareOrigin.x) continue;
                if (mapData.fieldTypes[row, col] != 'S') continue;

                float worldX = col * scaleConfig.tileSize;
                float worldZ = row * scaleConfig.tileSize;
                Vector3 worldPos = new Vector3(worldX, scaleConfig.topHeight, -worldZ);

                spawnable.Add(worldPos);
            }
        }
        
        Utils.Shuffle<Vector3>(spawnable);

        return spawnable;
    }

    private List<GameObject> SelectRandomNpcPrefab(int amount)
    {
        string boyPrefabPath = "Prefabs/NPC_Boy";
        string girlPrefabPath = "Prefabs/NPC_Girl";

        GameObject boyPrefab = Resources.Load<GameObject>(boyPrefabPath);
        GameObject girlPrefab = Resources.Load<GameObject>(girlPrefabPath);
        
        if (boyPrefab == null || girlPrefab == null)
        {
            if (boyPrefab == null) Debug.LogWarning($"NPC asset not found : {boyPrefab}");
            if (girlPrefab == null) Debug.LogWarning($"NPC asset not found : {girlPrefab}");
            return null;
        }

        List<GameObject> prefabs = new List<GameObject>();

        for (int i = 0; i < amount; i++)
        {
            GameObject randomPrefab = (Random.value < 0.5f) ? boyPrefab : girlPrefab;
            prefabs.Add(randomPrefab);
        }

        return prefabs;
    }
}
