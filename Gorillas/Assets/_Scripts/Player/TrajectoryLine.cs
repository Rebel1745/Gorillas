using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrajectoryLine : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    private float _gravity;
    [SerializeField] private int _maxNumPoints = 100;
    [SerializeField] private float _timeStep = 0.1f;
    private Vector3[] _segments;
    public int SegmentCount { get { return _segmentsList.Count; } }
    public Vector3 LastSegment { get { return _segmentsList.Last(); } }
    private List<Vector3> _segmentsList = new();
    private bool _hitPlayer;
    public bool HitPlayer { get { return _hitPlayer; } }
    private bool _hitGround;
    public bool HitGround { get { return _hitGround; } }

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        _gravity = -Physics2D.gravity.y;
    }

    public void CalculateTrajectoryLine(float angle, float power, Vector3 spawnPoint, int direction)
    {
        float totalTime = 0f;
        _segmentsList.Clear();

        // set the start position of the line renderer
        Vector3 startPos = spawnPoint;
        Vector3 previousPos = startPos;
        bool pathComplete = false;
        _hitGround = false;
        _hitPlayer = false;
        Vector3 zenith = new(0f, -Mathf.Infinity, 0f);
        Vector3 newPos = Vector3.zero, rayDir;
        float rayDistance = 0f;
        RaycastHit2D[] hits;
        bool containsMask;

        for (int i = 0; i < _maxNumPoints; i++)
        {
            float angleRad = Mathf.Deg2Rad * angle;
            float vx = power * Mathf.Cos(angleRad);
            float vy = power * Mathf.Sin(angleRad);

            float x = vx * totalTime * direction;
            float y = vy * totalTime - 0.5f * _gravity * totalTime * totalTime;

            totalTime += _timeStep;
            newPos = spawnPoint + new Vector3(x, y, 0f);
            rayDir = newPos - previousPos;
            rayDistance = Vector3.Distance(previousPos, newPos);
            hits = Physics2D.CircleCastAll(previousPos, 0.1f, rayDir, rayDistance);
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
                    if (hit.transform.CompareTag("Ground"))
                    {
                        pathComplete = true;
                        _hitGround = true;
                        //newPos = hit.point;
                    }
                    // if there is no mask, we can check for other hits
                    if (hit.transform.CompareTag("Player"))
                    {
                        pathComplete = true;
                        _hitPlayer = true;
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

        DrawTrajectoryLine();
    }

    public void DrawTrajectoryLine()
    {
        _lineRenderer.positionCount = _segments.Length;
        _lineRenderer.SetPositions(_segments);
    }

    private void HideTrajectoryLine()
    {
        _lineRenderer.positionCount = 0;
    }
}