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

    public MapData MapData { get; private set; }

    public MapData GenerateMapData(PerlinNoise perlinNoise)
    {
        GenerateFieldData(perlinNoise);
        GenerateOutlineData();

        MapData = BuildMapData();
        return MapData;
    }

    public MapData GenerateMapData(CellularAutomata cellularAutomata)
    {
        MapData = BuildMapData();
        return MapData;
    }

    public MapData GenerateMapData(PerlinNoise perlinNoise, CellularAutomata cellularAutomata)
    {
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
                char value = MapData.fieldTypes[row, col];
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
                float perlin = perlinNoise.NoiseValue[row, col];
                nodes[row, col] = new MapNode
                {
                    row = row,
                    col = col,
                    perlin = perlin,
                    areaID = Undefined,
                    type = (perlin >= 1) ? 'A' : ' '
                };

                if(row == 0 || col == 0 || row == height - 1 || col == width - 1)
                {
                    nodes[row, col].perlin = 0.0f;
                    nodes[row, col].type = ' ';
                }
            }
        }

        // BFS : 영역 구분
        List<List<MapNode>> boundaryNodesByArea = new List<List<MapNode>>();
        boundaryNodesByArea.Add(new List<MapNode>());

        int[] dr = new int[4] { 0, 0, 1, -1 };
        int[] dc = new int[4] { 1, -1, 0, 0 };

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (nodes[row, col].areaID != Undefined) continue;
                if (nodes[row, col].type != 'A') continue;

                areaCount++;
                boundaryNodesByArea.Add(new List<MapNode>());

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
                    
                    if (isEdge) boundaryNodesByArea[areaCount].Add(curNode);
                }
            }
        }

        // areaCount 확정 후 동적 배열 할당
        areaAdj = new AreaEdge[areaCount + 1, areaCount + 1];
        for (int i = 0; i <= areaCount; i++)
        {
            for (int j = 0; j <= areaCount; j++)
            {
                areaAdj[i, j] = new AreaEdge { weight = MaxWeight };
            }
        }

        // BFS : 경계 노드에서 다른 영역까지 최단거리 탐색
        bool[,] isVisited = new bool[height, width];

        foreach (List<MapNode> boundaryNodes in boundaryNodesByArea)
        {
            if (boundaryNodes.Count == 0) continue;

            Array.Clear(isVisited, 0, isVisited.Length);
            Queue<(MapNode startNode, MapNode node, int dist)> bfsQueue = new Queue<(MapNode, MapNode, int)>();

            foreach(MapNode startNode in boundaryNodes)
            {
                isVisited[startNode.row, startNode.col] = true;
                bfsQueue.Enqueue((startNode, startNode, 0));
            }

            while (bfsQueue.Count > 0)
            {
                (MapNode startNode, MapNode curNode, int curNodeDist) = bfsQueue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nextRow = curNode.row + dr[i];
                    int nextCol = curNode.col + dc[i];

                    if (nextRow < 0 || nextCol < 0 || nextRow >= height || nextCol >= width) continue;
                    if (isVisited[nextRow, nextCol]) continue;
                    if (nodes[nextRow, nextCol].areaID == startNode.areaID) continue;

                    isVisited[nextRow, nextCol] = true;

                    MapNode nextNode = nodes[nextRow, nextCol];
                    int nextNodeDist = curNodeDist + 1;

                    if (nextNode.areaID > 0 && nextNodeDist < areaAdj[startNode.areaID, nextNode.areaID].weight)
                    {
                        AreaEdge uv = areaAdj[startNode.areaID, nextNode.areaID];
                        uv.src = startNode;
                        uv.dest = nextNode;
                        uv.weight = nextNodeDist;

                        AreaEdge vu = areaAdj[nextNode.areaID, startNode.areaID];
                        vu.src = nextNode;
                        vu.dest = startNode;
                        vu.weight = nextNodeDist;
                    }
                    else
                    {
                        bfsQueue.Enqueue((startNode, nextNode, nextNodeDist));
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

            MapNode path = prev[dest.row, dest.col];
            while (path != src)
            {
                if (nodes[path.row, path.col].type == 'A')
                {
                    Debug.Log("Error: Path should only go through empty nodes. Check BFS path tracking logic.");
                }

                nodes[path.row, path.col].type = 'R';
                nodes[path.row, path.col].perlin = 1.0f;
                path = prev[path.row, path.col];
            }

            Union(srcAreaID, destAreaID, parentAreaID);
            unionCount++;
            
            if (unionCount == areaCount - 1) break;
        }
    }

    private void GenerateSimpleFieldData(PerlinNoise perlinNoise)
    {
        width = perlinNoise.NoiseSettings.resolution.x;
        height = perlinNoise.NoiseSettings.resolution.y;

        nodes = new MapNode[height, width];
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                float perlin = perlinNoise.NoiseValue[row, col];
                nodes[row, col] = new MapNode
                {
                    row = row,
                    col = col,
                    perlin = perlin,
                    areaID = Undefined,
                    type = (perlin >= 1) ? 'A' : ' '
                };

                if (row == 0 || col == 0 || row == height - 1 || col == width - 1)
                {
                    nodes[row, col].perlin = 0.0f;
                    nodes[row, col].type = ' ';
                }
            }
        }
    }

    private void GenerateOutlineData()
    {
        int[] dr = new int[4] { 1, -1, 0, 0 };
        int[] dc = new int[4] { 0, 0, 1, -1 };

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (nodes[row, col].type != ' ') continue;

                for (int i = 0; i < 4; i++)
                {
                    int adjRow = row + dr[i];
                    int adjCol = col + dc[i];

                    if (adjRow < 0 || adjCol < 0 || adjRow >= height || adjCol >= width) continue;
                    if (nodes[adjRow, adjCol].type == 'O') continue;
                    if (nodes[adjRow, adjCol].type == ' ') continue;

                    nodes[row, col].type = 'O';
                    break;
                }
            }
        }
    }

    private MapData BuildMapData()
    {
        MapData mapData = new MapData
        {
            resolution = new Vector2Int(width, height),
            fieldTypes = new char[height, width],
            values = new float[height, width]
        };

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                mapData.fieldTypes[row, col] = nodes[row, col].type;
                mapData.values[row, col] = nodes[row, col].perlin;
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
}