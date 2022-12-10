namespace Crpg.Module.Balancing;

internal class PriorityQueue<T>
{
    private (T element, float priority)[] _nodes;

    public PriorityQueue()
    {
        _nodes = new (T, float)[16];
    }

    public int Count { get; private set; }

    public void Enqueue(T element, float priority)
    {
        if (Count >= _nodes.Length)
        {
            Array.Resize(ref _nodes, Count * 2);
        }

        _nodes[Count] = (element, priority);
        SiftUp(Count++);
    }

    public T Dequeue()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException();
        }

        var element = _nodes[0].element;
        _nodes[0] = _nodes[--Count];
        if (Count > 0)
        {
            SiftDown(0);
        }

        return element;
    }

    private void SiftUp(int n)
    {
        var node = _nodes[n];
        for (int n2 = n / 2; n > 0 && node.priority > _nodes[n2].priority; n = n2, n2 /= 2)
        {
            _nodes[n] = _nodes[n2];
        }

        _nodes[n] = node;
    }

    private void SiftDown(int n)
    {
        var element = _nodes[n];
        for (int n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
        {
            if (n2 + 1 < Count && _nodes[n2 + 1].priority > _nodes[n2].priority)
            {
                n2++;
            }

            if (element.priority >= _nodes[n2].priority)
            {
                break;
            }

            _nodes[n] = _nodes[n2];
        }

        _nodes[n] = element;
    }
}
