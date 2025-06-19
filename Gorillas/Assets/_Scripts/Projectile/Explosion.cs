using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float _cameraShakeDuration = 0.1f;
    [SerializeField] private float _cameraShakeAmount = 1f;
    void Start()
    {
        CameraShake.Instance.Shake(_cameraShakeDuration, _cameraShakeAmount);
        Destroy(gameObject, 2f);
    }
}
