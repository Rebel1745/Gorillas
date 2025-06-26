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
    [SerializeField] private int _maxIterations = 20;
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

        if (angleAndPower == Vector2.zero)
        {
            // we didn't find a proper result, so try something a little different
            angleAndPower = GetMinimumGroundHitsAngleAndPower();
        }

        yield return new WaitForSeconds(1f);

        //pc.UpdateAngleAndPower(angleAndPower.x, angleAndPower.y);
        pc.LaunchProjectile();
    }

    private Vector2 GetMinimumGroundHitsAngleAndPower()
    {
        _minAngle = 20;
        float bestAngle = _maxAngle, bestPower = _maxPower;
        int bestGroundHitCount = 999;
        float bestGroundCountAngle = 0, bestGroundCountPower = 0;
        int groundHits;
        bool checkComplete = false;
        int currentPowerIncrementIndex = 2;
        float currentPowerIncrement = _increments[currentPowerIncrementIndex];
        int iterations = 0, totalIterations = 0;

        while (!checkComplete)
        {
            while (iterations <= _maxIterations)
            {
                //Debug.Log("Checking " + bestPower + " power and " + bestAngle + " angle");
                // if we hit the player with the current values, see if we hit fewer buildings than last time
                groundHits = CheckAngleAndPowerForHit(bestAngle, bestPower, true);
                if (groundHits > 0)
                {
                    Debug.Log(bestAngle + " - " + bestPower + " - " + groundHits);
                    if (groundHits == bestGroundCountAngle)
                    {
                        // if we hit the same amount of buildings, update the angle and power if the power required is less that before
                        if (bestGroundCountPower > bestPower)
                        {
                            bestGroundCountPower = bestPower;
                            bestGroundCountAngle = bestAngle;
                        }
                    }
                    else if (groundHits < bestGroundHitCount)
                    {
                        // if we hit fewer buildings this time round, we cooking
                        bestGroundHitCount = groundHits;
                        bestGroundCountPower = bestPower;
                        bestGroundCountAngle = bestAngle;
                    }

                    // if we only hit one building, that is a good as we can do, bail
                    if (groundHits == 1)
                    {
                        checkComplete = true;
                        break;
                    }
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
                            bestPower += currentPowerIncrement;
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
                        Debug.Log(_trajectoryLine.LastSegment.x + " - " + _targetPlayer.x);
                        // make sure we can decrease the increment
                        if (currentPowerIncrementIndex > 0)
                        {
                            // if we have overshot, decrease the power by the current increment and decrease the current increment
                            //Debug.Log("Increase best power from " + bestPower + " to " + (bestPower + currentPowerIncrement));
                            bestPower += currentPowerIncrement;
                            currentPowerIncrementIndex--;
                            currentPowerIncrement = _increments[currentPowerIncrementIndex];
                        }
                    }
                }

                // if we have hit the max power and still cant hit the player, bail and up the angle
                if (bestPower <= _minPower)
                {
                    bestPower = _maxPower;
                    break;
                }

                //Debug.Log("Decrease best power from " + bestPower + " to " + (bestPower - currentPowerIncrement));
                // if it isnt, up the power
                bestPower -= currentPowerIncrement;
                iterations++;
                totalIterations++;
            }

            // if we have hit the max angle and still cant hit the player, bail and throw up and error, i will write something to handle it
            // probably a different check function that doesnt stop when it hits a building, but tries to hit the minimum number of buildings
            if (bestAngle < _minAngle)
            {
                Debug.Log("Tried all angles and powers, still nothing.  What is going on?");
                return Vector2.zero;
            }
            //Debug.Log("Decrease best angle from " + bestAngle + " to " + (bestAngle - _increments[1]) + " and start again from " + _maxPower + " power");
            // if we still dont hit the player with that angle and any power, up the angle
            bestAngle -= _increments[1];
            currentPowerIncrementIndex = 2;
            currentPowerIncrement = _increments[currentPowerIncrementIndex];
            bestPower = _maxPower;
            iterations = 0;
            totalIterations++;
        }
        Debug.Log(totalIterations);

        return new Vector2(bestGroundCountPower, bestGroundCountAngle);
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
        float currentPowerIncrement = _increments[currentPowerIncrementIndex]; // always start with the large increment

        while (!checkComplete)
        {
            while (iterations <= _maxIterations)
            {
                //Debug.Log(bestPower + " " + bestAngle);
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
                //if (iterations == _maxIterations) Debug.Log("Hit Max iterations");
            }

            // if we have hit the max angle and still cant hit the player, bail and throw up and error, i will write something to handle it
            // probably a different check function that doesnt stop when it hits a building, but tries to hit the minimum number of buildings
            if (bestAngle >= _maxAngle)
            {
                Debug.Log("Tried all angles and powers, still nothing.  Checking for building count.");
                return Vector2.zero;
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

    private bool CheckAngleAndPowerForHit(float angle, float power)
    {
        _playerController.UpdateAngleAndPower(angle, power);
        return _trajectoryLine.HitPlayer;
    }

    private int CheckAngleAndPowerForHit(float angle, float power, bool ignoreGroundHits)
    {
        _playerController.UpdateAngleAndPower(angle, power, ignoreGroundHits);
        if (_trajectoryLine.HitPlayer)
            return _trajectoryLine.GroundHitCount;
        else return -1;
    }
}
