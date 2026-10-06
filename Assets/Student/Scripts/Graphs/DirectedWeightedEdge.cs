public class DirectedWeightedEdge
{
    int from;
    int to;
    double weight;

    public DirectedWeightedEdge(int from, int to, double weight)
    {
        this.from = from;
        this.to = to;
        this.weight = weight;
    }

    public double Weight()
    {
        return weight;
    }

    public int From()
    {
        return from;
    }

    public int To()
    {
        return to;
    }

    public int CompareTo(DirectedWeightedEdge other)
    {
        if (weight < other.Weight()) return -1;
        else if (weight > other.Weight()) return 1;
        else return 0;
    }
}