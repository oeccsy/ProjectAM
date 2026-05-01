using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MapDataGenerator
{
    private const int Undefined = -100;
    private const int MaxWeight = 10000;

    private MapNode[,] nodes;
    private AreaEdge[,] areaAdj;

    private int width;
    private int height;
    private int areaCount;
    private int forestCount;

    public MapData MapData { get; private set; }

    public MapData GenerateMapData(PerlinNoise perlinNoise)
    {
        GenerateFieldData(perlinNoise);

        MapData = BuildMapData();
        return MapData;
    }

    public void PrintMapData()
    {
        if (width <= 0 || height <= 0)
        {
            Debug.LogError($"Invalid resolution: {(width, height)}. Cannot print values.");
            return;
        }

        StringBuilder stringBuilder = new StringBuilder();

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                char value = MapData.map[row, col];
                stringBuilder.Append(value);
            }

            stringBuilder.Append('\n');
        }

        Debug.Log(stringBuilder.ToString());
    }

    private void GenerateFieldData(PerlinNoise perlinNoise)
    {
        width = perlinNoise.NoiseSettings.resolution.x;
        height = perlinNoise.NoiseSettings.resolution.y;
        areaCount = 0;

        // 노드 초기화
        nodes = new MapNode[height, width];
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int perlin = perlinNoise.NoiseValue[row, col];
                nodes[row, col] = new MapNode
                {
                    row = row,
                    col = col,
                    perlin = perlin,
                    areaID = Undefined,
                    type = (perlin >= 1) ? 'A' : ' '
                };
            }
        }

        // BFS : 영역 구분
        List<MapNode> areaBorderNodes = new List<MapNode>();
        int[] dr = new int[4] { 0, 0, 1, -1 };
        int[] dc = new int[4] { 1, -1, 0, 0 };

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (nodes[row, col].areaID != Undefined) continue;
                if (nodes[row, col].type != 'A') continue;

                areaCount++;

                Queue<MapNode> bfsQueue = new Queue<MapNode>();
                MapNode start = nodes[row, col];
                start.areaID = areaCount;
                bfsQueue.Enqueue(start);

                while (bfsQueue.Count > 0)
                {
                    MapNode curNode = bfsQueue.Dequeue();
                    bool isEdge = false;

                    for (int k = 0; k < 4; k++)
                    {
                        int nextRow = curNode.row + dr[k];
                        int nextCol = curNode.col + dc[k];

                        if (nextRow < 0 || nextCol < 0 || nextRow >= height || nextCol >= width) continue;
                        if (nodes[nextRow, nextCol].areaID == Undefined) isEdge = true;
                        if (nodes[nextRow, nextCol].areaID != Undefined) continue;
                        if (nodes[nextRow, nextCol].type != 'A') continue;

                        nodes[nextRow, nextCol].areaID = curNode.areaID;

                        bfsQueue.Enqueue(nodes[nextRow, nextCol]);
                    }

                    if (isEdge) areaBorderNodes.Add(curNode);
                }
            }
        }

        // areaCount 확정 후 동적 배열 할당
        areaAdj = new AreaEdge[areaCount + 1, areaCount + 1];
        for (int i = 0; i <= areaCount; i++)
            for (int j = 0; j <= areaCount; j++)
                areaAdj[i, j] = new AreaEdge { weight = MaxWeight };

        // BFS : 경계 노드에서 다른 영역까지 최단거리 탐색
        bool[,] isVisit = new bool[height, width];

        foreach (MapNode borderNode in areaBorderNodes)
        {
            Array.Clear(isVisit, 0, isVisit.Length);
            Queue<(MapNode node, int dist)> bfsQueue = new Queue<(MapNode, int)>();

            MapNode start = borderNode;
            isVisit[start.row, start.col] = true;
            bfsQueue.Enqueue((start, 0));

            while (bfsQueue.Count > 0)
            {
                (MapNode curNode, int curNodeDist) = bfsQueue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nextRow = curNode.row + dr[i];
                    int nextCol = curNode.col + dc[i];

                    if (nextRow < 0 || nextCol < 0 || nextRow >= height || nextCol >= width) continue;
                    if (isVisit[nextRow, nextCol]) continue;
                    if (nodes[nextRow, nextCol].areaID == start.areaID) continue;

                    isVisit[nextRow, nextCol] = true;

                    MapNode nextNode = nodes[nextRow, nextCol];
                    int nextNodeDist = curNodeDist + 1;

                    if (nextNode.areaID > 0 && nextNodeDist < areaAdj[start.areaID, nextNode.areaID].weight)
                    {
                        AreaEdge uv = areaAdj[start.areaID, nextNode.areaID];
                        uv.src = start;
                        uv.dest = nextNode;
                        uv.weight = nextNodeDist;

                        AreaEdge vu = areaAdj[nextNode.areaID, start.areaID];
                        vu.src = nextNode;
                        vu.dest = start;
                        vu.weight = nextNodeDist;
                    }
                    else
                    {
                        bfsQueue.Enqueue((nextNode, nextNodeDist));
                    }
                }
            }
        }

        // Kruskal MST : 영역 연결
        MapNode[,] prev = new MapNode[height, width];
        List<AreaEdge> edges = new List<AreaEdge>();

        for (int i = 1; i <= areaCount; i++)
        {
            for (int j = i + 1; j <= areaCount; j++)
            {
                AreaEdge edge = areaAdj[i, j];

                if (edge.weight == 0 || edge.weight == MaxWeight) continue;
                
                edges.Add(edge);
            }
        }

        edges.Sort((a, b) => a.weight.CompareTo(b.weight));

        int[] parentAreaID = new int[areaCount + 1];
        int unionCount = 0;
        
        for (int i = 0; i <= areaCount; i++)
        {
            parentAreaID[i] = i;
        }

        foreach (AreaEdge edge in edges)
        {
            int srcAreaID = edge.src.areaID;
            int destAreaID = edge.dest.areaID;

            if (Find(srcAreaID, parentAreaID) == Find(destAreaID, parentAreaID)) continue;

            // BFS : 경로 추적
            Array.Clear(prev, 0, prev.Length);
            Queue<MapNode> bfsQueue = new Queue<MapNode>();

            MapNode src = edge.src;
            prev[src.row, src.col] = src;
            bfsQueue.Enqueue(src);

            MapNode dest = null;

            while (bfsQueue.Count > 0 && dest == null)
            {
                MapNode curNode = bfsQueue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nextRow = curNode.row + dr[i];
                    int nextCol = curNode.col + dc[i];

                    if (nextRow < 0 || nextCol < 0 || nextRow >= height || nextCol >= width) continue;
                    if (prev[nextRow, nextCol] != null) continue;
                    if (nodes[nextRow, nextCol].areaID == edge.src.areaID) continue;

                    prev[nextRow, nextCol] = curNode;

                    MapNode nextNode = nodes[nextRow, nextCol];
                    bfsQueue.Enqueue(nextNode);

                    if (nextNode.areaID == destAreaID)
                    { 
                        dest = nextNode;
                        break;
                    }
                }
            }

            if (dest == null) continue;

            MapNode path = dest;
            while (path != src)
            {
                nodes[path.row, path.col].type = 'R';
                path = prev[path.row, path.col];
            }

            Union(srcAreaID, destAreaID, parentAreaID);
            unionCount++;
            
            if (unionCount == areaCount - 1) break;
        }
    }
    private void GenerateHouseData(int houseCount)
    {
        List<List<MapNode>> candidatesByArea = new List<List<MapNode>>();
        for (int i = 0; i <= areaCount; i++) candidatesByArea.Add(new List<MapNode>());

        List<int> candidateAreas = new List<int>();

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (nodes[row, col].perlin > 1 && nodes[row, col].areaID > 0)
                    candidatesByArea[nodes[row, col].areaID].Add(nodes[row, col]);
            }
        }

        for (int i = 1; i <= areaCount; i++)
        {
            Utils.Shuffle(candidatesByArea[i]);
            candidateAreas.Add(i);
        }

        Utils.Shuffle(candidateAreas);
        int areaIndex = 0;

        for (int i = 0; i < houseCount; i++)
        {
            bool isValid;
            do
            {
                isValid = true;

                int targetArea = candidateAreas[areaIndex];
                while (candidatesByArea[targetArea].Count == 0)
                {
                    areaIndex = (areaIndex + 1) % candidateAreas.Count;
                    targetArea = candidateAreas[areaIndex];
                }

                MapNode target = candidatesByArea[targetArea][^1];
                candidatesByArea[targetArea].RemoveAt(candidatesByArea[targetArea].Count - 1);

                for (int j = 0; j < 4 && isValid; j++)
                {
                    for (int k = 0; k < 4 && isValid; k++)
                    {
                        int r = target.row + j;
                        int c = target.col + k;
                        if (r >= height || c >= width) { isValid = false; break; }

                        MapNode adj = nodes[r, c];
                        if (adj.perlin <= 1 || adj.areaID <= 0) isValid = false;
                        if (adj.type == 'H' || adj.type == 'h') isValid = false;
                    }
                }

                if (isValid)
                {
                    for (int j = 0; j < 4; j++)
                        for (int k = 0; k < 4; k++)
                            nodes[target.row + j, target.col + k].type = 'h';

                    nodes[target.row, target.col].type = 'H';
                    areaIndex = (areaIndex + 1) % candidateAreas.Count;
                }
            }
            while (!isValid);
        }
    }

    private void GenerateForestData()
    {
        int[] dr = new int[4] { 0, 0, 1, -1 };
        int[] dc = new int[4] { 1, -1, 0, 0 };

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (nodes[row, col].type != ' ') continue;
                if (!IsForestBorder(nodes[row, col])) continue;

                forestCount++;

                bool[,] isVisit = new bool[height, width];
                Stack<MapNode> dfsStack = new Stack<MapNode>();

                MapNode start = nodes[row, col];
                start.areaID = -forestCount;
                start.type = 'F';
                isVisit[start.row, start.col] = true;
                dfsStack.Push(start);

                while (dfsStack.Count > 0)
                {
                    MapNode cur = dfsStack.Pop();

                    for (int k = 0; k < 4; k++)
                    {
                        int nextRow = cur.row + dr[k];
                        int nextCol = cur.col + dc[k];

                        if (nextRow < 0 || nextCol < 0 || nextRow >= height || nextCol >= width) continue;
                        if (nodes[nextRow, nextCol].areaID >= 0) continue;
                        if (isVisit[nextRow, nextCol]) continue;

                        isVisit[nextRow, nextCol] = true;
                        MapNode next = nodes[nextRow, nextCol];

                        if (IsForestBorder(next))
                        {
                            next.areaID = cur.areaID;
                            next.type = cur.type;
                            dfsStack.Push(next);
                        }
                    }
                }
            }
        }

    }
    private MapData BuildMapData()
    {
        MapData mapData = new MapData
        {
            resolution = new Vector2Int(width, height),
            map = new char[height, width],
        };

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                mapData.map[row, col] = nodes[row, col].type;
            }
        }

        return mapData;
    }

    private void Union(int id1, int id2, int[] parents)
    {
        int root1 = Find(id1, parents);
        int root2 = Find(id2, parents);

        parents[root2] = root1;
    }

    private int Find(int id, int[] parents)
    {
        if (parents[id] != id) parents[id] = Find(parents[id], parents);

        return parents[id];
    }

    private bool IsForestBorder(MapNode node)
    {
        if (node.areaID >= 0) return false;
        if (node.type == 'A' || node.type == 'R') return false;

        bool isBorder = false;

        int[] dr = new int[8] { 0, 0, 1, 1, 1, -1, -1, -1 };
        int[] dc = new int[8] { 1, -1, 0, 1, -1, 0, 1, -1 };

        for (int k = 0; k < 8; k++)
        {
            int nextRow = node.row + dr[k];
            int nextCol = node.col + dc[k];

            if (nextRow < 0 || nextCol < 0 || nextRow >= height || nextCol >= width)
            {
                isBorder = true;
                continue;
            }

            if (nodes[nextRow, nextCol].areaID >= 0) isBorder = true;
            if (nodes[nextRow, nextCol].type == 'A' || nodes[nextRow, nextCol].type == 'R') isBorder = true;
        }

        return isBorder;
    }
}