using System;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    [SerializeField] private Transform seeker;
    [SerializeField] private Transform target;

    private const int DiagonalMoveCost = 14;
    private const int StraightMoveCost = 10;
    private Grid _grid;

    private Heap<Node> _availableNodes;
    private HashSet<Node> _visitedNodes;

    void Awake()
    {
        _grid = GetComponent<Grid>();    
        _availableNodes = new Heap<Node>(_grid.MaxSize);
        _visitedNodes = new HashSet<Node>();
    }

    void Update()
    {
        FindPath(seeker.position, target.position);
    }

    private void FindPath(Vector3 startPoint, Vector3 endPoint)
    {
        Node startingNode = _grid.GetNodeFromPoint(startPoint);
        Node endingNode = _grid.GetNodeFromPoint(endPoint);

        // TO-DO: si el nodo no es alcanzable, llegar al mas cercano
        if(!endingNode.IsWalkable)
            return;

        _availableNodes.Clear();
        _visitedNodes.Clear();

        _availableNodes.Add(startingNode);

        while(_availableNodes.Count > 0)
        {
            // Get the node with the lowest F cost
            Node currentNode = _availableNodes.RemoveFirst();
            _visitedNodes.Add(currentNode);

            // Is finished the path
            if(currentNode == endingNode)
            {
                RetracePath(startingNode, endingNode);
                return;
            }

            // Update the neighbour costs and parents
            List<Node> neighboursNodes = _grid.GetNodeNeighbours(currentNode);

            foreach(Node neighbourNode in neighboursNodes)
            {
                // If is valid for path
                if(!neighbourNode.IsWalkable || _visitedNodes.Contains(neighbourNode))
                    continue;

                int newMovementCostToNeighbour = currentNode.GCost + GetDistanceBetweenNodes(currentNode, neighbourNode);

                // Update the neighbour node costs and parent if the new path is more optimal or 
                // if the node is not in the available list
                if(newMovementCostToNeighbour < neighbourNode.GCost || !_availableNodes.Contains(neighbourNode))
                {
                    neighbourNode.GCost = newMovementCostToNeighbour;
                    neighbourNode.HCost = GetDistanceBetweenNodes(neighbourNode, endingNode);
                    neighbourNode.Parent = currentNode;

                    // Add to available nodes if is not in the list
                    if(!_availableNodes.Contains(neighbourNode))
                        _availableNodes.Add(neighbourNode);
                }
            }
        }
    }

    private void RetracePath(Node startingNode, Node endingNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endingNode;

        while(currentNode != startingNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();

        _grid.SetPath(path);
    }

    // Diagonal Distance (eight directions)
    private int GetDistanceBetweenNodes(Node nodeA, Node nodeB)
    {
        int xDistance = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int yDistance = Mathf.Abs(nodeA.GridY - nodeB.GridY);
        int distance;

        if(xDistance > yDistance)
            distance = DiagonalMoveCost * yDistance + StraightMoveCost * (xDistance - yDistance);
        else
            distance = DiagonalMoveCost * xDistance + StraightMoveCost * (yDistance - xDistance);

        return distance;
    }

    // Manhattan Distance (four directions)        
    private int GetManhattanDistanceBetweenNodes(Node nodeA, Node nodeB)
    {
        int xDistance = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int yDistance = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        return xDistance + yDistance;
    }

    private bool IsNodeMoreOptimal(List<Node> candidateNodes, Node currentNode, int index)
    {
        return candidateNodes[index].FCost < currentNode.FCost || candidateNodes[index].FCost == currentNode.FCost && candidateNodes[index].HCost < currentNode.HCost;
    }
}
