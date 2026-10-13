using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinding
{
    double[] gValue;
    double[] hValue;
    //double[] fValue;
    IndexMinPQ<double> open;
    HashSet<int> closed;
    DirectedWeightedEdge[] edgeTo;

    double FValue(int vertex)
    {
        return gValue[vertex] + hValue[vertex];
    }

    public AStarPathFinding(WeightedEdgeDigraph weightedEdgeDiGraph, int source, int target)
    {
        gValue = new double[weightedEdgeDiGraph.Vertices];
        hValue = new double[weightedEdgeDiGraph.Vertices];
        //fValue = new double[weightedEdgeDiGraph.Vertices];
        open = new IndexMinPQ<double>(weightedEdgeDiGraph.Vertices);
        closed = new (weightedEdgeDiGraph.Vertices);
        edgeTo = new DirectedWeightedEdge[weightedEdgeDiGraph.Vertices];

        edgeTo[source] = null;
        open.Insert(source, 0.0);

        int iterations = 0;

        while (!open.IsEmpty())
        {
            iterations++;

            int current = open.DeleteMin();

            if (current == target)
            {
                closed.Add(current);
                break;
            }

            foreach (DirectedWeightedEdge edge in weightedEdgeDiGraph.GetNeighbors(current))
            {
                int neighbor = edge.To();

                if (!open.Contains(neighbor) && !closed.Contains(neighbor))
                {
                    gValue[neighbor] = double.PositiveInfinity;
                }

                double neighborsNewGValue = gValue[current] + edge.Weight();
                if ((open.Contains(neighbor) || closed.Contains(neighbor)) && gValue[neighbor] <= neighborsNewGValue)
                {
                    continue;
                }

                gValue[neighbor] = neighborsNewGValue;
                hValue[neighbor] = Heuristic(neighbor, target, weightedEdgeDiGraph.MapData);
                //fValue[neighbor] = gValue[neighbor] + hValue[neighbor];
                edgeTo[neighbor] = edge;

                if (closed.Contains(neighbor))
                {
                    closed.Remove(neighbor);
                }

                if (!open.Contains(neighbor))
                {
                    open.Insert(neighbor, FValue(neighbor));
                }
            }

            closed.Add(current);
        }

        Debug.Log($"A* Pathfinding completed in {iterations} iterations");
    }

    double Heuristic(int vertex, int target, IMapData mapData)
    {
        Vector2Int vertexPos = new Vector2Int(vertex % mapData.Width, vertex / mapData.Width);
        Vector2Int targetPos = new Vector2Int(target % mapData.Width, target / mapData.Width);
        return Mathf.Abs(vertexPos.x - targetPos.x) + Mathf.Abs(vertexPos.y - targetPos.y); // Manhattan distance
    }

    public double DistTo(int vertex)
    {
        return gValue[vertex];
    }

    public bool HasPathTo(int vertex)
    {
        bool hasPath = closed.Contains(vertex) || open.Contains(vertex);

        if (!hasPath)
        {
            Debug.Log($"No path to vertex {vertex}");
            Debug.Log($"Closed list: {string.Join(", ", closed)}");
        }

        return hasPath;
    }

    public IEnumerable<DirectedWeightedEdge> PathTo(int vertex)
    {
        if (!HasPathTo(vertex))
            return null;

        Stack<DirectedWeightedEdge> path = new Stack<DirectedWeightedEdge>();
        for (DirectedWeightedEdge edge = edgeTo[vertex]; edge != null; edge = edgeTo[edge.From()])
        {
            path.Push(edge);
        }

        return path;
    }
}