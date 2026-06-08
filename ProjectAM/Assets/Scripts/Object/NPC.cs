using Unity.Behavior;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private BehaviorGraphAgent behaviorGraph;

    private void Awake()
    {
        behaviorGraph = GetComponent<BehaviorGraphAgent>();
        behaviorGraph.SetVariableValue<NPC>("NPC", this);
    }

    public void Move(Vector2Int destTile)
    {
        
    }
}