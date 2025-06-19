using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 _originalPos;
    private float _timeAtCurrentFrame;
    private float _timeAtLastFrame;
    private float _fakeDelta;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        _timeAtCurrentFrame = Time.realtimeSinceStartup;
        _fakeDelta = _timeAtCurrentFrame - _timeAtLastFrame;
        _timeAtLastFrame = _timeAtCurrentFrame;
    }

    public void Shake(float duration, float amount)
    {
        _originalPos = transform.localPosition;
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(duration, amount));
    }

    public IEnumerator ShakeCoroutine(float duration, float amount)
    {
        float endTime = Time.time + duration;

        while (duration > 0)
        {
            transform.localPosition = _originalPos + Random.insideUnitSphere * amount;
            duration -= _fakeDelta;
            yield return null;
        }

        transform.localPosition = _originalPos;
    }
}
