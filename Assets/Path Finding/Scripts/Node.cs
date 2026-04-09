using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool IsWalkable { get; private set; }
    public Vector3 WorldPosition { get; private set; }

    public int GCost = 0; // Cost from the starting node to this node
    public int HCost = 0; // Cost from this node to the ending node
    public int FCost => GCost + HCost;

    public int Penalty;

    public Node Parent;
    // Se podria añadir vecinos, depende si quieres priorizar rendimiento a memoria

    public int GridX { get; private set; }
    public int GridY { get; private set; }

    private int _heapIndex;

    public Node(bool isWalkable, Vector3 worldPosition, int gridX, int gridY, int penalty)
    {
        IsWalkable = isWalkable;
        WorldPosition = worldPosition;
        GridX = gridX;
        GridY = gridY;
        Penalty = penalty;
    }

    public int CompareTo(Node other)
    {
        int compare = FCost.CompareTo(other.FCost);
 
        if(compare == 0)
            compare = HCost.CompareTo(other.HCost);

        return -compare;
    }

    public int HeapIndex { get => _heapIndex; set => _heapIndex = value; }
}
