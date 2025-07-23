using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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

    // powerup stuff
    private bool _isBigBomb = false;
    private int _burstCount = 1;
    private int _currentBurstNumber;
    private float _lastLaunchTime;
    [SerializeField] float _timeBetweenBurstFire = 0.25f;
    bool _isBurstFiring = false;
    bool _isVariablePower = false;
    [SerializeField] float _variablePowerAmount = 0.5f;
    private float _variablePowerAmountPerShotOfBurst;
    private float _currentVariablePowerAmount;
    [SerializeField] private Transform _shieldTransform;
    private bool _isShieldActive = false;
    public bool IsShieldActive { get { return _isShieldActive; } }
    private int _movementDistance;

    // UI Stuff
    private GameObject _uIGO;
    private TMP_Text _powerText;
    private Slider _powerSlider;
    public Slider PowerSlider { get { return _powerSlider; } }
    private TMP_Text _angleText;
    private Slider _angleSlider;
    public Slider AngleSlider { get { return _angleSlider; } }
    private Button _launchButton;
    private int _currentArrowIndex = -1;

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

        if (_isBurstFiring)
        {
            CheckBurstFire();
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
            _launchButton.onClick.AddListener(StartLaunchProjectile);
        }

        _throwNumber = 0;

        EnableDisableAllUIButtons(false);

        _powerText.text = _powerSlider.value.ToString("F1");
        _angleText.text = _angleSlider.value.ToString("F1");
    }

    private void CheckBurstFire()
    {
        if (_currentBurstNumber == _burstCount)
        {
            EndLaunchProjectile();
            return;
        }

        if (Time.time >= _lastLaunchTime + _timeBetweenBurstFire)
        {
            _currentBurstNumber++;
            LaunchProjectile();
        }
    }

    private void EndLaunchProjectile()
    {
        _isBurstFiring = false;
        _burstCount = 1;
        _currentBurstNumber = 0;
        _isVariablePower = false;
        _currentVariablePowerAmount = 0f;

        UIManager.Instance.ShowHideUIElement(_playerDetails.PlayerUI, false);

        GameManager.Instance.UpdateGameState(GameState.WaitingForDetonation);
    }

    public void StartLaunchProjectile()
    {
        _throwNumber++;
        EnableDisableAllUIButtons(false);
        ShowHideMovementPowerupIndicators(false);

        HideTrajectoryLine();

        if (_burstCount == 1)
        {
            LaunchProjectile();
            EndLaunchProjectile();
            return;
        }

        _currentBurstNumber = 0;
        _isBurstFiring = true;
    }

    private void LaunchProjectile()
    {
        // set animation and return to idle
        PlayerManager.Instance.SetPlayerAnimation(_playerId, "Throw");
        AudioManager.Instance.PlayAudioClip(_throwSFX, 0.95f, 1.05f);

        if (_isBurstFiring)
            StartCoroutine(ResetAnimation(_timeBetweenBurstFire - 0.05f));
        else
            StartCoroutine(ResetAnimation(_delayBeforeAttackAnimationReset));

        GameObject projectile = Instantiate(_projectilePrefab, _projectileLaunchPoint.position, Quaternion.identity);

        float powerValue = _powerSlider.value;

        if (_isVariablePower)
        {
            powerValue += _currentVariablePowerAmount;
            _currentVariablePowerAmount -= _variablePowerAmountPerShotOfBurst;
        }

        float angleRad = Mathf.Deg2Rad * _angleSlider.value;
        Vector2 force = new(
            _defaultForceMultiplier * powerValue * Mathf.Cos(angleRad),
            _defaultForceMultiplier * powerValue * Mathf.Sin(angleRad)
        );
        force.x *= _playerDetails.ThrowDirection;
        projectile.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        IProjectile iProjectile = projectile.GetComponent<IProjectile>();
        iProjectile.SetProjectileExplosionMaskParent(_explosionMaskParent);

        if (_isBurstFiring)
        {
            // if this is the first projectile launched we can follow it
            if (_currentBurstNumber == 1)
                CameraManager.Instance.UpdateCameraForProjectile();

            iProjectile.SetProjectileNumber(_currentBurstNumber);

            if (_currentBurstNumber == _burstCount)
                iProjectile.SetLastProjectileInBurst();
        }
        else
        {
            CameraManager.Instance.UpdateCameraForProjectile();
            iProjectile.SetProjectileNumber(1);
            iProjectile.SetLastProjectileInBurst();
        }

        if (_isBigBomb)
        {
            iProjectile.SetExplosionSizeMultiplier(2f);
            _isBigBomb = false;
        }

        _lastLaunchTime = Time.time;
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

    public void UpdatePower(float power)
    {
        _trajectoryLine.HideTrajectoryLine();
        _trajectoryLine.CalculateTrajectoryLine(_angleSlider.value, _defaultForceMultiplier * power + _currentVariablePowerAmount, _projectileLaunchPoint.position, _playerDetails.ThrowDirection);

        _powerText.text = power.ToString("F1");
        if (_playerDetails.IsCPU) _powerSlider.value = power;
    }

    public void UpdateAngle(float angle)
    {
        _trajectoryLine.HideTrajectoryLine();
        _trajectoryLine.CalculateTrajectoryLine(angle, _defaultForceMultiplier * _powerSlider.value + _currentVariablePowerAmount, _projectileLaunchPoint.position, _playerDetails.ThrowDirection);

        _angleText.text = angle.ToString("F1");
        if (_playerDetails.IsCPU) _angleSlider.value = angle;
    }

    public void UpdateAngleAndPower(float angle, float power)
    {
        _trajectoryLine.HideTrajectoryLine();
        _trajectoryLine.CalculateTrajectoryLine(angle, _defaultForceMultiplier * power + _currentVariablePowerAmount, _projectileLaunchPoint.position, _playerDetails.ThrowDirection);

        _angleText.text = angle.ToString("F1");
        if (_playerDetails.IsCPU) _angleSlider.value = angle;

        _powerText.text = power.ToString("F1");
        if (_playerDetails.IsCPU) _powerSlider.value = power;
    }

    public void UpdateAngleAndPower(float angle, float power, bool ignoreGroundHits)
    {
        _trajectoryLine.HideTrajectoryLine();
        _trajectoryLine.CalculateTrajectoryLine(angle, _defaultForceMultiplier * power + _currentVariablePowerAmount, _projectileLaunchPoint.position, _playerDetails.ThrowDirection, ignoreGroundHits);

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

    public void PlacePlayerAndEnable(Vector3 position, int spawnPointIndex)
    {
        _throwNumber = 0;
        transform.position = position;
        _playerDetails.SpawnPointIndex = spawnPointIndex;
        PlayerManager.Instance.Players[_playerId].SpawnPointIndex = spawnPointIndex;
        if (_playerDetails.IsCPU)
            _playerDetails.PlayerAIController.ForceRecalculatePerfectShot();
        gameObject.SetActive(true);
    }

    public void DestroyPlayer()
    {
        gameObject.SetActive(false);
    }

    #region Powerup Functions
    public void ShowTrajectoryLine()
    {
        _trajectoryLine.DrawTrajectoryLine();
    }

    private void HideTrajectoryLine()
    {
        _trajectoryLine.HideTrajectoryLine();
    }

    public void SetBigBomb()
    {
        _isBigBomb = true;
    }

    public void SetProjectileBurst(int number)
    {
        _burstCount = number;
    }

    public void SetVariablePower()
    {
        _isVariablePower = true;
        _variablePowerAmountPerShotOfBurst = (_burstCount - 1) / 2f * _variablePowerAmount;
        _currentVariablePowerAmount = _variablePowerAmountPerShotOfBurst;
    }

    public void ShowShield()
    {
        _isShieldActive = true;
        _shieldTransform.gameObject.SetActive(true);
        //_gorillaCollider.enabled = false;
    }

    public void HideShield()
    {
        _isShieldActive = false;
        _shieldTransform.gameObject.SetActive(false);
        //_gorillaCollider.enabled = true;
    }

    public void ShowHideMovementPowerupIndicators(bool show)
    {
        ShowHideMovementPowerupIndicators(_movementDistance, show);
    }

    public void ShowHideMovementPowerupIndicators(int distance, bool show)
    {
        float lowestY = 999;
        float currentY;
        float lowestPlayerY;
        Vector3 newCameraPosition;

        _movementDistance = distance;

        // figure out the span of the arrows / spawn points
        int firstIndex = _playerDetails.SpawnPointIndex - distance;
        int lastIndex = _playerDetails.SpawnPointIndex + distance;

        // check whether player 1's last index is not too close to player 2
        if (_playerId == 0)
        {
            if (PlayerManager.Instance.Players[1].SpawnPointIndex - LevelManager.Instance.MinimumDistanceBetweenPlayers < lastIndex)
                lastIndex = PlayerManager.Instance.Players[1].SpawnPointIndex - LevelManager.Instance.MinimumDistanceBetweenPlayers;
        }
        else
        {
            // check whether player 2's first index is not too close to player 1
            if (PlayerManager.Instance.Players[0].SpawnPointIndex + LevelManager.Instance.MinimumDistanceBetweenPlayers > firstIndex)
                firstIndex = PlayerManager.Instance.Players[0].SpawnPointIndex + LevelManager.Instance.MinimumDistanceBetweenPlayers;
        }

        // show the arrows on screen
        LevelManager.Instance.ShowHideSpawnPointArrowsBetweenIndexes(firstIndex, _playerDetails.SpawnPointIndex, lastIndex, show);

        // figure out the lowest spawn point (or player if it is lower)
        for (int i = firstIndex; i <= lastIndex; i++)
        {
            currentY = LevelManager.Instance.GetSpawnPointAtIndex(i).y;
            if (currentY < lowestY)
                lowestY = currentY;
        }

        // we have the lowest Y of the spawn points, now get the lowest player
        lowestPlayerY = Mathf.Min(PlayerManager.Instance.Players[0].PlayerGameObject.transform.position.y, PlayerManager.Instance.Players[1].PlayerGameObject.transform.position.y);
        lowestY = Mathf.Min(lowestY, lowestPlayerY);

        if (_playerId == 0)
            newCameraPosition = new(LevelManager.Instance.GetSpawnPointAtIndex(firstIndex).x, lowestY);
        else
            newCameraPosition = new(LevelManager.Instance.GetSpawnPointAtIndex(lastIndex).x, lowestY);

        CameraManager.Instance.UpdatePlayerPosition(_playerId, newCameraPosition);
    }

    public void SetPlayerMovementSprite(int arrowIndex)
    {
        if (_currentArrowIndex != arrowIndex)
        {
            _currentArrowIndex = arrowIndex;
            Vector3 spawnPointPosition = LevelManager.Instance.GetSpawnPointAtIndex(arrowIndex);

            if (_playerDetails.PlayerMovementSpriteGO == null)
            {
                _playerDetails.PlayerMovementSpriteGO = Instantiate(_playerDetails.PlayerMovementSpritePrefab, transform);
            }

            _playerDetails.PlayerMovementSpriteGO.transform.position = spawnPointPosition;
            _playerDetails.PlayerMovementSpriteGO.SetActive(true);
        }
    }

    public void HidePlayerMovementSprite()
    {
        if (_playerDetails.PlayerMovementSpriteGO != null)
            _playerDetails.PlayerMovementSpriteGO.SetActive(false);
        _currentArrowIndex = -1;
    }

    public void ConfirmMovementPowerupPosition()
    {
        if (_currentArrowIndex == -1) return;

        ShowHideMovementPowerupIndicators(_movementDistance, false);
        transform.position = LevelManager.Instance.GetSpawnPointAtIndex(_currentArrowIndex);
        _playerDetails.SpawnPointIndex = _currentArrowIndex;
        PlayerManager.Instance.Players[_playerId].SpawnPointIndex = _currentArrowIndex;
        HidePlayerMovementSprite();
        StartCoroutine(CalculateTrajectoryLine());
        CameraManager.Instance.UpdatePlayerPosition(_playerId, transform.position);
        InputManager.Instance.EnableDisableGameplayControls(true);

        // if we have moved, tell the other player to recalculate the trajectory if they are CPU
        if (PlayerManager.Instance.Players[GameManager.Instance.OtherPlayerId].IsCPU)
            PlayerManager.Instance.Players[GameManager.Instance.OtherPlayerId].PlayerAIController.ForceRecalculatePerfectShot();

        GameManager.Instance.UpdateGameState(GameState.WaitingForLaunch);
    }

    public void CancelMovementPowerupPosition()
    {
        ShowHideMovementPowerupIndicators(false);
        HidePlayerMovementSprite();
        CameraManager.Instance.UpdatePlayerPosition(_playerId, transform.position);
        GameManager.Instance.UpdateGameState(GameState.WaitingForLaunch);
    }
    #endregion

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
