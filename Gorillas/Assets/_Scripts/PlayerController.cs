using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileLaunchPoint;
    [SerializeField] private float _defaultForceMultiplier = 10f;

    // UI Stuff
    [SerializeField] private TMP_Text _powerText;
    [SerializeField] private Slider _powerSlider;
    [SerializeField] private TMP_Text _angleText;
    [SerializeField] private Slider _angleSlider;

    private void Start()
    {
        _powerText.text = _powerSlider.value.ToString("F1");
        _angleText.text = _angleSlider.value.ToString("F1");
    }

    public void LaunchProjectile()
    {
        Quaternion launchAngle = Quaternion.Euler(0, 0, _angleSlider.value);
        _projectileLaunchPoint.rotation = launchAngle;

        GameObject projectile = Instantiate(_projectilePrefab, _projectileLaunchPoint.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody2D>().AddForce(_projectileLaunchPoint.right * _powerSlider.value * _defaultForceMultiplier, ForceMode2D.Force);
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
