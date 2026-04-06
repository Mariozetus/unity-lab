using System;

public class Heap<T> where T : IHeapItem<T>
{
    private T[] _items;
    private int _currentItemCount;

    public Heap(int heapSize)
    {
        _items = new T[heapSize];
    }

    public void Add(T item) 
    {
        item.HeapIndex = _currentItemCount;
        _items[_currentItemCount] = item;

        SiftUp(item);

        _currentItemCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = _items[0];
        _currentItemCount--;

        T lastItem = _items[_currentItemCount];
        _items[_currentItemCount] = default;
        
        if(_currentItemCount > 0){
            _items[0] = lastItem;
            _items[0].HeapIndex = 0;
            SiftDown(_items[0]);
        }

        firstItem.HeapIndex = -1;

        return firstItem;
    }

    public void UpdateItem(T item){
        SiftUp(item);
    }

    public bool Contains(T item)
    {
        if (item.HeapIndex < 0 || item.HeapIndex >= _currentItemCount)
            return false;

        return Equals(_items[item.HeapIndex], item);
    }

    // Sort an item on the heap to its correct position from the root
    private void SiftDown(T item)
    {
        while(item.HeapIndex < _currentItemCount){
            
            int leftChildIndex = item.HeapIndex * 2 + 1;
            int rightChildIndex = item.HeapIndex * 2 + 2;

            // If the left child index is out of the bounds of the heap, then there are no children and we are done
            if(leftChildIndex >= _currentItemCount)
                return;            

            int swapIndex = leftChildIndex;

            // If the right child index is in bounds
            if(rightChildIndex < _currentItemCount){
                // Check which child has higher priority and set it as the swap index
                if(_items[leftChildIndex].CompareTo(_items[rightChildIndex]) < 0)
                    swapIndex = rightChildIndex;
            }

            // If the item has higher or equal priority than the highest priority child, then we are done
            if(item.CompareTo(_items[swapIndex]) >= 0)
                return;
            
            Swap(item, _items[swapIndex]);
        }
    }



    // Sort an item on the heap to its correct position from the bottom
    private void SiftUp(T item)
    {
        // Move the item up the heap until its in the correct position
        while (item.HeapIndex > 0)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;
            T parentItem = _items[parentIndex];

            // If item has same or lower priority, its in the right spot
            if (item.CompareTo(parentItem) <= 0)
                break;

            Swap(item, parentItem);
        }
    }

    private void Swap(T itemA, T itemB)
    {
        _items[itemA.HeapIndex] = itemB;
        _items[itemB.HeapIndex] = itemA;

        (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
    }

    public void Clear()
    {
        _currentItemCount = 0;
    }

    public int Count => _currentItemCount;
}

public interface IHeapItem<T> : IComparable<T>
{
    public int HeapIndex {
        get;
        set;
    }
}