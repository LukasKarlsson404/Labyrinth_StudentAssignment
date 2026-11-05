using System.Collections.Generic;
using UnityEngine;

public abstract class PathFinder
{
    public PathFinder(Graph graph) { }

    public abstract bool HasPathTo(int vertex);
    public abstract IEnumerable<int> PathTo(int vertex);
}