using System.Collections.Generic;

public abstract class PathFinder
{
    public abstract bool HasPathTo(int vertex);
    public abstract IEnumerable<int> PathTo(int vertex);
}