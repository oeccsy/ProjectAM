using UnityEngine;

public interface IMovable
{
    Movement Movement { get; }
    Vector2Int CurrentTile { get; }
}
