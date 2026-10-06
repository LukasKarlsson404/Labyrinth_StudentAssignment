using System.Collections;
using System.Diagnostics;

public class IndexMinPQ<T>
{
    int maxN; // maximum number of elements on PQ
    int n; // number of elements on PQ
    int[] pq; // binary heap using 1-based indexing
    int[] qp; // inverse of pq - qp[pq[i]] = pq[qp[i]] = i
    T[] keys; // keys[i] = priority of i

    public IndexMinPQ(int maxN)
    {
        if (maxN < 0)
        {
            throw new System.ArgumentException("maxN must be non-negative");
        }

        this.maxN = maxN;
        n = 0;
        keys = new T[maxN + 1];
        pq = new int[maxN + 1];
        qp = new int[maxN + 1];
        for (int i = 0; i <= maxN; i++)
        {
            qp[i] = -1;
        }
    }

    public void Insert(int index, T key)
    {
        if (ValidIndex(index))
        {
            if (Contains(index))
            {
                UnityEngine.Debug.LogError($"Index {index} is already in the priority queue");
                throw new System.ArgumentException("Index is already in the priority queue");
            }
            n++;
            qp[index] = n;
            pq[n] = index;
            keys[index] = key;
            Swim(n);
        }
        else
        {
            UnityEngine.Debug.LogError($"Index {index} is out of bounds");
        }
    }

    public void ChangeKey(int index, T key)
    {
        if (ValidIndex(index))
        {
            if (!Contains(index))
            {
                UnityEngine.Debug.LogError($"Index {index} is not in the priority queue");
                throw new System.ArgumentException("Index is not in the priority queue");
            }
            UnityEngine.Debug.Log($"Changing key for index {index} to {key}");
            keys[index] = key;
            Swim(qp[index]);
            Sink(qp[index]);
        }
        else
        {
            UnityEngine.Debug.LogError($"Index {index} is out of bounds");
        }
    }

    public bool Contains(int index)
    {
        if (ValidIndex(index))
        {
            return qp[index] != -1;
        }

        return false;
    }

    public void Delete(int index)
    {
        if (ValidIndex(index))
        {
            if (!Contains(index))
            {
                throw new System.ArgumentException("Index is not in the priority queue");
            }
            int i = qp[index];
            Exchange(i, n--);
            Swim(i);
            Sink(i);
            keys[index] = default;
            qp[index] = -1;
        }
    }

    public T Min()
    {
        if (n == 0)
        {
            throw new System.InvalidOperationException("Priority queue underflow");
        }

        return keys[pq[1]];
    }

    public int MinIndex()
    {
        if (n == 0)
        {
            throw new System.InvalidOperationException("Priority queue underflow");
        }
        return pq[1];
    }

    public int DeleteMin()
    {
        if (n == 0)
        {
            throw new System.InvalidOperationException("Priority queue underflow");
        }

        int minIndex = pq[1];
        Exchange(1, n--);
        Sink(1);
        Debug.Assert(pq[n + 1] == minIndex);
        qp[minIndex] = -1; // delete
        keys[minIndex] = default; // to avoid loitering

        return minIndex;
    }

    public bool IsEmpty()
    {
        return n == 0;
    }

    public int Size()
    {
        return n;
    }

    void Swim(int k)
    {
        while (k > 1 && Less(k / 2, k))
        {
            Exchange(k / 2, k);
            k = k / 2;
        }
    }

    void Sink(int k)
    {
        while (2 * k <= n)
        {
            int j = 2 * k;
            if (j < n && Greater(j, j + 1)) j++;
            if (!Greater(k, j)) break;
            Exchange(k, j);
            k = j;
        }
    }

    bool Less(int i, int j)
    {
        return Comparer.Default.Compare(keys[pq[i]], keys[pq[j]]) < 0;
    }

    bool Greater(int i, int j)
    {
        return Comparer.Default.Compare(keys[pq[i]], keys[pq[j]]) > 0;
    }

    void Exchange(int i, int j)
    {
        int swap = pq[i];
        pq[i] = pq[j];
        pq[j] = swap;
        qp[pq[i]] = i;
        qp[pq[j]] = j;
    }

    bool ValidIndex(int index)
    {
        return index >= 0 && index < maxN;
    }
}