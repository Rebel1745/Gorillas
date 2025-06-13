using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private List<Transform> _cameraTargets = new();
    private Camera _camera;
    [SerializeField] private Vector3 _cameraOffset;
    [SerializeField] private float _minZoom = 5f;
    [SerializeField] private float _maxZoom = 10f;
    [SerializeField] private float _distanceToZoomMultiplier = 0.5f;
    private Vector3 _moveVelocity;
    private float _zoomVelocity;
    private float _smoothTime = 0.5f;
    private Bounds _cameraBounds;
    private Vector3 _internalOffset;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_cameraTargets.Count == 0) return;

        MoveCamera();
        ZoomCamera();
    }

    private void MoveCamera()
    {
        Vector3 centrePoint = GetCentrePoint();

        Vector3 newPos = _cameraOffset + _internalOffset;

        _camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, newPos, ref _moveVelocity, _smoothTime);
    }

    private void ZoomCamera()
    {
        float zoom = Mathf.Clamp(GetGreatestDistance() * _distanceToZoomMultiplier, _minZoom, _maxZoom);

        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, zoom, ref _zoomVelocity, _smoothTime);
    }

    private void GetLowestPlayer()
    {
        _internalOffset.y = Mathf.Infinity;

        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            if (_cameraTargets[i].position.y < _internalOffset.y)
                _internalOffset.y = _camera.orthographicSize + _cameraTargets[i].position.y;
        }
    }

    private Vector3 GetCentrePoint()
    {
        if (_cameraTargets.Count == 1) return _cameraTargets[0].position;

        return _cameraBounds.center;
    }

    private float GetGreatestDistance()
    {
        if (_cameraTargets.Count == 1) return _minZoom;

        return _cameraBounds.size.x;
    }

    private void SetBounds()
    {
        _cameraBounds = new Bounds(_cameraTargets[0].position, Vector3.zero);
        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            _cameraBounds.Encapsulate(_cameraTargets[i].position);
        }

        GetLowestPlayer();
    }

    public void AddTarget(Transform target)
    {
        if (_cameraTargets.Contains(target)) return;

        _cameraTargets.Add(target);

        SetBounds();
    }

    public void RemoveTarget(Transform target)
    {
        if (!_cameraTargets.Contains(target)) return;

        _cameraTargets.Remove(target);

        SetBounds();
    }
}
