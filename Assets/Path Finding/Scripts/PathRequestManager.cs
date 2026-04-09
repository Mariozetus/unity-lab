using System;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    public static PathRequestManager Instance { get; private set; }

    private Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
    private PathRequest _currentRequest;

    private Pathfinding _pathfinding;

    private bool _isProcessingPath;


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        _pathfinding = GetComponent<Pathfinding>();
    }

    public void RequestPath(Vector3 start, Vector3 end, Action<Vector3[], bool> callback)
    {
        PathRequest request = new PathRequest(start, end, callback);

        _pathRequestQueue.Enqueue(request);
        TryProcessNext();
    }

    private void TryProcessNext()
    {
        if(!_isProcessingPath && _pathRequestQueue.Count > 0)
        {
            _currentRequest = _pathRequestQueue.Dequeue();
            _isProcessingPath = true;
            _pathfinding.StartFindPath(_currentRequest.pathStart, _currentRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success) {
        _currentRequest.callback(path, success);
        _isProcessingPath = false;
        TryProcessNext();
    }
}

public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;

    public Action<Vector3[], bool> callback;

    public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        this.pathStart = pathStart;
        this.pathEnd = pathEnd;
        this.callback = callback;
    }
}
