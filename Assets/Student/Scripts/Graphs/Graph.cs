using UnityEngine;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class Graph
{
    int vertices;
    int edges; // Weighted edges can be found on page 642 in the book
    ConcurrentBag<int>[] adjacencyList;

    public int Vertices => vertices;
    public int Edges => edges;

    public Graph(IMapData mapData)
    {
        vertices = mapData.Width * mapData.Height;
        edges = 0;
        adjacencyList = new ConcurrentBag<int>[vertices];

        for (int i = 0; i < vertices; i++)
        {
            adjacencyList[i] = new ConcurrentBag<int>();
        }
        
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                int currentVertex = y * mapData.Width + x;

                // Add edges to adjacent vertices if movement is not blocked
                Vector2Int currentPosition = new Vector2Int(x, y);
                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int neighborPos = currentPosition + direction;
                    if (neighborPos.x >= 0 && neighborPos.x < mapData.Width && neighborPos.y >= 0 && neighborPos.y < mapData.Height)
                    {
                        if (!PathfindingAlgorithm.IsMovementBlocked(currentPosition, neighborPos, mapData))
                        {
                            int neighborVertex = neighborPos.y * mapData.Width + neighborPos.x;
                            AddEdge(currentVertex, neighborVertex);
                        }
                    }
                }

                // Add directed jump edges here when needed
            }
        }
    }

    public void AddEdge(int v, int w)
    {
        adjacencyList[v].Add(w);
        adjacencyList[w].Add(v);
        edges++;
    }

    public IEnumerable<int> GetNeighbors(int v)
    {
        return adjacencyList[v];
    }
}