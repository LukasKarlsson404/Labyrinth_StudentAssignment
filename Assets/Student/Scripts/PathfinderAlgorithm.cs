using System.Collections.Generic;
using UnityEngine;

public static class PathfindingAlgorithm
{
    /* <summary>
     TODO: Implement pathfinding algorithm here.
     Find the lowest-cost path from start to goal position in the maze.

     Dijkstra's Algorithm Steps:
     1. Initialize distances to all nodes as infinity
     2. Set distance to start node as 0
     3. Add start node to priority queue
     4. While priority queue is not empty:
        a. Remove node with minimum distance
        b. If it is the goal, reconstruct path
        c. Check the four adjacent grid neighbours
        d. Check mapData.GetJumpsFrom(current) for directed jump neighbours
        e. Relax each valid edge using its movement/jump cost

     MAZE FEATURES TO HANDLE:
     - Basic movement cost: 1.0 between adjacent cells
     - Walls: a missing cost is normalized to infinity (impassable)
     - Costed walls: may be crossed using the cost supplied in the JSON
     - Jumps: directed edges from one cell to another with their own float cost
     - Dynamic dimensions: always use mapData.Width and mapData.Height

     USEFUL MAP DATA:
     - GetHorizontalWallCost / GetVerticalWallCost
     - GetJumpsFrom(position)
     - Width / Height

     HINT: Start with ordinary four-directional Dijkstra, then add the directed
     jump connections as extra neighbours of the current node.
     </summary> */
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData)
    {
        // TODO: Implement your pathfinding algorithm here.
        Debug.LogWarning("FindShortestPath not implemented yet!");
        return null;
    }

    public static bool IsMovementBlocked(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        // A path may contain a directed jump between non-adjacent cells.
        if (mapData.TryGetJumpCost(from, to, out _))
            return false;

        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;

        if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) != 1)
            return true;

        if (deltaX != 0)
        {
            int wallX = deltaX > 0 ? to.x : from.x;
            return float.IsPositiveInfinity(mapData.GetVerticalWallCost(wallX, from.y)) ||
                   mapData.GetVerticalWallCost(wallX, from.y) >= float.MaxValue;
        }

        int wallY = deltaY > 0 ? to.y : from.y;
        return float.IsPositiveInfinity(mapData.GetHorizontalWallCost(from.x, wallY)) ||
               mapData.GetHorizontalWallCost(from.x, wallY) >= float.MaxValue;
    }
}
