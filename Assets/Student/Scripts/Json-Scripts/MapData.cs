using System;
using UnityEngine;

[Serializable]
public class MapData
{
    public int width;
    public int height;
    public Wall[] hwalls;
    public Wall[] vwalls;
    public Quest[] quests;
    public Jump[] jumps;

    public void NormalizeDefaults()
    {
        if (hwalls == null) hwalls = Array.Empty<Wall>();
        if (vwalls == null) vwalls = Array.Empty<Wall>();
        if (quests == null) quests = Array.Empty<Quest>();
        if (jumps == null) jumps = Array.Empty<Jump>();

        NormalizeWallCosts(hwalls);
        NormalizeWallCosts(vwalls);
    }

    private static void NormalizeWallCosts(Wall[] walls)
    {
        foreach (Wall wall in walls)
        {
            if (wall != null && wall.cost <= 0f)
                wall.cost = float.MaxValue;
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
