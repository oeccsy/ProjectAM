using System.Collections.Generic;
using UnityEngine;
public static class EarClipping
{
    public static bool TryEarClipping(IReadOnlyList<Vector2> polygon, List<int> remaining, List<int> triangles)
    {
        for (int i = 0; i < remaining.Count; i++)
        {
            int prevIndex = remaining[(i - 1 + remaining.Count) % remaining.Count];
            int curIndex = remaining[i];
            int nextIndex = remaining[(i + 1) % remaining.Count];

            if (!IsClockwiseConvex(polygon[prevIndex], polygon[curIndex], polygon[nextIndex])) continue;
            if (ContainsAnyPoint(polygon, remaining, prevIndex, curIndex, nextIndex)) continue;

            triangles.Add(prevIndex);
            triangles.Add(curIndex);
            triangles.Add(nextIndex);

            remaining.RemoveAt(i);

            return true;
        }

        return false;
    }

    private static bool IsClockwiseConvex(Vector2 previous, Vector2 current, Vector2 next)
    {
        return Cross(current - previous, next - current) < -0.0001f;
    }

    private static bool ContainsAnyPoint(IReadOnlyList<Vector2> polygon, IReadOnlyList<int> remaining, int previousIndex, int currentIndex, int nextIndex)
    {
        Vector2 previous = polygon[previousIndex];
        Vector2 current = polygon[currentIndex];
        Vector2 next = polygon[nextIndex];

        foreach (int pointIndex in remaining)
        {
            if (pointIndex == previousIndex || pointIndex == currentIndex || pointIndex == nextIndex) continue;
            if (IsPointInTriangle(polygon[pointIndex], previous, current, next)) return true;
        }

        return false;
    }

    private static bool IsPointInTriangle(Vector2 point, Vector2 first, Vector2 second, Vector2 third)
    {
        float firstSide = Cross(second - first, point - first);
        float secondSide = Cross(third - second, point - second);
        float thirdSide = Cross(first - third, point - third);
        bool hasPositive = firstSide > 0f || secondSide > 0f || thirdSide > 0f;
        bool hasNegative = firstSide < 0f || secondSide < 0f || thirdSide < 0f;
        return !(hasPositive && hasNegative);
    }

    private static float Cross(Vector2 left, Vector2 right)
    {
        return left.x * right.y - left.y * right.x;
    }
}