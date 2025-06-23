using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;

public class AIController : MonoBehaviour
{
    private float _minPowerMissed = 10f;
    private float _maxPowerMissed = 100f;
    private float _minAngleMissed = 10f;
    private float _maxAngleMissed = 80f;
    private Slider _powerSlider;
    private Slider _angleSlider;
    private int _throwNumber;
    private float _previousAttackLandingPositionX;
    private float _otherPlayerXPos;
    private int _playerId;
    private int _otherPlayerId;

    [SerializeField] private TrajectoryLine _trajectoryLine;
    private PlayerController _playerController;

    public IEnumerator DoAI(PlayerController pc)
    {
        BananaTrajectorySolver bts = new();

        _playerId = pc.PlayerId;
        _otherPlayerId = (_playerId + 1) % 2;

        Vector3 throwingPlayer = PlayerManager.Instance.Players[_playerId].PlayerGameObject.transform.position;
        Vector3 targetPlayer = PlayerManager.Instance.Players[_otherPlayerId].PlayerGameObject.transform.position;

        float xDiff = throwingPlayer.x - targetPlayer.x;
        float yDiff = throwingPlayer.y - targetPlayer.y;

        bts.InitialiseValues(xDiff, yDiff);
        bts.CalculatePower(50f);
        bts.CalculateAngle(60f);

        yield return new WaitForSeconds(0.5f);

        pc.UpdatePower(bts.CalculatePower(50f));

        yield return new WaitForSeconds(1f);

        pc.LaunchProjectile();
    }

    /*public IEnumerator DoAI(PlayerController pc)
    {
        _playerController = pc;
        _powerSlider = pc.PowerSlider;
        _angleSlider = pc.AngleSlider;

        _throwNumber = pc.ThrowNumber;
        _previousAttackLandingPositionX = pc.LastProjectileLandingPositionX;

        _playerId = pc.PlayerId;
        _otherPlayerId = (_playerId + 1) % 2;
        _otherPlayerXPos = PlayerManager.Instance.Players[_otherPlayerId].PlayerGameObject.transform.position.x;

        float newPower, newAngle;

        newPower = CalculatePower();
        newAngle = CalculateAngle();
        float dist = Vector2.Distance(PlayerManager.Instance.Players[0].PlayerGameObject.transform.position, PlayerManager.Instance.Players[1].PlayerGameObject.transform.position);
        Debug.Log(dist + " - " + Physics2D.gravity.y + " - ");
        Debug.Log(CalcAngle(dist, Mathf.Abs(Physics2D.gravity.y), 50f));
        Debug.Log(CalcVelocity(dist, Mathf.Abs(Physics2D.gravity.y), 45f));

        yield return new WaitForSeconds(0.5f);

        pc.UpdatePower(newPower);

        yield return new WaitForSeconds(0.5f);

        pc.UpdateAngle(newAngle);

        yield return new WaitForSeconds(1f);

        pc.LaunchProjectile();
    }

    private float CalculatePower()
    {
        float currentPower = _powerSlider.value;
        float minPower = _powerSlider.minValue;
        float maxPower = _powerSlider.maxValue;

        if (maxPower - minPower < 1.0f) minPower -= 5f;

        if (_throwNumber == 0)
        {
            // random values to start with
            return Random.Range(minPower, maxPower);
        }
        else
        {
            // if player 1
            if (_playerId == 0)
            {
                // if we landed before the target, increase the power from a minimum of the last power
                if (_otherPlayerXPos > _previousAttackLandingPositionX)
                {
                    _minPowerMissed = currentPower;
                    return Random.Range(currentPower, _maxPowerMissed);
                }
                else
                {
                    _maxPowerMissed = currentPower;
                    return Random.Range(_minPowerMissed, currentPower);
                }
            }
            else
            {
                if (_otherPlayerXPos > _previousAttackLandingPositionX)
                {
                    _maxPowerMissed = currentPower;
                    return Random.Range(_minPowerMissed, currentPower);
                }
                else
                {
                    _minPowerMissed = currentPower;
                    return Random.Range(currentPower, _maxPowerMissed);
                }
            }
        }
    }

    private float CalculateAngle()
    {
        float currentAngle = _angleSlider.value;
        float minAngle = _angleSlider.minValue;
        float maxAngle = _angleSlider.maxValue;

        int segments = _trajectoryLine.SegmentCount;

        while (segments < 15)
        {
            currentAngle += 0.5f;
            _playerController.UpdateAngle(currentAngle);
            segments = _trajectoryLine.SegmentCount;
        }

        return currentAngle;
    }

    float CalcAngle(float distance, float gravity, float velocity)
    {
        velocity *= 0.25f;
        float angle = 0.5f * Mathf.Asin((gravity * distance) / (velocity * velocity));
        return angle * Mathf.Rad2Deg;
    }

    float CalcVelocity(float distance, float gravity, float angle)
    {
        float velocity = Mathf.Sqrt((gravity * distance) / Mathf.Sin(2 * angle * Mathf.Deg2Rad));
        return velocity;
    }*/
}
