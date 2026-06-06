using System.Collections.Generic;
using UnityEngine;

public class PlacementCandidate
{
    public Vector2Int anchor;
    public float nearestDist;
    public PlacementCandidate nearestCandidate;
    public List<PlacementCandidate> incomingCandidates;

    public bool CanPlace(char[,] fieldTypes, Vector2Int tileSize)
    {
        int height = fieldTypes.GetLength(0);
        int width = fieldTypes.GetLength(1);

        for (int row = anchor.y; row < anchor.y + tileSize.y; row++)
        {
            for (int col = anchor.x; col < anchor.x + tileSize.x; col++)
            {
                if (row < 0 || col < 0 || row >= height || col >= width) return false;
                if (fieldTypes[row, col] != 'A') return false;
            }
        }

        return true;
    }

    public PlacementCandidate FindNearestCandidate(List<PlacementCandidate> candidates)
    {
        nearestDist = float.MaxValue;
    
        foreach (PlacementCandidate other in candidates)
        {
            if (other == this) continue;

            float tempDist = Vector2Int.Distance(anchor, other.anchor);
            if (tempDist < nearestDist)
            {
                nearestDist = tempDist;
                nearestCandidate = other;
            }
        }

        return nearestCandidate;
    }
}
