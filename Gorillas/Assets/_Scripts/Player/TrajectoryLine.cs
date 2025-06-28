using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class TrajectoryLine : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private PlayerController _playerController;
    private float _gravity;
    private float _mass;
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
    private Vector3 _targetPosition;
    private List<GameObject> _groundHitList = new();
    public int GroundHitCount { get { return _groundHitList.Count; } }
    private Rigidbody2D _projectileRB;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        _projectileRB = _playerController.ProjectilePrefab.GetComponent<Rigidbody2D>();
        _gravity = -Physics2D.gravity.y * _projectileRB.gravityScale;
        _mass = _projectileRB.mass;
    }

    // ignoreBuildingHits will not stop the line when it hits the ground (buildings), but it will create a list of the buildings hit
    // it will register a hit on the ground, but only if the banana has already gone past the target player
    public void CalculateTrajectoryLine(float angle, float power, Vector3 spawnPoint, int direction, bool ignoreGroundHits = false)
    {
        float totalTime = 0f;
        _segmentsList.Clear();
        _groundHitList.Clear();

        int targetPlayerId = (_playerController.PlayerId + 1) % 2;
        _targetPosition = PlayerManager.Instance.Players[targetPlayerId].PlayerGameObject.transform.position;

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
            float vx = (power / _mass) * Mathf.Cos(angleRad);
            float vy = (power / _mass) * Mathf.Sin(angleRad);

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
                        _hitGround = true;
                        //newPos = hit.point;
                        if (ignoreGroundHits)
                        {
                            AddBuildingToHitList(hit.transform.gameObject);
                            if (_playerController.PlayerId == 0)
                            {
                                if (rayDir.y < 0 || newPos.x > _targetPosition.x)
                                {
                                    pathComplete = true;
                                }
                            }
                            else
                            {
                                if (rayDir.y < 0 || newPos.x < _targetPosition.x)
                                {
                                    pathComplete = true;
                                }
                            }
                        }
                        else
                        {
                            pathComplete = true;
                        }
                    }
                    // if there is no mask, we can check for other hits
                    if (hit.transform.CompareTag("Player"))
                    {
                        pathComplete = true;

                        // if we are in the second phase of the AI calculations, only mark that we have hit the player
                        // if we have hit the target player, not the throwing player
                        if (!ignoreGroundHits || hit.transform.gameObject == PlayerManager.Instance.Players[targetPlayerId].PlayerGameObject)
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

            // check to see if the line has gone below some depth
            if (newPos.y < -15f) pathComplete = true;

            if (pathComplete) break;
        }
        _segments = _segmentsList.ToArray();

        //Debug.Log($"CalculateTrajectoryLine Angle {angle} Power {power * 4} Last {LastSegment.x} Count {SegmentCount}");

        DrawTrajectoryLine();
    }

    private void AddBuildingToHitList(GameObject groundObject)
    {
        if (!_groundHitList.Contains(groundObject))
        {
            _groundHitList.Add(groundObject);
        }
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