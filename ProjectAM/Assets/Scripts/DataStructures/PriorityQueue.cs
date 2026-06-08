using System.Collections.Generic;

public class PriorityQueue<T>
{
    private readonly List<T> heap = new List<T>();
    private readonly IComparer<T> comparer;

    public PriorityQueue() : this(Comparer<T>.Default) { }

    public PriorityQueue(IComparer<T> comparer)
    {
        this.comparer = comparer;
    }

    public int Count => heap.Count;

    public void Clear()
    {
        heap.Clear();
    }

    public void Push(T item)
    {
        heap.Add(item);

        int rootIndex = 0;
        int currentIndex = heap.Count - 1;

        while (currentIndex > rootIndex)
        {
            int parentIndex = (currentIndex - 1) / 2;

            T parent = heap[parentIndex];
            T child = heap[currentIndex];
            if (comparer.Compare(parent, child) <= 0) break;

            (heap[parentIndex], heap[currentIndex]) = (heap[currentIndex], heap[parentIndex]);
            currentIndex = parentIndex;
        }
    }

    public T Pop()
    {
        T root = heap[0];
        
        int lastIndex = heap.Count - 1;
        heap[0] = heap[lastIndex];
        heap.RemoveAt(lastIndex);

        int count = heap.Count;
        int currentIndex = 0;
        while (true)
        {
            int leftChildIndex = 2 * currentIndex + 1;
            int rightChildIndex = 2 * currentIndex + 2;
            int smallestIndex = currentIndex;

            if (leftChildIndex < count && comparer.Compare(heap[leftChildIndex], heap[smallestIndex]) < 0) smallestIndex = leftChildIndex;
            if (rightChildIndex < count && comparer.Compare(heap[rightChildIndex], heap[smallestIndex]) < 0) smallestIndex = rightChildIndex;
            if (smallestIndex == currentIndex) break;

            (heap[currentIndex], heap[smallestIndex]) = (heap[smallestIndex], heap[currentIndex]);
            currentIndex = smallestIndex;
        }

        return root;
    }

    public T Top()
    {
        return heap[0];
    }
}
