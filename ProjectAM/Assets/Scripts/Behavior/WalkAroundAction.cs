using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WalkAround", story: "[Agent] Move To Random Tile", category: "Action", id: "a9c968d44a5cc48d7a3324bb740fa600")]
public partial class WalkAroundAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    private NPC npc;
    private Astar astar;
    private List<Vector2Int> candidates = new List<Vector2Int>();
    private List<Vector2Int> path;
    

#if UNITY_EDITOR
    private NpcPathDebugRenderer pathDebug;
#endif

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        MapData mapData = World.Instance.MapData;
        if (mapData == null) return Status.Failure;

        Vector2Int destTile = SelectDestination(npc.CurrentTile, mapData);
        if (destTile == npc.CurrentTile) return Status.Failure;

        if (astar == null) astar = new Astar(mapData.fieldTypes, NPC.MovableTypes, Astar.HeuristicType.Manhattan);
        astar.FindPath(npc.CurrentTile, destTile);
        path = astar.Path;

        if (path.Count <= 1) return Status.Failure;
        npc.MoveAlong(path);

#if UNITY_EDITOR
        if (pathDebug == null) pathDebug = npc.GetComponent<NpcPathDebugRenderer>();
        if (pathDebug != null) pathDebug.RegisterPath(path);
#endif

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (npc == null) return Status.Failure;

        return (npc.State == NpcState.Walking) ? Status.Running : Status.Success;
    }

    protected override void OnEnd()
    {
        NPC npc = Agent?.Value;
        if (npc != null) npc.StopMove();

#if UNITY_EDITOR
        if (pathDebug != null) pathDebug.ClearPath();
#endif
    }

    private Vector2Int SelectDestination(Vector2Int origin, MapData mapData)
    {
        candidates.Clear();
        int width = mapData.resolution.x;
        int height = mapData.resolution.y;

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if(row == origin.y && col == origin.x) continue;
                if (!NPC.MovableTypes.Contains(mapData.fieldTypes[row, col])) continue;

                candidates.Add(new Vector2Int(col, row));
            }
        }

        if (candidates.Count == 0)
        {
            return origin;
        }
        else
        {
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
    }
}
