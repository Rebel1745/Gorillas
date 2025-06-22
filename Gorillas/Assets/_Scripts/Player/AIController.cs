using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private float _minPowerMissed = 0f;
    private float _maxPowerMissed = 100f;
    private float _minAngleMissed = 0f;
    private float _maxAngleMissed = 100f;

    public IEnumerator DoAI(PlayerController pc)
    {
        Slider _powerSlider = pc.PowerSlider;
        float currentPower = _powerSlider.value;
        float minPower = _powerSlider.minValue;
        float maxPower = _powerSlider.maxValue;

        Slider _angleSlider = pc.AngleSlider;
        float currentAngle = _angleSlider.value;
        float minAngle = _angleSlider.minValue;
        float maxAngle = _angleSlider.maxValue;

        int throwNumber = pc.ThrowNumber;
        float previousAttackLandingPositionX = pc.LastProjectileLandingPositionX;
        int otherPlayerId = (pc.PlayerId + 1) % 2;
        float otherPlayerXPos = PlayerManager.Instance.Players[otherPlayerId].PlayerGameObject.transform.position.x;

        float newPower, newAngle = 45f;

        if (throwNumber == 0)
        {
            // random values to start with
            newPower = Random.Range(minPower, maxPower);
            //newAngle = Random.Range(minAngle, maxAngle);
        }
        else
        {
            // if player 1
            if (pc.PlayerId == 0)
            {
                // if we landed before the target, increase the power from a minimum of the last power
                if (otherPlayerXPos > previousAttackLandingPositionX)
                {
                    _minPowerMissed = currentPower;
                    newPower = Random.Range(currentPower, _maxPowerMissed);
                }
                else
                {
                    _maxPowerMissed = currentPower;
                    newPower = Random.Range(_minPowerMissed, currentPower);
                }
            }
            else
            {
                if (otherPlayerXPos > previousAttackLandingPositionX)
                {
                    _maxPowerMissed = currentPower;
                    newPower = Random.Range(_minPowerMissed, currentPower);
                }
                else
                {
                    _minPowerMissed = currentPower;
                    newPower = Random.Range(currentPower, _maxPowerMissed);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        pc.UpdatePower(newPower);

        yield return new WaitForSeconds(0.5f);

        pc.UpdateAngle(newAngle);

        yield return new WaitForSeconds(1f);

        pc.LaunchProjectile();
    }
}
