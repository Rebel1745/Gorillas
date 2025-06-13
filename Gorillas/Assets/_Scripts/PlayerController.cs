using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Data.Common;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileLaunchPoint;
    [SerializeField] private float _defaultForceMultiplier = 10f;
    private Transform _explosionMaskParent;
    private int _playerId;
    [SerializeField] private float _delayBeforeAttackAnimationReset;

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

    public void SetPlayerDetails(int id, GameObject uIGO)
    {
        _playerId = id;
        _uIGO = uIGO;
        _powerText = _uIGO.transform.GetChild(1).GetComponent<TMP_Text>();
        _powerSlider = _uIGO.transform.GetChild(2).GetComponent<Slider>();
        _powerSlider.onValueChanged.AddListener(UpdatePowerText);
        _angleText = _uIGO.transform.GetChild(4).GetComponent<TMP_Text>();
        _angleSlider = _uIGO.transform.GetChild(5).GetComponent<Slider>();
        _angleSlider.onValueChanged.AddListener(UpdateAngleText);
        _launchButton = _uIGO.transform.GetChild(6).GetComponent<Button>();
        _launchButton.onClick.AddListener(LaunchProjectile);

        _powerText.text = _powerSlider.value.ToString("F1");
        _angleText.text = _angleSlider.value.ToString("F1");
    }

    public void LaunchProjectile()
    {
        // set animation and return to idle
        PlayerManager.Instance.SetPlayerAnimation(_playerId, "Throw");
        StartCoroutine(ResetAnimation(_delayBeforeAttackAnimationReset));

        SetLaunchButtonActive(false);
        Quaternion launchAngle = Quaternion.Euler(0, _projectileLaunchPoint.rotation.eulerAngles.y, _angleSlider.value);
        _projectileLaunchPoint.rotation = launchAngle;

        GameObject projectile = Instantiate(_projectilePrefab, _projectileLaunchPoint.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody2D>().AddForce(_defaultForceMultiplier * _powerSlider.value * _projectileLaunchPoint.right, ForceMode2D.Force);
        projectile.GetComponent<IProjectile>().SetProjectileExplosionMaskParent(_explosionMaskParent);

        CameraManager.Instance.AddTarget(projectile.transform);

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

    public void UpdatePowerText(float power)
    {
        _powerText.text = power.ToString("F1");
    }

    public void UpdateAngleText(float angle)
    {
        _angleText.text = angle.ToString("F1");
    }
}
