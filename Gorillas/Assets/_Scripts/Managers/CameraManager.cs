using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private List<Vector3> _cameraTargets = new();
    private Camera _camera;
    [SerializeField] private Vector3 _cameraOffset;
    private const float ADDITIONAL_Y_OFFSET = 0.5f; // just to add a little space above the banana trajectory
    [SerializeField] private float _minZoom = 5f;
    [SerializeField] private float _maxZoom = 10f;
    private Vector3 _moveVelocity;
    private float _zoomVelocity;
    [SerializeField] private float _zoomSmoothTime = 0.1f;
    private Bounds _cameraBounds;
    private bool _moveCamera = false;
    private bool _instantCameraMovement;
    private float _screenHeightWidthRatio;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _camera = Camera.main;
        _screenHeightWidthRatio = (float)Screen.width / Screen.height;

        // when we first start, don't move the camera, wait for both players to be loaded
        _moveCamera = false;
        // when the level loads, the camera movement should be instant
        _instantCameraMovement = true;
    }

    private void LateUpdate()
    {
        // initial movement (instant)
        // launch banana (zoom speed)
        // banana destroyed, zoom to players (zoom speed)

        // if there are no targets, bail
        if (_cameraTargets.Count == 0) return;

        // if we are not supposed to move the camera, bail
        if (!_moveCamera) return;

        ZoomCamera(_zoomSmoothTime);
        MoveCamera();
    }

    private void ZoomCamera(float smoothTime)
    {
        float zoom = Mathf.Max(GetBoundsSize().x / 2f / _screenHeightWidthRatio, (GetBoundsSize().y + _cameraOffset.y) / 2.0f + ADDITIONAL_Y_OFFSET);
        zoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);

        if (_instantCameraMovement)
        {
            _camera.orthographicSize = zoom;
            _instantCameraMovement = false;
        }
        else
            _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, zoom, ref _zoomVelocity, smoothTime);
    }

    // moving the camera doesn't need a smooth time becuase it should remain in the same Y position
    private void MoveCamera()
    {
        Vector3 newPos = new(_camera.transform.position.x, _camera.orthographicSize + GetLowestPlayer() - _cameraOffset.y, _cameraOffset.z);

        _camera.transform.position = newPos;
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

        // adjust the size to take into account the offset
        Vector3 ajustedSize = new(_cameraBounds.size.x + _cameraOffset.x / 2, _cameraBounds.size.y, _cameraBounds.size.z);

        //return _cameraBounds.size;
        return ajustedSize;
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
            _moveCamera = true;
        }
    }

    public void RemovePlayer(Vector3 target)
    {
        if (!_cameraTargets.Contains(target)) return;

        _cameraTargets.Remove(target);

        SetBounds();
        _moveCamera = true;
    }

    public void SetProjectileZenith(Vector3 target)
    {
        if (_cameraTargets.Count == 2)
            _cameraTargets.Add(target);
        else
            _cameraTargets[2] = target;
    }

    public void UpdateCameraForProjectile()
    {
        // we now need to update the bounds with the zenith of the projectiles trajectory
        SetBounds();
        _moveCamera = true;
    }

    public void RemoveProjectile()
    {
        // players are 0 and 1, the projectile is 2, remove it
        _cameraTargets.RemoveAt(2);

        SetBounds();
        _moveCamera = true;
    }
}
