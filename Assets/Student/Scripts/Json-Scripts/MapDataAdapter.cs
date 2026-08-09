using System.Collections.Generic;
using UnityEngine;

public class MapDataAdapter : IMapData
{
    private readonly MapData mapData;
    private readonly Dictionary<Vector2Int, float> horizontalWallCosts = new();
    private readonly Dictionary<Vector2Int, float> verticalWallCosts = new();
    private readonly Dictionary<Vector2Int, List<JumpConnection>> jumpsFrom = new();

    public MapDataAdapter(MapData mapData)
    {
        this.mapData = mapData;
        BuildLookupTables();
    }

    public int Width => mapData.width;
    public int Height => mapData.height;

    private void BuildLookupTables()
    {
        if (mapData.hwalls != null)
        {
            foreach (Wall wall in mapData.hwalls)
            {
                if (wall != null)
                    horizontalWallCosts[new Vector2Int(wall.x, wall.y)] = wall.cost;
            }
        }

        if (mapData.vwalls != null)
        {
            foreach (Wall wall in mapData.vwalls)
            {
                if (wall != null)
                    verticalWallCosts[new Vector2Int(wall.x, wall.y)] = wall.cost;
            }
        }

        if (mapData.jumps != null)
        {
            foreach (Jump jump in mapData.jumps)
            {
                if (jump?.from == null || jump.to == null)
                    continue;

                Vector2Int from = new Vector2Int(jump.from.x, jump.from.y);
                Vector2Int to = new Vector2Int(jump.to.x, jump.to.y);

                if (!IsInsideMap(from) || !IsInsideMap(to))
                {
                    Debug.LogWarning($"Ignoring jump outside map bounds: {from} -> {to}");
                    continue;
                }

                if (!jumpsFrom.TryGetValue(from, out List<JumpConnection> connections))
                {
                    connections = new List<JumpConnection>();
                    jumpsFrom[from] = connections;
                }

                connections.Add(new JumpConnection(to, jump.cost));
            }
        }
    }

    private bool IsInsideMap(Vector2Int position)
    {
        return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
    }

    public bool HasHorizontalWall(int x, int y)
    {
        return horizontalWallCosts.ContainsKey(new Vector2Int(x, y));
    }

    public bool HasVerticalWall(int x, int y)
    {
        return verticalWallCosts.ContainsKey(new Vector2Int(x, y));
    }

    public float GetHorizontalWallCost(int x, int y)
    {
        return horizontalWallCosts.TryGetValue(new Vector2Int(x, y), out float cost)
            ? cost
            : 1.0f;
    }

    public float GetVerticalWallCost(int x, int y)
    {
        return verticalWallCosts.TryGetValue(new Vector2Int(x, y), out float cost)
            ? cost
            : 1.0f;
    }

    public Vector2Int GetQuestStart(int questIndex)
    {
        if (mapData.quests == null || questIndex < 0 || questIndex >= mapData.quests.Length)
            return Vector2Int.zero;

        Quest quest = mapData.quests[questIndex];
        return new Vector2Int(quest.from.x, quest.from.y);
    }

    public Vector2Int GetQuestGoal(int questIndex)
    {
        if (mapData.quests == null || questIndex < 0 || questIndex >= mapData.quests.Length)
            return Vector2Int.zero;

        Quest quest = mapData.quests[questIndex];
        return new Vector2Int(quest.to.x, quest.to.y);
    }

    public bool HasJumpFrom(int x, int y)
    {
        return jumpsFrom.ContainsKey(new Vector2Int(x, y));
    }

    public bool TryGetJumpCost(Vector2Int from, Vector2Int to, out float cost)
    {
        if (jumpsFrom.TryGetValue(from, out List<JumpConnection> connections))
        {
            foreach (JumpConnection connection in connections)
            {
                if (connection.To == to)
                {
                    cost = connection.Cost;
                    return true;
                }
            }
        }

        cost = float.MaxValue;
        return false;
    }

    public List<JumpConnection> GetJumpsFrom(Vector2Int from)
    {
        if (jumpsFrom.TryGetValue(from, out List<JumpConnection> connections))
            return new List<JumpConnection>(connections);

        return new List<JumpConnection>();
    }
}
