using System;
using System.Collections.Generic;
using UnityEngine;

public struct ClosestPair
{
    public int indexA;
    public int indexB;
    public float distance;
    
    // O(n (log n)²) divide-and-conquer
    public static ClosestPair Solve(IList<Vector2Int> points)
    {
        int count = points.Count;
        if (count < 2) return new ClosestPair { indexA = -1, indexB = -1, distance = float.MaxValue };

        int[] sortedIndices = new int[count];
        for (int i = 0; i < count; i++)
        {
            sortedIndices[i] = i;
        }

        Array.Sort(sortedIndices, (a, b) => points[a].x.CompareTo(points[b].x));

        return FindClosestPair(points, sortedIndices, 0, count - 1);
    }

    private static ClosestPair FindClosestPair(IList<Vector2Int> points, int[] sortedIndices, int left, int right)
    {
        if (right - left < 3) return BruteForce(points, sortedIndices, left, right);

        int mid = (left + right) / 2;

        ClosestPair cpl = FindClosestPair(points, sortedIndices, left, mid);
        ClosestPair cpr = FindClosestPair(points, sortedIndices, mid + 1, right);
        float delta = (cpl.distance <= cpr.distance) ? cpl.distance : cpr.distance;

        List<int> stripIndices = new List<int>();
        for (int i = left; i <= right; i++)
        {
            if (Mathf.Abs(points[sortedIndices[i]].x - points[sortedIndices[mid]].x) < delta)
            {
                stripIndices.Add(sortedIndices[i]);
            }
        }

        stripIndices.Sort((a, b) => points[a].y.CompareTo(points[b].y));

        ClosestPair cpc = FindClosestPairCenter(points, stripIndices, delta);
        ClosestPair closestPair = (cpl.distance <= cpr.distance) ? (cpl.distance <= cpc.distance ? cpl : cpc) : (cpr.distance <= cpc.distance ? cpr : cpc);

        return closestPair;
    }

    private static ClosestPair FindClosestPairCenter(IList<Vector2Int> points, List<int> stripIndices, float delta)
    {
        ClosestPair closestPair = new ClosestPair { indexA = -1, indexB = -1, distance = float.MaxValue };

        for (int i = 0; i < stripIndices.Count; i++)
        {
            for (int j = i + 1; j < stripIndices.Count; j++)
            {
                Vector2Int pointA = points[stripIndices[i]];
                Vector2Int pointB = points[stripIndices[j]];

                if (pointB.y - pointA.y >= delta) break;

                float tempDist = Vector2Int.Distance(pointA, pointB);
                if (tempDist < closestPair.distance)
                {
                    closestPair = new ClosestPair
                    { 
                        indexA = stripIndices[i],
                        indexB = stripIndices[j],
                        distance = tempDist
                    };

                    delta = tempDist;
                }
            }
        }

        return closestPair;
    }

    private static ClosestPair BruteForce(IList<Vector2Int> points, int[] sortedIndices, int left, int right)
    {
        ClosestPair closestPair = new ClosestPair { indexA = -1, indexB = -1, distance = float.MaxValue };

        for (int i = left; i <= right; i++)
        {
            for (int j = i + 1; j <= right; j++)
            {
                Vector2Int pointA = points[sortedIndices[i]];
                Vector2Int pointB = points[sortedIndices[j]];

                float tempDist = Vector2Int.Distance(pointA, pointB);
                if (tempDist < closestPair.distance)
                {
                    closestPair = new ClosestPair
                    {
                        indexA = sortedIndices[i],
                        indexB = sortedIndices[j],
                        distance = tempDist
                    };
                }
            }
        }

        return closestPair;
    }
}
