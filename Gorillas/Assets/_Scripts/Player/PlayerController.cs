using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
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
    private bool _initialTrajectoryLine;

    // UI Stuff
    private GameObject _uIGO;
    private TMP_Text _powerText;
    private Slider _powerSlider;
    private TMP_Text _angleText;
    private Slider _angleSlider;
    private Button _launchButton;

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

    public void SetPlayerDetails(int id, GameObject uIGO, PlayerDetails playerDetails)
    {
        _playerId = id;
        _playerDetails = playerDetails;
        _uIGO = uIGO;
        _powerText = _uIGO.transform.GetChild(1).GetComponent<TMP_Text>();
        _powerSlider = _uIGO.transform.GetChild(2).GetComponent<Slider>();
        _powerSlider.onValueChanged.AddListener(UpdatePower);
        _angleText = _uIGO.transform.GetChild(4).GetComponent<TMP_Text>();
        _angleSlider = _uIGO.transform.GetChild(5).GetComponent<Slider>();
        _angleSlider.onValueChanged.AddListener(UpdateAngle);
        _launchButton = _uIGO.transform.GetChild(6).GetComponent<Button>();
        _launchButton.onClick.AddListener(LaunchProjectile);
        _trajectoryLine.SetSpawnPoint(_projectileLaunchPoint);
        _initialTrajectoryLine = true;

        SetLaunchButtonActive(false);

        _powerText.text = _powerSlider.value.ToString("F1");
        _angleText.text = _angleSlider.value.ToString("F1");
        UpdateLaunchPointAngle(_angleSlider.value);
    }

    public void LaunchProjectile()
    {
        // set animation and return to idle
        PlayerManager.Instance.SetPlayerAnimation(_playerId, "Throw");
        StartCoroutine(ResetAnimation(_delayBeforeAttackAnimationReset));

        SetLaunchButtonActive(false);

        GameObject projectile = Instantiate(_projectilePrefab, _projectileLaunchPoint.position, Quaternion.identity);

        projectile.GetComponent<Rigidbody2D>().linearVelocity = _defaultForceMultiplier * _powerSlider.value * _projectileLaunchPoint.right;
        projectile.GetComponent<IProjectile>().SetProjectileExplosionMaskParent(_explosionMaskParent);

        CameraManager.Instance.UpdateCameraForProjectile();

        _playerDetails.PlayerLineRenderer.enabled = false;
        _playerDetails.PlayerUI.SetActive(false);

        GameManager.Instance.UpdateGameState(GameState.WaitingForDetonation);
    }

    IEnumerator ResetAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        PlayerManager.Instance.SetPlayerAnimation(_playerId, "Idle");
    }

    public void SetLaunchButtonActive(bool active)
    {
        _launchButton.enabled = active;
    }

    public IEnumerator CalculateTrajectoryLine()
    {
        float start = Time.time;

        if (_initialTrajectoryLine)
            yield return new WaitForSeconds(0.5f);

        _initialTrajectoryLine = false;
        SetLaunchButtonActive(true);
        UpdatePower(_powerSlider.value);
    }

    public void ShowTrajectoryLine()
    {
        _trajectoryLine.ShowTrajectoryLine();
    }

    public void UpdatePower(float power)
    {
        _trajectoryLine.SetPower(_defaultForceMultiplier * power);

        _powerText.text = power.ToString("F1");
    }

    public void UpdateAngle(float angle)
    {
        UpdateLaunchPointAngle(angle);
        _trajectoryLine.SetPower(_defaultForceMultiplier * _powerSlider.value);

        _angleText.text = angle.ToString("F1");
    }

    private void UpdateLaunchPointAngle(float angle)
    {
        Quaternion launchAngle = Quaternion.Euler(0f, _projectileLaunchPoint.eulerAngles.y, angle);
        _projectileLaunchPoint.rotation = launchAngle;
    }

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
}
