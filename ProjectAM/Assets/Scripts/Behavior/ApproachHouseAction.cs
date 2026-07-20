using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ApproachHouse", story: "[Agent] Approach House", category: "Action", id: "b7d3e9f1a2c4486d8e0f1a2b3c4d5e6f")]
public partial class ApproachHouseAction : Action
{
    [SerializeReference]
    public BlackboardVariable<NPC> Agent;
    [SerializeReference]
    public BlackboardVariable<House> TargetHouse;

    private const float MaxWaitTime = 3.0f;
    private Movement movement;
    private Vector2Int destTile;
    
    private float waitTime = 0.0f;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        movement = npc.Movement;
        if (movement == null) return Status.Failure;

        MapData mapData = World.Instance.MapData;
        if (mapData == null) return Status.Failure;

        House house = TargetHouse?.Value;
        if (house == null) house = World.Instance.Houses.Get(npc.OwnColor);
        if (house == null) return Status.Failure;

        destTile = SelectYardTile(house, mapData);
        if (destTile == npc.CurrentTile) return Status.Failure;

        movement.StartMoveTo(destTile);
        if (movement.State == MoveState.Idle) return Status.Failure;

        waitTime = 0.0f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (movement.State == MoveState.Moving)
        {
            waitTime = 0.0f;
            return Status.Running;
        }
        if (movement.State == MoveState.Waiting)
        {
            waitTime += Time.deltaTime;
            if (waitTime >= MaxWaitTime)
            {
                return Status.Failure;
            }
            else
            {
                return Status.Running;
            }
        }

        return (movement.CurrentTile == destTile) ? Status.Success : Status.Failure;
    }

    protected override void OnEnd() { }

    private Vector2Int SelectYardTile(House house, MapData mapData)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        int width = mapData.resolution.x;
        int height = mapData.resolution.y;

        for (int row = house.anchor.y; row < house.anchor.y + house.size.y; row++)
        {
            for (int col = house.anchor.x; col < house.anchor.x + house.size.x; col++)
            {
                if (row < 0 || col < 0 || row >= height || col >= width) continue;
                if (mapData.fieldTypes[row, col] != 'Y') continue;

                candidates.Add(new Vector2Int(col, row));
            }
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
}
