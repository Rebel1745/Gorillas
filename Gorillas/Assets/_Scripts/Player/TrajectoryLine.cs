using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    [SerializeField] private int _maxSegmentCount = 50;
    [SerializeField] private float _curveLength = 5f;

    private Vector3[] _segments;
    private List<Vector3> _segmentsList = new();
    private LineRenderer _lineRenderer;
    private float _projectilePower;
    private Transform _spawnPoint;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
    }

    void UpdateLineSegments()
    {
        _segmentsList.Clear();

        // set the start position of the line renderer
        Vector3 startPos = _spawnPoint.position;
        Vector3 previousPos = startPos;
        bool pathComplete = false;

        _segmentsList.Add(startPos);

        Vector3 startVelocity = _spawnPoint.right * _projectilePower;
        for (int i = 1; i < _maxSegmentCount; i++)
        {
            float timeOffset = (i * Time.fixedDeltaTime * _curveLength);
            Vector3 gravityOffset = 0.5f * Mathf.Pow(timeOffset, 2) * Physics2D.gravity;
            Vector3 newPos = startPos + startVelocity * timeOffset + gravityOffset;
            Vector3 rayDir = previousPos - newPos;

            foreach (var h in Physics2D.RaycastAll(newPos, rayDir, 0.1f))
            {
                // if there is, bail
                if (h.transform.CompareTag("Ground") || h.transform.CompareTag("Player"))
                {
                    pathComplete = true;
                    break;
                }
            }

            previousPos = newPos;
            _segmentsList.Add(newPos);

            if (pathComplete) break;
        }

        _segments = _segmentsList.ToArray();
        _lineRenderer.positionCount = _segments.Length;
        _lineRenderer.SetPositions(_segments);
    }

    public void SetPower(float power)
    {
        _projectilePower = power;
        UpdateLineSegments();
    }

    public void SetSpawnPoint(Transform spawnPoint)
    {
        _spawnPoint = spawnPoint;
    }
}
