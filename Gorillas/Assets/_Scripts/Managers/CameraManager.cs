using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private List<Transform> _cameraTargets = new();
    private Camera _camera;
    [SerializeField] private Vector3 _cameraOffset;
    [SerializeField] private float _minZoom = 5f;
    [SerializeField] private float _maxZoom = 10f;
    [SerializeField] private float _distanceToZoomMultiplierX = 0.3f;
    [SerializeField] private float _distanceToZoomMultiplierY = 1f;
    private Vector3 _moveVelocity;
    private float _zoomVelocity;
    private float _moveSmoothTime = 0.5f;
    private float _zoomSmoothTime = 0.1f;
    private Bounds _cameraBounds;
    private bool _initialCameraMovement;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _camera = Camera.main;
        _initialCameraMovement = true;
    }

    private void LateUpdate()
    {
        if (_cameraTargets.Count == 0) return;

        if (_initialCameraMovement)
        {
            SetBounds();
            ZoomCamera(0f);
            MoveCamera(0f);
            _initialCameraMovement = false;
            return;
        }

        if (_cameraTargets.Count > 2)
            SetBounds();

        ZoomCamera(_zoomSmoothTime);
        MoveCamera(_moveSmoothTime);
    }

    private void MoveCamera(float smoothTime)
    {
        Vector3 lowest = new(GetCenterPoint().x, _camera.orthographicSize + GetLowestPlayer(), 0f);
        Vector3 newPos = _cameraOffset + lowest;

        _camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, newPos, ref _moveVelocity, smoothTime);
        //_camera.transform.position = newPos;
    }

    private void ZoomCamera(float smoothTime)
    {
        float zoom = Mathf.Max(GetBoundsSize().x * _distanceToZoomMultiplierX, GetBoundsSize().y * _distanceToZoomMultiplierY);
        zoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);

        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, zoom, ref _zoomVelocity, smoothTime);
        //_camera.orthographicSize = zoom;
    }

    private Vector3 GetCenterPoint()
    {
        if (_cameraTargets.Count == 1) return _cameraTargets[0].position;

        return _cameraBounds.center;
    }

    private float GetLowestPlayer()
    {
        if (_cameraTargets.Count == 0) return 0f;
        if (_cameraTargets.Count == 1) return _cameraTargets[0].transform.position.y;

        float _lowestTarget = Mathf.Infinity;

        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            if (_cameraTargets[i].position.y < _lowestTarget)
                _lowestTarget = _cameraTargets[i].position.y;
        }

        return _lowestTarget;
    }

    private Vector3 GetBoundsSize()
    {
        if ((_cameraTargets.Count == 1)) return _cameraTargets[0].position;
        //Debug.Log(_cameraBounds.size);
        return _cameraBounds.size;
    }

    private void SetBounds()
    {
        _cameraBounds = new Bounds(_cameraTargets[0].position, Vector3.zero);
        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            _cameraBounds.Encapsulate(_cameraTargets[i].position);
        }
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
