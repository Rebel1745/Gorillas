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
        bool containsMask = false;
        Vector3 zenith = Vector3.zero;

        _segmentsList.Add(startPos);

        Vector3 startVelocity = _spawnPoint.right * _projectilePower;
        for (int i = 1; i < _maxSegmentCount; i++)
        {
            float timeOffset = (i * Time.fixedDeltaTime * _curveLength);
            Vector3 gravityOffset = 0.5f * Mathf.Pow(timeOffset, 2) * Physics2D.gravity;
            Vector3 newPos = startPos + startVelocity * timeOffset + gravityOffset;
            Vector3 rayDir = newPos - previousPos;
            float rayDistance = Vector3.Distance(previousPos, newPos);
            RaycastHit2D[] hits = Physics2D.CircleCastAll(previousPos, 0.01f, rayDir, rayDistance);
            containsMask = false;

            foreach (var hit in hits)
            {
                // if we hit a mask, we can keep going and ignore any ground or player hits
                if (hit.transform.CompareTag("ExplosionMask"))
                {
                    containsMask = true;
                    break;
                }
            }

            if (!containsMask)
            {
                foreach (var hit in hits)
                {
                    // if there is no mask, we can check for other hits
                    if (hit.transform.CompareTag("Ground") || hit.transform.CompareTag("Player"))
                    {
                        pathComplete = true;
                        //newPos = hit.point;
                    }
                }
            }

            // if the point is at a higher Y-value than currently saved, update it so we can use it as the highest point for the camera to track
            if (newPos.y > zenith.y)
            {
                zenith = newPos;
                // add the new zenith
                CameraManager.Instance.SetProjectileZenith(zenith);
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
