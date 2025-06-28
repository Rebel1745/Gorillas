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
    private float _currentMinAngle;
    private float _maxAngle = 89f;
    private Slider _angleSlider;
    private int _playerId;
    private int _otherPlayerId;
    private Vector3 _throwingPlayer;
    private Vector3 _targetPlayer;
    [SerializeField] private int _maxIterations = 20;
    private readonly float[] _increments = { 0.1f, 1f, 5f };

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
        {
            Initialise(pc);

            _currentMinAngle = CalculateMinAngle();
            Vector2 angleAndPower = CalculateTrajectory();

            if (angleAndPower == Vector2.zero)
            {
                // we didn't find a proper result, so try something a little different
                angleAndPower = GetMinimumGroundHitsAngleAndPower();
                _playerController.UpdateAngleAndPower(angleAndPower.x, angleAndPower.y);
            }
        }

        yield return new WaitForSeconds(1f);

        //pc.UpdateAngleAndPower(angleAndPower.x, angleAndPower.y);
        pc.LaunchProjectile();
    }

    private Vector2 GetMinimumGroundHitsAngleAndPower()
    {
        _currentMinAngle = 20;
        float bestAngle = 89.9f, bestPower = 100f;
        int groundHits;
        int currentPowerIncrementIndex = 2;
        float currentPowerIncrement = _increments[currentPowerIncrementIndex];
        int iterations = 0, maxIterations = 100;

        while (iterations <= maxIterations)
        {
            //Debug.Log($"Checking {bestPower} power and {bestAngle} angle");
            //Debug.Log($"GetMinimumGroundHitsAngleAndPower Pre Angle: {bestAngle}  Power: {bestPower} Hit - {_trajectoryLine.LastSegment.x} Target - {_targetPlayer.x} Segment Count {_trajectoryLine.SegmentCount}");
            // if we hit the player with the current values, see if we hit fewer buildings than last time
            groundHits = CheckAngleAndPowerForHit(bestAngle, bestPower, true);
            //Debug.Log($"GetMinimumGroundHitsAngleAndPower Angle: {bestAngle}  Power: {bestPower} Hit - {_trajectoryLine.LastSegment.x} Target - {_targetPlayer.x} Segment Count {_trajectoryLine.SegmentCount}");

            if (groundHits > 0)
            {
                // we hit the target, bail and celebrate
                //Debug.Log($"WOOHOOOOOOOOOOO - Angle: {bestAngle}  Power: {bestPower}  Hits: {groundHits}");
                break;
            }
            // we didn't hit the player, if we missed short, even at highest power, decrease the angle until we either hit the player
            // or we go long and have to decrease the power.

            // if we are player 1, we have undershot if the x position of the banana impact is less than the targets' position
            if ((_playerId == 0 && _trajectoryLine.LastSegment.x < _targetPlayer.x) || (_playerId == 1 && _trajectoryLine.LastSegment.x > _targetPlayer.x))
            {
                // if the power is maximum, decrease the angle and go again
                if (bestPower == _maxPower && bestAngle > 45f)
                {
                    //Debug.Log($"We were short, decrease the angle from {bestAngle} to {bestAngle - _increments[2]} with power {bestPower}");
                    bestAngle -= _increments[2];
                }
                // if current power is not at max, increase it
                else if (bestPower != _maxPower)
                {
                    //Debug.Log($"We were short, increase the power from {bestPower} to {Mathf.Clamp(bestPower + _increments[1], 0, _maxPower)} with angle {bestAngle}");
                    bestPower = Mathf.Clamp(bestPower + _increments[1], 0, _maxPower);
                }
                //else Debug.LogError("Power at maximum, and angle below 45, what is going on?");
            }

            // if we go long, decrease the power
            if ((_playerId == 0 && _trajectoryLine.LastSegment.x > _targetPlayer.x) || (_playerId == 1 && _trajectoryLine.LastSegment.x < _targetPlayer.x))
            {
                //Debug.Log($"We overshot, changing power from {bestPower} to {bestPower - _increments[0]} with angle {bestAngle}");
                bestPower -= _increments[0];
            }
            iterations++;
        }

        if (iterations == maxIterations) Debug.LogWarning($"We hit the max, why? Power {bestPower} and angle {bestAngle}");

        return new Vector2(bestAngle, bestPower);
    }

    private float CalculateMinAngle()
    {
        float currentAngle = 0;

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
        if (_currentMinAngle < 45f) _currentMinAngle = 45f;
        int iterations = 0, totalIterations = 0;
        float bestAngle = _currentMinAngle, bestPower = _minPower;
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
                //Debug.Log($"CalculateTrajectory () Angle: {bestAngle}  Power: {bestPower} Hit - {_trajectoryLine.LastSegment.x} Target - {_targetPlayer.x} Segment Count {_trajectoryLine.SegmentCount}");

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
                //Debug.Log("Tried all angles and powers, still nothing.  Checking for building count.");
                //Debug.Log($"{_trajectoryLine.LastSegment.x} Target - {_targetPlayer.x} Segment Count {_trajectoryLine.SegmentCount}");
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
        //Debug.Log($"CheckAngleAndPowerForHit1 () Angle: {angle}  Power: {power} Hit - {_trajectoryLine.LastSegment.x} Target - {_targetPlayer.x} Segment Count {_trajectoryLine.SegmentCount}");
        return _trajectoryLine.HitPlayer;
    }

    private int CheckAngleAndPowerForHit(float angle, float power, bool ignoreGroundHits)
    {
        _playerController.UpdateAngleAndPower(angle, power, ignoreGroundHits);
        //Debug.Log($"CheckAngleAndPowerForHit2 () Angle: {angle}  Power: {power} Hit - {_trajectoryLine.LastSegment.x} Target - {_targetPlayer.x} Segment Count {_trajectoryLine.SegmentCount}");
        if (_trajectoryLine.HitPlayer)
            return _trajectoryLine.GroundHitCount;
        else return -1;
    }
}
