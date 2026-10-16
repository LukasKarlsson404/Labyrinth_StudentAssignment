using System.Collections.Generic;
using UnityEngine;

public class DijkstraShortestPath
{
    DirectedWeightedEdge[] edgeTo;
    double[] distTo;
    IndexMinPQ<double> pq;

    public DijkstraShortestPath(WeightedEdgeDigraph weightedEdgeDiGraph, int source)
    {
        edgeTo = new DirectedWeightedEdge[weightedEdgeDiGraph.Vertices];
        distTo = new double[weightedEdgeDiGraph.Vertices];
        pq = new IndexMinPQ<double>(weightedEdgeDiGraph.Vertices);

        for (int v = 0; v < weightedEdgeDiGraph.Vertices; v++)
        {
            distTo[v] = double.PositiveInfinity;
        }

        distTo[source] = 0.0;
        pq.Insert(source, 0.0);

        int iterations = 0;

        while (!pq.IsEmpty())
        {
            iterations++;
            Relax(weightedEdgeDiGraph, pq.DeleteMin());
        }

        Debug.Log($"Dijkstra's algorithm completed in {iterations} iterations.");
    }

    void Relax(WeightedEdgeDigraph weightedEdgeDiGraph, int v)
    {
        foreach (DirectedWeightedEdge edge in weightedEdgeDiGraph.GetNeighbors(v))
        {
            int w = edge.To();
            if (distTo[w] > distTo[v] + edge.Weight())
            {
                distTo[w] = distTo[v] + edge.Weight();
                edgeTo[w] = edge;
                if (pq.Contains(w))
                {
                    pq.ChangeKey(w, distTo[w]);
                }
                else
                {
                    pq.Insert(w, distTo[w]);
                }
            }
        }
    }

    public double DistTo(int vertex)
    {
        return distTo[vertex];
    }

    public bool HasPathTo(int vertex)
    {
        return !(distTo[vertex] == double.PositiveInfinity);
    }

    public IEnumerable<int> PathTo(int vertex)
    {
        if (!HasPathTo(vertex))
            return null;
        Stack<int> path = new Stack<int>();
        for (DirectedWeightedEdge edge = edgeTo[vertex]; edge != null; edge = edgeTo[edge.From()])
        {
            path.Push(edge.From());
        }

        return path;
    }
}