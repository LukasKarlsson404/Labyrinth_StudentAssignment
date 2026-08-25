using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData
{
    public int width;
    public int height;
    public Wall[] hwalls;
    public Wall[] vwalls;
    public Quest[] quests;

    // Current format: directed jump connections.
    public Jump[] jumps;

    // Legacy format: vent positions. If jumps are not supplied, every vent
    // can teleport to every other vent with the default movement cost.
    public Position[] vents;

    public void NormalizeDefaults()
    {
        if (hwalls == null) hwalls = Array.Empty<Wall>();
        if (vwalls == null) vwalls = Array.Empty<Wall>();
        if (quests == null) quests = Array.Empty<Quest>();
        if (vents == null) vents = Array.Empty<Position>();

        // A file uses either jumps or vents. Prefer explicit jumps when they
        // exist; otherwise convert the legacy vent list to directed jumps so
        // the rest of the project only has to understand one connection type.
        if (jumps == null || jumps.Length == 0)
            jumps = BuildJumpsFromVents(vents);

        if (jumps == null) jumps = Array.Empty<Jump>();

        NormalizeWallCosts(hwalls);
        NormalizeWallCosts(vwalls);
        NormalizeJumpCosts(jumps);
    }

    private static Jump[] BuildJumpsFromVents(Position[] ventPositions)
    {
        if (ventPositions == null || ventPositions.Length < 2)
            return Array.Empty<Jump>();

        List<Jump> generatedJumps = new();

        // Legacy vent behaviour allowed teleportation between vent cells.
        // Create both directions (and all pairs when a map has >2 vents).
        for (int i = 0; i < ventPositions.Length; i++)
        {
            Position from = ventPositions[i];
            if (from == null) continue;

            for (int j = 0; j < ventPositions.Length; j++)
            {
                if (i == j) continue;

                Position to = ventPositions[j];
                if (to == null) continue;

                generatedJumps.Add(new Jump
                {
                    from = new Position { x = from.x, y = from.y },
                    to = new Position { x = to.x, y = to.y },
                    cost = 1f
                });
            }
        }

        return generatedJumps.ToArray();
    }

    private static void NormalizeWallCosts(Wall[] walls)
    {
        foreach (Wall wall in walls)
        {
            if (wall != null && wall.cost <= 0f)
                wall.cost = float.MaxValue;
        }
    }

    private static void NormalizeJumpCosts(Jump[] jumpConnections)
    {
        foreach (Jump jump in jumpConnections)
        {
            if (jump != null && jump.cost <= 0f)
                jump.cost = 1f;
        }
    }
}

[Serializable]
public class Wall
{
    public int x;
    public int y;
    public float cost;
}

[Serializable]
public class Quest
{
    public Position from;
    public Position to;
}

[Serializable]
public class Jump
{
    public Position from;
    public Position to;
    public float cost;
}

[Serializable]
public class Position
{
    public int x;
    public int y;
}
