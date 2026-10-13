using UnityEngine;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class WeightedEdgeDigraph
{
    int vertices;
    int edges;
    ConcurrentBag<DirectedWeightedEdge>[] adjacencyList;
    IMapData mapData;

    public int Vertices => vertices;
    public int Edges => edges;
    public IMapData MapData => mapData;

    public WeightedEdgeDigraph(IMapData mapData)
    {
        this.mapData = mapData;
        vertices = mapData.Width * mapData.Height;
        edges = 0;
        adjacencyList = new ConcurrentBag<DirectedWeightedEdge>[vertices];

        for (int i = 0; i < vertices; i++)
        {
            adjacencyList[i] = new ConcurrentBag<DirectedWeightedEdge>();
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
                    Vector2Int neighborPosition = currentPosition + direction;
                    if (neighborPosition.x >= 0 && neighborPosition.x < mapData.Width && neighborPosition.y >= 0 && neighborPosition.y < mapData.Height)
                    {
                        if (!PathfindingAlgorithm.IsMovementBlocked(currentPosition, neighborPosition, mapData))
                        {
                            int neighborVertex = neighborPosition.y * mapData.Width + neighborPosition.x;

                            // Walls are placed to the left and below the tile, so we need to check the direction to determine which wall cost to use
                            if (currentPosition + Vector2Int.left == neighborPosition) // Neighbor is to the left
                            {
                                double weight = mapData.GetVerticalWallCost(currentPosition.x, currentPosition.y);
                                AddEdge(new DirectedWeightedEdge(currentVertex, neighborVertex, weight));
                            }
                            else if (currentPosition + Vector2Int.right == neighborPosition) // Neighbor is to the right
                            {
                                double weight = mapData.GetVerticalWallCost(neighborPosition.x, neighborPosition.y);
                                AddEdge(new DirectedWeightedEdge(currentVertex, neighborVertex, weight));
                            }
                            else if (currentPosition + Vector2Int.down == neighborPosition) // Neighbor is below
                            {
                                double weight = mapData.GetHorizontalWallCost(currentPosition.x, currentPosition.y);
                                AddEdge(new DirectedWeightedEdge(currentVertex, neighborVertex, weight));
                            }
                            else if (currentPosition + Vector2Int.up == neighborPosition) // Neighbor is above
                            {
                                double weight = mapData.GetHorizontalWallCost(neighborPosition.x, neighborPosition.y);
                                AddEdge(new DirectedWeightedEdge(currentVertex, neighborVertex, weight));
                            }
                        }
                    }
                }

                if (mapData.HasJumpFrom(x, y))
                {
                    foreach (JumpConnection jump in mapData.GetJumpsFrom(currentPosition))
                    {
                        int jumpToVertex = jump.To.y * mapData.Width + jump.To.x;
                        AddEdge(new DirectedWeightedEdge(currentVertex, jumpToVertex, jump.Cost));
                    }
                }
            }
        }
    }

    public void AddEdge(DirectedWeightedEdge edge)
    {
        int v = edge.From();
        adjacencyList[v].Add(edge);
        edges++;
    }

    public IEnumerable<DirectedWeightedEdge> GetNeighbors(int v)
    {
        return adjacencyList[v];
    }
}