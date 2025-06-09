using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileLaunchPoint;
    [SerializeField] private float _defaultForceMultiplier = 10f;
    private Transform _explosionMaskParent;

    // UI Stuff
    [SerializeField] private TMP_Text _powerText;
    [SerializeField] private Slider _powerSlider;
    [SerializeField] private TMP_Text _angleText;
    [SerializeField] private Slider _angleSlider;
    [SerializeField] private Button _launchButton;

    private void Start()
    {
        _powerText.text = _powerSlider.value.ToString("F1");
        _angleText.text = _angleSlider.value.ToString("F1");

        _explosionMaskParent = GameObject.Find("ExplosionMasks").transform;
    }

    public void LaunchProjectile()
    {
        SetLaunchButtonActive(false);
        Quaternion launchAngle = Quaternion.Euler(0, _projectileLaunchPoint.rotation.eulerAngles.y, _angleSlider.value);
        _projectileLaunchPoint.rotation = launchAngle;

        GameObject projectile = Instantiate(_projectilePrefab, _projectileLaunchPoint.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody2D>().AddForce(_defaultForceMultiplier * _powerSlider.value * _projectileLaunchPoint.right, ForceMode2D.Force);
        projectile.GetComponent<IProjectile>().SetProjectileExplosionMaskParent(_explosionMaskParent);

        GameManager.Instance.UpdateGameState(GameState.WaitingForDetonation);
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
