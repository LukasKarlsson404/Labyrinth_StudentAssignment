using System.Collections.Generic;
using UnityEngine;

public interface IMapData
{
    int Width { get; }
    int Height { get; }

    bool HasHorizontalWall(int x, int y);
    bool HasVerticalWall(int x, int y);

    float GetHorizontalWallCost(int x, int y);
    float GetVerticalWallCost(int x, int y);

    Vector2Int GetQuestStart(int questIndex);
    Vector2Int GetQuestGoal(int questIndex);

    // Directed jump connections supplied by the generated map.
    bool HasJumpFrom(int x, int y);
    bool TryGetJumpCost(Vector2Int from, Vector2Int to, out float cost);
    List<JumpConnection> GetJumpsFrom(Vector2Int from);
}

public readonly struct JumpConnection
{
    public Vector2Int To { get; }
    public float Cost { get; }

    public JumpConnection(Vector2Int to, float cost)
    {
        To = to;
        Cost = cost;
    }
}
