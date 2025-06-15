using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private List<Vector3> _cameraTargets = new();
    private Camera _camera;
    [SerializeField] private Vector3 _cameraOffset;
    [SerializeField] private float _minZoom = 5f;
    [SerializeField] private float _maxZoom = 10f;
    private Vector3 _moveVelocity;
    private float _zoomVelocity;
    private float _moveSmoothTime = 0.5f;
    private float _zoomSmoothTime = 0.1f;
    private Bounds _cameraBounds;
    private bool _initialCameraMovement;
    private bool _focusOnPlayers = false;
    private bool _includeProjectileInFocus = false;
    private float _screenHeightWidthRatio;
    private const float PLAYER_WIDTH = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _camera = Camera.main;
        _screenHeightWidthRatio = (float)Screen.width / Screen.height;
        _initialCameraMovement = true;
        _focusOnPlayers = false;
    }

    private void LateUpdate()
    {
        if (_cameraTargets.Count == 0) return;

        if (!_focusOnPlayers) return;

        if (_initialCameraMovement)
        {
            ZoomCamera(0f);
            MoveCamera(0f);
            if (_cameraTargets.Count == 2) _initialCameraMovement = false;
            return;
        }

        if (_includeProjectileInFocus)
        {
            ZoomCamera(_zoomSmoothTime);
            MoveCamera(_moveSmoothTime);
        }
    }

    private void ZoomCamera(float smoothTime)
    {
        float zoom = Mathf.Max((GetBoundsSize().x + _cameraOffset.x) / 2f / _screenHeightWidthRatio, GetBoundsSize().y + Mathf.Abs(_cameraOffset.y));
        zoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);

        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, zoom, ref _zoomVelocity, smoothTime);
    }

    private void MoveCamera(float smoothTime)
    {
        Vector3 newPos = new(_cameraOffset.x / 2 + GetCenterPoint().x, _camera.orthographicSize + GetLowestPlayer() - _cameraOffset.y, _cameraOffset.z);

        _camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, newPos, ref _moveVelocity, smoothTime);
    }

    private Vector3 GetCenterPoint()
    {
        if (_cameraTargets.Count == 1) return _cameraTargets[0];

        return _cameraBounds.center;
    }

    private float GetLowestPlayer()
    {
        if (_cameraTargets.Count == 0) return 0f;
        if (_cameraTargets.Count == 1) return _cameraTargets[0].y;

        float _lowestTarget = Mathf.Infinity;

        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            if (_cameraTargets[i].y < _lowestTarget)
                _lowestTarget = _cameraTargets[i].y;
        }

        return _lowestTarget;
    }

    private Vector3 GetBoundsSize()
    {
        if ((_cameraTargets.Count == 1)) return _cameraTargets[0];
        //Debug.Log(_cameraBounds.size);
        return _cameraBounds.size;
    }

    private void SetBounds()
    {
        _cameraBounds = new Bounds(_cameraTargets[0], Vector3.zero);
        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            _cameraBounds.Encapsulate(_cameraTargets[i]);
        }
    }

    // function waits until two players are added to the scene to set the initial camera position and zoom
    public void AddPlayer(Vector3 target)
    {
        if (_cameraTargets.Contains(target)) return;

        _cameraTargets.Add(target);

        if (_cameraTargets.Count == 2)
        {
            SetBounds();
            _focusOnPlayers = true;
        }
    }

    public void AddProjectileZenith(Vector3 target)
    {
        _cameraTargets.Add(target);
    }

    public void UpdateCameraForProjectile()
    {
        // we now need to update the bounds with the zenith of the projectiles trajectory
        SetBounds();
        Debug.Log(GetBoundsSize().y);
        _includeProjectileInFocus = true;
    }

    public void RemoveTarget(Vector3 target)
    {
        if (!_cameraTargets.Contains(target)) return;

        _cameraTargets.Remove(target);

        SetBounds();
    }
}
