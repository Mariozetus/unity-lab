using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private LayerMask unwalkableMask;
    [SerializeField] private Vector2 gridWorldSize;
    [SerializeField][Min(0.15f)] private float nodeRadius;
    [SerializeField] List<SurfaceType> walkableSurfaces;
    [SerializeField] private bool displayGridGizmos;

    private Node[,] _grid;

    private LayerMask _walkableMask;
    private Dictionary<int, int> _walkableSurfacesDictionary = new Dictionary<int, int>();

    private float _nodeDiameter => nodeRadius * 2;
    private int _xGridNodeSize, _yGridNodeSize;

    void OnValidate()
    {
        _xGridNodeSize = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
        _yGridNodeSize = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);
        CreateGrid();
    }

    void Awake()
    {
        _xGridNodeSize = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
        _yGridNodeSize = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);

        foreach(SurfaceType surface in walkableSurfaces){
            _walkableMask.value |= surface.SurfaceLayer.value;
            _walkableSurfacesDictionary.Add(surface.LayerIndex, surface.Penalty);
        }

        CreateGrid();
    }

    private void CreateGrid()
    {
        _grid = new Node[_xGridNodeSize, _yGridNodeSize];
        Vector3 gridTopLeftPosition = transform.position 
            + Vector3.left * (gridWorldSize.x / 2) 
            + Vector3.forward * (gridWorldSize.y / 2);

        for(int x = 0; x < _xGridNodeSize; x++)
        {
            for(int y = 0; y < _yGridNodeSize; y++)
            {
                Vector3 gridNodePosition = gridTopLeftPosition 
                    + Vector3.right * (x * _nodeDiameter + nodeRadius)
                    + Vector3.back * (y * _nodeDiameter + nodeRadius);

                bool isWalkable = !Physics.CheckSphere(gridNodePosition, nodeRadius, unwalkableMask);
                
                int nodePenalty = 0;

                if(isWalkable){
                    Ray ray = new Ray(gridNodePosition + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    
                    if(Physics.Raycast(ray, out hit, 100, _walkableMask))
                        _walkableSurfacesDictionary.TryGetValue(hit.collider.gameObject.layer, out nodePenalty);

                }

                _grid[x, y] = new Node(isWalkable, gridNodePosition, x, y, nodePenalty);
            }
        }
    }

    public Node GetNodeFromPoint(Vector3 worldPosition)
    {
        if (_grid == null || _xGridNodeSize == 0 || _yGridNodeSize == 0) return null;

        // Nos aseguremos que usen mismo sistema por si el grid esta girado
        Vector3 localPos = transform.InverseTransformPoint(worldPosition); 
        float xFrac = (localPos.x / gridWorldSize.x) + 0.5f;
        float yFrac = (- localPos.z / gridWorldSize.y) + 0.5f;

        xFrac = Mathf.Clamp01(xFrac);
        yFrac = Mathf.Clamp01(yFrac);

        int x = Mathf.FloorToInt(Mathf.Clamp(_xGridNodeSize * xFrac, 0, _xGridNodeSize - 1));
        int y = Mathf.FloorToInt(Mathf.Clamp(_yGridNodeSize * yFrac, 0, _yGridNodeSize - 1));

        return _grid[x, y];
    }

    public List<Node> GetNodeNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if(x == 0 && y == 0) continue;

                int neighbourX = node.GridX + x;   
                int neighbourY = node.GridY + y;   

                if(neighbourX >= 0 && neighbourX < _xGridNodeSize && neighbourY >= 0 && neighbourY < _yGridNodeSize)
                    neighbours.Add(_grid[neighbourX, neighbourY]);
            }
        }
 
        return neighbours;
    }

    public int MaxSize => _xGridNodeSize * _yGridNodeSize;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(!displayGridGizmos) return;

        if (_grid != null)
        {
            foreach(Node node in _grid)
            {
                Gizmos.color = node.IsWalkable ? Color.green : Color.red;
                Gizmos.DrawCube(node.WorldPosition, new Vector3(_nodeDiameter - 0.1f, 0.1f, _nodeDiameter - 0.1f));          
            }
        }
    }
#endif
}

[System.Serializable]
public class SurfaceType
{
    [SerializeField] [SingleLayer] private int surfaceLayer;
    public int LayerIndex => surfaceLayer;
    public LayerMask SurfaceLayer => 1 << surfaceLayer;
    public int Penalty;
}