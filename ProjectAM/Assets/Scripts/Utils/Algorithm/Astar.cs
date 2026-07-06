using System;
using System.Collections.Generic;
using UnityEngine;

public class Astar
{
    public enum HeuristicType
    {
        Manhattan,
        Euclidean
    }

    private HeuristicType heuristicType = HeuristicType.Manhattan;

    private int height;
    private int width;

    private AstarNode[,] nodes;
    private Func<Vector2Int, bool> isMovable;
    private bool[,] isVisit;

    private readonly PriorityQueue<AstarNode> openNodes = new PriorityQueue<AstarNode>(Comparer<AstarNode>.Create((a, b) => a.fCost.CompareTo(b.fCost)));
    private readonly Queue<AstarNode> closedNodes = new Queue<AstarNode>();
    private readonly List<Vector2Int> path = new List<Vector2Int>();

    private AstarNode start;
    private AstarNode goal;

    private readonly int[] dr = { 0, 0, 1, -1 };
    private readonly int[] dc = { 1, -1, 0, 0 };

    public Astar(Func<Vector2Int, bool> isMovable, HeuristicType heuristicType)
    {
        this.isMovable = isMovable;
        this.heuristicType = heuristicType;

        height = 512;
        width = 512;

        nodes = new AstarNode[height, width];
        isVisit = new bool[height, width];
        
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                nodes[row, col] = new AstarNode { row = row, col = col };
            }
        }
    }

    public List<Vector2Int> Path => path;

    public void FindPath(Vector2Int startPos, Vector2Int goalPos)
    {
        Init();

        start = nodes[startPos.y, startPos.x];
        goal = nodes[goalPos.y, goalPos.x];

        start.gCost = 0.0f;
        openNodes.Push(start);

        while (openNodes.Count > 0)
        {
            AstarNode curNode = openNodes.Pop();

            if (isVisit[curNode.row, curNode.col]) continue;

            isVisit[curNode.row, curNode.col] = true;
            closedNodes.Enqueue(curNode);

            if (curNode == goal)
            {
                ConstructPath(curNode);
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                int nextRow = curNode.row + dr[i];
                int nextCol = curNode.col + dc[i];

                if (nextRow < 0 || nextRow >= height || nextCol < 0 || nextCol >= width) continue;
                if (!isMovable(new Vector2Int(nextCol, nextRow))) continue;

                AstarNode adjNode = nodes[nextRow, nextCol];

                if (isVisit[nextRow, nextCol] || (adjNode.gCost <= curNode.gCost + 1)) continue;

                adjNode.parent = curNode;
                adjNode.gCost = curNode.gCost + 1;
                adjNode.hCost = CalcHeuristicCost(heuristicType, adjNode, goal);
                adjNode.fCost = adjNode.gCost + adjNode.hCost;

                openNodes.Push(adjNode);
            }
        }
    }

    private void Init()
    {
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                nodes[row, col].gCost = float.MaxValue;
                nodes[row, col].hCost = 0.0f;
                nodes[row, col].fCost = 0.0f;
                nodes[row, col].parent = null;
                
                isVisit[row, col] = false;
            }
        }

        openNodes.Clear();
        closedNodes.Clear();
        path.Clear();
    }

    private float CalcHeuristicCost(HeuristicType heuristicType, AstarNode curNode, AstarNode goalNode)
    {
        switch(heuristicType)
        {
            case HeuristicType.Manhattan:
                return CalcManhattanHeuristic(curNode, goalNode);
            case HeuristicType.Euclidean:
                return CalcEuclideanHeuristic(curNode, goalNode);
            default:
                return 0.0f;
        }
    }

    private float CalcManhattanHeuristic(AstarNode curNode, AstarNode goalNode)
    {
        int dx = Mathf.Abs(goalNode.col - curNode.col);
        int dy = Mathf.Abs(goalNode.row - curNode.row);

        return dx + dy;
    }

    private float CalcEuclideanHeuristic(AstarNode curNode, AstarNode goalNode)
    {
        Vector2 diff = new Vector2(goalNode.col - curNode.col, goalNode.row - curNode.row);

        return Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
    }

    private void ConstructPath(AstarNode goal)
    {
        Stack<AstarNode> pathStack = new Stack<AstarNode>();
        AstarNode curNode = goal;

        while (curNode != null)
        {
            pathStack.Push(curNode);
            curNode = curNode.parent;
        }

        while (pathStack.Count > 0)
        {
            AstarNode pathNode = pathStack.Pop();
            path.Add(new Vector2Int(pathNode.col, pathNode.row));
        }
    }
}
