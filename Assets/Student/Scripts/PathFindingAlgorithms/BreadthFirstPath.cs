using System.Collections.Generic;
using UnityEngine;

public class BreadthFirstPath
{
    bool[] marked;
    int[] edgeTo;
    int source;

    public BreadthFirstPath(Graph graph, int source)
    {
        this.source = source;
        marked = new bool[graph.Vertices];
        edgeTo = new int[graph.Vertices];
        BFS(graph, source);
    }

    private void BFS(Graph graph, int s)
    {
        Queue<int> queue = new Queue<int>();
        
        marked[s] = true;
        queue.Enqueue(s);

        while (queue.Count > 0)
        {
            int currnetVertex = queue.Dequeue();
            foreach (int nextVertex in graph.GetNeighbors(currnetVertex))
            {
                if (!marked[nextVertex])
                {
                    edgeTo[nextVertex] = currnetVertex;
                    marked[nextVertex] = true;
                    queue.Enqueue(nextVertex);
                }
            }
        }
    }

    public bool HasPathTo(int v)
    {
        return marked[v];
    }

    public IEnumerable<int> PathTo(int vertex)
    {
        if (!HasPathTo(vertex))
            return null;
        Stack<int> path = new Stack<int>();
        for (int x = vertex; x != source; x = edgeTo[x])
        {
            path.Push(x);
        }
        path.Push(source);
        return path;
    }
}