using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;

public static class PathfindingAlgorithm
{
    // TODO: Implement the pathfinding assignment here.
    // Find the lowest-cost path from start to goal using the map dimensions, wall costs and directed jumps exposed by IMapData.
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData, SearchAlgorithm searchAlgorithm)
    {
        // Most of your solution should be implemented in this method.

        int startVertex = start.y * mapData.Width + start.x;
        int goalVertex = goal.y * mapData.Width + goal.x;

        PathFinder pathfinder;

        switch (searchAlgorithm)
        {
            case SearchAlgorithm.BreadthFirstSearch:
                NormalGraph graph = new NormalGraph(mapData);
                pathfinder = new BreadthFirstPath(graph, startVertex);
                break;
            case SearchAlgorithm.Dijkstra:
                WeightedEdgeDigraph weightedEdgeDigraph = new WeightedEdgeDigraph(mapData);
                pathfinder = new DijkstraShortestPath(weightedEdgeDigraph, startVertex);
                break;
            case SearchAlgorithm.AStar:
                weightedEdgeDigraph = new WeightedEdgeDigraph(mapData);
                pathfinder = new AStarPathFinding(weightedEdgeDigraph, startVertex, goalVertex);
                break;
            default: // Don't think this will ever be used but needed to not cause compiler error
                throw new InvalidEnumArgumentException("Selected type is not valid"); 
        }

        if (pathfinder.HasPathTo(goalVertex))
        {
            List<Vector2Int> path = new List<Vector2Int>();
            foreach (int node in pathfinder.PathTo(goalVertex))
            {
                int x = node % mapData.Width;
                int y = node / mapData.Width;
                path.Add(new Vector2Int(x, y));
            }
            return path;
        }

        return null;
    }

    public static bool IsMovementBlocked(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        if (mapData.TryGetJumpCost(from, to, out _))
            return false;

        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;

        if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) != 1)
            return true;

        if (deltaX != 0)
        {
            int wallX = deltaX > 0 ? to.x : from.x;
            float wallCost = mapData.GetVerticalWallCost(wallX, from.y);
            return float.IsPositiveInfinity(wallCost) || wallCost >= float.MaxValue;
        }

        int wallY = deltaY > 0 ? to.y : from.y;
        float horizontalWallCost = mapData.GetHorizontalWallCost(from.x, wallY);
        return float.IsPositiveInfinity(horizontalWallCost) || horizontalWallCost >= float.MaxValue;
    }
}
