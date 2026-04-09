using System;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 3f;

    private Vector3[] _path;
    private int _targetIndex;
    private Coroutine _followPathCoroutine;

    void Start()
    {
        PathRequestManager.Instance.RequestPath(transform.position, target.position, OnPathFound);
    }

    private void OnPathFound(Vector3[] path, bool wasSearchSuccesful)
    {
        if (wasSearchSuccesful)
        {
            _path = path;
            
            if(_followPathCoroutine != null)
                StopCoroutine(_followPathCoroutine);
            
            _followPathCoroutine = StartCoroutine(FollowPath());
        }
    }

    private IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = _path[0];

        while (true)
        {
            Vector3 targetPosition = new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.z);

            if(Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                _targetIndex++;

                if(_targetIndex >= _path.Length)
                {
                    _targetIndex = 0;
                    _path = new Vector3[0];
                    yield break;
                }
                
                currentWaypoint = _path[_targetIndex];
                targetPosition = new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.z);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }
}