using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using System;

public class AIController : MonoBehaviour
{
    private float _minPower = 50f;
    private float _maxPower = 100f;
    private float _minAngle = 20f;
    private float _maxAngle = 89f;
    private Slider _angleSlider;
    private int _playerId;
    private int _otherPlayerId;
    private Vector3 _throwingPlayer;
    private Vector3 _targetPlayer;
    private int _maxIterations = 20;
    private float[] _increments = { 0.1f, 1f, 5f };

    [SerializeField] private TrajectoryLine _trajectoryLine;
    private PlayerController _playerController;

    private void Initialise(PlayerController pc)
    {
        _playerController = pc;
        _angleSlider = pc.AngleSlider;
        _playerId = pc.PlayerId;
        _otherPlayerId = (_playerId + 1) % 2;

        _throwingPlayer = PlayerManager.Instance.Players[_playerId].PlayerGameObject.transform.position;
        _targetPlayer = PlayerManager.Instance.Players[_otherPlayerId].PlayerGameObject.transform.position;
    }

    public IEnumerator DoAI(PlayerController pc)
    {
        if (pc.ThrowNumber == 0)
            Initialise(pc);
        _minAngle = CalculateMinAngle();
        Vector2 angleAndPower = CalculateTrajectory();

        yield return new WaitForSeconds(1f);

        //pc.UpdateAngleAndPower(angleAndPower.x, angleAndPower.y);
        pc.LaunchProjectile();
    }

    private float CalculateMinAngle()
    {
        float minAngle = _angleSlider.minValue;
        float maxAngle = _angleSlider.maxValue;
        float currentAngle = minAngle;

        int segments = _trajectoryLine.SegmentCount;

        while (segments < 15)
        {
            currentAngle += 0.5f;
            _playerController.UpdateAngle(currentAngle);
            segments = _trajectoryLine.SegmentCount;
        }

        return currentAngle;
    }

    private Vector2 CalculateTrajectory()
    {
        // start at 45 degrees if we can
        if (_minAngle < 45f) _minAngle = 45f;
        int iterations = 0, totalIterations = 0;
        float bestAngle = _minAngle, bestPower = _minPower;
        bool checkComplete = false;
        int currentPowerIncrementIndex = 2;
        float currentPowerIncrement = _increments[currentPowerIncrementIndex]; // always start wiht the large increment

        while (!checkComplete)
        {
            while (iterations <= _maxIterations)
            {
                //Debug.Log(bestPower + " " + bestAngle);
                // if we hit the player with the current values, we are done, bail
                if (CheckAngleAndPowerForHit(bestAngle, bestPower))
                {
                    checkComplete = true;
                    break;
                }

                // if we missed, find out if we missed long, or short
                if (_playerId == 0)
                {
                    // if we are player 1, we have overshot if the x position of the banana impact is greate than the targets' position
                    if (_trajectoryLine.LastSegment.x > _targetPlayer.x)
                    {
                        // make sure we can decrease the increment
                        if (currentPowerIncrementIndex > 0)
                        {
                            // if we have overshot, decrease the power by the current increment and decrease the current increment
                            bestPower -= currentPowerIncrement;
                            currentPowerIncrementIndex--;
                            currentPowerIncrement = _increments[currentPowerIncrementIndex];
                        }
                    }
                }
                else
                {
                    // if we are player 2, we have overshot if the x position of the banana impact is less than the targets' position
                    if (_trajectoryLine.LastSegment.x < _targetPlayer.x)
                    {
                        //Debug.Log(_trajectoryLine.LastSegment.x + " - " + _targetPlayer.x);
                        // make sure we can decrease the increment
                        if (currentPowerIncrementIndex > 0)
                        {
                            // if we have overshot, decrease the power by the current increment and decrease the current increment
                            //Debug.Log("Decrease best power from " + bestPower + " to " + (bestPower - currentPowerIncrement));
                            bestPower -= currentPowerIncrement;
                            currentPowerIncrementIndex--;
                            currentPowerIncrement = _increments[currentPowerIncrementIndex];
                        }
                    }
                }

                // if we have hit the max power and still cant hit the player, bail and up the angle
                if (bestPower >= _maxPower)
                {
                    bestPower = _minPower;
                    break;
                }

                //Debug.Log("Increase best power from " + bestPower + " to " + (bestPower + currentPowerIncrement));
                // if it isnt, up the power
                bestPower += currentPowerIncrement;
                iterations++;
                totalIterations++;
            }

            // if we have hit the max angle and still cant hit the player, bail and throw up and error, i will write something to handle it
            // probably a different check function that doesnt stop when it hits a building, but tries to hit the minimum number of buildings
            if (bestAngle >= _maxAngle)
            {
                Debug.LogError("Tried all angles and powers, still nothing");
                break;
            }
            // if we still dont hit the player with that angle and any power, up the angle
            bestAngle += _increments[0];
            currentPowerIncrementIndex = 2;
            currentPowerIncrement = _increments[currentPowerIncrementIndex];
            bestPower = _minPower;
            iterations = 0;
            totalIterations++;

            //Debug.Log(totalIterations);
        }

        return new Vector2(bestPower, bestAngle);
    }

    /*private Vector2 CalculateTrajectory()
    {
        Debug.Log(_minAngle);
        // start at 45 degrees if we can
        if (_minAngle < 45f) _minAngle = 45f;
        int iterations = 0, totalIterations = 0;
        float bestAngle = _minAngle, bestPower = _minPower;
        bool checkComplete = false;
        float currentIncreament = LARGE_INCREMENT;

        while (!checkComplete)
        {
            while (iterations <= _maxIterations)
            {
                //Debug.Log($"Check angle: {bestAngle} - Check power: {bestPower}");
                // if we hit the player with the current values, we are done, bail
                if (CheckAngleAndPowerForHit(bestAngle, bestPower))
                {
                    checkComplete = true;
                    break;
                }

                // if we have hit the max power and still cant hit the player, bail and up the angle
                if (bestPower >= _maxPower)
                {
                    bestPower = _minPower;
                    break;
                }

                // if it isnt, up the power
                bestPower += currentIncreament;
                iterations++;
                totalIterations++;
            }

            // if we have hit the max angle and still cant hit the player, bail and throw up and error, i will write something to handle it
            // probably a different check function that doesnt stop when it hits a building, but tries to hit the minimum number of buildings
            if (bestAngle >= _maxAngle)
            {
                Debug.LogError("Tried all angles and powers, still nothing");
                break;
            }
            // if we still dont hit the player with that angle and any power, up the angle
            bestAngle += currentIncreament;
            iterations = 0;
            totalIterations++;

            Debug.Log(totalIterations);
        }

        return new Vector2(bestPower, bestAngle);
    }*/

    private bool CheckAngleAndPowerForHit(float angle, float power)
    {
        _playerController.UpdateAngleAndPower(angle, power);
        return _trajectoryLine.HitPlayer;
    }
}
