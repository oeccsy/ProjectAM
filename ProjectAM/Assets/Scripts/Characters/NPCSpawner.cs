using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 객체를 생성하는 역할의 클래스
/// </summary>
public class NPCSpawner
{
    public void Spawn(int amount)
    {
        List<GameObject> prefabs = SelectRandomNpcPrefab(amount);
        List<NpcColor> npcColors = ColorUtils.GetNpcColorList();

        GameObject container = new GameObject("NPCs");

        for (int i = 0; i < amount; i++)
        {
            House house = World.Instance.Houses.Get(npcColors[i]);
            if (house == null) continue;

            List<Vector2Int> spawnable = house.GetApproachTiles();
            Vector2Int spawnTile = spawnable[Random.Range(0, spawnable.Count)];
            Vector3 spawnPos = TileCoordinate.TileToWorld(spawnTile, World.Instance.TerrainScaleSettings);

            GameObject instance = Object.Instantiate(prefabs[i], spawnPos, Quaternion.identity, container.transform);

            NPC npc = instance.GetComponent<NPC>();
            npc.Init(npcColors[i]);

            World.Instance.NPCs.Register(npcColors[i], npc);
        }
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
