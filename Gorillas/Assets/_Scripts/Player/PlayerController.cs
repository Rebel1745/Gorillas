using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    public GameObject ProjectilePrefab { get { return _projectilePrefab; } }
    [SerializeField] private Transform _projectileLaunchPoint;
    [SerializeField] private float _defaultForceMultiplier = 10f;
    [SerializeField] private float _delayBeforeAttackAnimationReset;
    [SerializeField] private TrajectoryLine _trajectoryLine;
    [SerializeField] private float _powerIncreaseStep = 0.1f;
    [SerializeField] private float _powerIncreaseMax = 1f;
    private float _currentPowerIncrease;
    private bool _updatePower;
    private float _powerDelta;
    [SerializeField] private float _angleIncreaseStep = 0.1f;
    [SerializeField] private float _angleIncreaseMax = 1f;
    private float _currentAngleIncrease;
    private bool _updateAngle;
    private float _angleDelta;
    private Transform _explosionMaskParent;
    private int _playerId;
    public int PlayerId { get { return _playerId; } }
    private PlayerDetails _playerDetails;
    public bool AlwaysShowTrajectoryLine { get { return _playerDetails.AlwaysShowTrajectoryLine; } }
    public bool IsCPU { get { return _playerDetails.IsCPU; } }
    public CPU_TYPE CPUType { get { return _playerDetails.CPUType; } }
    private bool _initialTrajectoryLine;
    [SerializeField] private AudioClip _throwSFX;

    // UI Stuff
    private GameObject _uIGO;
    private TMP_Text _powerText;
    private Slider _powerSlider;
    public Slider PowerSlider { get { return _powerSlider; } }
    private TMP_Text _angleText;
    private Slider _angleSlider;
    public Slider AngleSlider { get { return _angleSlider; } }
    private Button _launchButton;

    // AI Details
    private int _throwNumber;
    public int ThrowNumber { get { return _throwNumber; } }
    public float LastProjectileLandingPositionX;

    private void Start()
    {
        _explosionMaskParent = GameObject.Find("ExplosionMasks").transform;
    }

    private void Update()
    {
        if (_updatePower)
            ChangePower();

        if (_updateAngle)
        {
            ChangeAngle();
        }
    }

    public void SetPlayerDetails(int id, PlayerDetails playerDetails)
    {
        _playerId = id;
        _playerDetails = playerDetails;
        _uIGO = playerDetails.PlayerUI.transform.GetChild(0).gameObject;
        _powerText = _uIGO.transform.GetChild(1).GetComponent<TMP_Text>();
        _powerSlider = _uIGO.transform.GetChild(2).GetComponent<Slider>();
        _angleText = _uIGO.transform.GetChild(4).GetComponent<TMP_Text>();
        _angleSlider = _uIGO.transform.GetChild(5).GetComponent<Slider>();
        _initialTrajectoryLine = true;

        if (!_playerDetails.IsCPU)
        {
            _powerSlider.value = 50f;
            _angleSlider.value = 45f;
            _powerSlider.onValueChanged.AddListener(UpdatePower);
            _angleSlider.onValueChanged.AddListener(UpdateAngle);
            _launchButton = _uIGO.transform.GetChild(6).GetComponent<Button>();
            _launchButton.onClick.AddListener(LaunchProjectile);
        }

        _throwNumber = 0;

        EnableDisableAllUIButtons(false);

        _powerText.text = _powerSlider.value.ToString("F1");
        _angleText.text = _angleSlider.value.ToString("F1");
    }

    public void LaunchProjectile()
    {
        _throwNumber++;
        // set animation and return to idle
        PlayerManager.Instance.SetPlayerAnimation(_playerId, "Throw");
        AudioManager.Instance.PlayAudioClip(_throwSFX, 0.95f, 1.05f);
        StartCoroutine(ResetAnimation(_delayBeforeAttackAnimationReset));

        EnableDisableAllUIButtons(false);

        GameObject projectile = Instantiate(_projectilePrefab, _projectileLaunchPoint.position, Quaternion.identity);

        float angleRad = Mathf.Deg2Rad * _angleSlider.value;
        Vector2 force = new(
            _defaultForceMultiplier * _powerSlider.value * Mathf.Cos(angleRad),
            _defaultForceMultiplier * _powerSlider.value * Mathf.Sin(angleRad)
        );
        force.x *= _playerDetails.ThrowDirection;
        projectile.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        projectile.GetComponent<IProjectile>().SetProjectileExplosionMaskParent(_explosionMaskParent);

        CameraManager.Instance.UpdateCameraForProjectile();

        //_playerDetails.PlayerLineRenderer.enabled = false;
        HideTrajectoryLine();
        UIManager.Instance.ShowHideUIElement(_playerDetails.PlayerUI, false);

        GameManager.Instance.UpdateGameState(GameState.WaitingForDetonation);
    }

    IEnumerator ResetAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        PlayerManager.Instance.SetPlayerAnimation(_playerId, "Idle");
    }

    public IEnumerator CalculateTrajectoryLine()
    {
        if (PlayerId == 0 && _initialTrajectoryLine)
            yield return new WaitForSeconds(0.5f);

        _initialTrajectoryLine = false;
        if (!_playerDetails.IsCPU)
        {
            EnableDisableAllUIButtons(true);
            UpdatePower(_powerSlider.value);
        }
    }

    public void ShowTrajectoryLine()
    {
        _trajectoryLine.DrawTrajectoryLine();
    }

    private void HideTrajectoryLine()
    {
        _trajectoryLine.HideTrajectoryLine();
    }

    public void UpdatePower(float power)
    {
        _trajectoryLine.HideTrajectoryLine();
        _trajectoryLine.CalculateTrajectoryLine(_angleSlider.value, _defaultForceMultiplier * power, _projectileLaunchPoint.position, _playerDetails.ThrowDirection);

        _powerText.text = power.ToString("F1");
        if (_playerDetails.IsCPU) _powerSlider.value = power;
    }

    public void UpdateAngle(float angle)
    {
        _trajectoryLine.HideTrajectoryLine();
        _trajectoryLine.CalculateTrajectoryLine(angle, _defaultForceMultiplier * _powerSlider.value, _projectileLaunchPoint.position, _playerDetails.ThrowDirection);

        _angleText.text = angle.ToString("F1");
        if (_playerDetails.IsCPU) _angleSlider.value = angle;
    }

    public void UpdateAngleAndPower(float angle, float power)
    {
        _trajectoryLine.HideTrajectoryLine();
        _trajectoryLine.CalculateTrajectoryLine(angle, _defaultForceMultiplier * power, _projectileLaunchPoint.position, _playerDetails.ThrowDirection);

        _angleText.text = angle.ToString("F1");
        if (_playerDetails.IsCPU) _angleSlider.value = angle;

        _powerText.text = power.ToString("F1");
        if (_playerDetails.IsCPU) _powerSlider.value = power;
    }

    public void UpdateAngleAndPower(float angle, float power, bool ignoreGroundHits)
    {
        _trajectoryLine.HideTrajectoryLine();
        _trajectoryLine.CalculateTrajectoryLine(angle, _defaultForceMultiplier * power, _projectileLaunchPoint.position, _playerDetails.ThrowDirection, ignoreGroundHits);

        _angleText.text = angle.ToString("F1");
        if (_playerDetails.IsCPU) _angleSlider.value = angle;

        _powerText.text = power.ToString("F1");
        if (_playerDetails.IsCPU) _powerSlider.value = power;
    }

    private void EnableDisableAllUIButtons(bool enable)
    {
        // first sort the launch button
        UIManager.Instance.EnableDisableButton(_launchButton, enable);

        // the sort the powerup buttons
        for (int i = 0; i < _playerDetails.PlayerUIPowerupHolder.childCount; i++)
        {
            UIManager.Instance.EnableDisableButton(_playerDetails.PlayerUIPowerupHolder.GetChild(i).GetComponent<Button>(), enable);
        }
    }

    public void DestroyPlayer()
    {
        gameObject.SetActive(false);
    }

    #region Input Controls
    public void StartPowerChange(float delta)
    {
        _powerDelta = delta;
        _updatePower = true;
    }
    public void StopPowerChange()
    {
        _currentPowerIncrease = _powerIncreaseStep;
        _updatePower = false;
    }

    private void ChangePower()
    {
        _currentPowerIncrease += Mathf.Clamp(_currentPowerIncrease, _powerIncreaseStep, _powerIncreaseMax);
        _powerSlider.value += _powerDelta * _currentPowerIncrease * Time.deltaTime;
    }

    public void StartAngleChange(float delta)
    {
        _angleDelta = delta;
        _updateAngle = true;
    }
    public void StopAngleChange()
    {
        _currentAngleIncrease = _angleIncreaseStep;
        _updateAngle = false;
    }

    private void ChangeAngle()
    {
        _currentAngleIncrease += Mathf.Clamp(_currentAngleIncrease, _angleIncreaseStep, _angleIncreaseMax);
        _angleSlider.value += _angleDelta * _currentAngleIncrease * Time.deltaTime;
    }
    #endregion
}
