using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
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

    public void StartFindPath(Vector3 startPoint, Vector3 endPoint)
    {
        StartCoroutine(FindPath(startPoint, endPoint));
    }

    private IEnumerator FindPath(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startingNode = _grid.GetNodeFromPoint(startPoint);
        Node endingNode = _grid.GetNodeFromPoint(endPoint);

        // TO-DO: si el nodo no es alcanzable, llegar al mas cercano
        if (endingNode.IsWalkable)
        {
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
                    pathSuccess = true;
                    break;
                }

                // Update the neighbour costs and parents
                List<Node> neighboursNodes = _grid.GetNodeNeighbours(currentNode);

                foreach(Node neighbourNode in neighboursNodes)
                {
                    // If is valid for path
                    if(!neighbourNode.IsWalkable || _visitedNodes.Contains(neighbourNode))
                        continue;

                    int newMovementCostToNeighbour = currentNode.GCost + GetDistanceBetweenNodes(currentNode, neighbourNode) + neighbourNode.Penalty;

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
                        else
                            _availableNodes.UpdateItem(neighbourNode);
                    }
                }
            }
        }
        
        yield return null;

        if(pathSuccess)
            waypoints = RetracePath(startingNode, endingNode);

        PathRequestManager.Instance.FinishedProcessingPath(waypoints, pathSuccess);
    }

    private Vector3[] RetracePath(Node startingNode, Node endingNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endingNode;
        
        while(currentNode != startingNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;
    }

    private Vector3[] SimplifyPath(List<Node> path)
    {
        if (path == null || path.Count == 0)
            return new Vector3[0];

        List<Vector3> waypoints = new List<Vector3>();
        Vector2 previousDirection = Vector2.zero;

        waypoints.Add(path[0].WorldPosition);

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 nextDirection = new Vector2(path[i - 1].GridX - path[i].GridX, path[i - 1].GridY - path[i].GridY);

            if (nextDirection != previousDirection)
                waypoints.Add(path[i].WorldPosition);

            previousDirection = nextDirection;
        }

        return waypoints.ToArray();
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
}
