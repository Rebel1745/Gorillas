using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private float _minPower = 50f;
    private float _maxPower = 100f;
    private float _currentMinAngle;
    private float _maxAngle = 89f;
    private int _playerId;
    private int _otherPlayerId;
    private Vector3 _throwingPlayer;
    private Vector3 _targetPlayer;
    [SerializeField] private int _maxIterations = 20;
    private readonly float[] _increments = { 0.1f, 1f, 5f };
    [SerializeField] private TrajectoryLine _trajectoryLine;
    [SerializeField] private PlayerController _playerController;
    private float _cpuVariability;
    private float _perfectAngle;
    private float _perfectPower;
    private bool _forceRecheck;

    // Powerups
    private bool _powerupUsed = false;
    private GameObject _bigBombPowerup;
    private GameObject _movePlayerPowerup;
    private GameObject _shieldPowerup;
    private GameObject _trajectoryLinePowerup;
    private GameObject _tripleBombPowerup;
    private GameObject _scatterBombPowerup;

    private void Initialise()
    {
        _playerId = _playerController.PlayerId;
        _otherPlayerId = (_playerId + 1) % 2;
        _forceRecheck = false;

        _throwingPlayer = PlayerManager.Instance.Players[_playerId].PlayerGameObject.transform.position;
        _targetPlayer = PlayerManager.Instance.Players[_otherPlayerId].PlayerGameObject.transform.position;

        _cpuVariability = GetCPUVariability();
    }

    private float GetCPUVariability()
    {
        float variability = 0f;

        switch (_playerController.CPUType)
        {
            case CPU_TYPE.Easy:
                variability = 7.5f;
                break;
            case CPU_TYPE.Medium:
                variability = 5f;
                break;
            case CPU_TYPE.Hard:
                variability = 2f;
                break;
            case CPU_TYPE.Impossible:
                variability = 0.5f;
                break;
        }

        return variability;
    }

    public IEnumerator DoAI()
    {
        CheckPlayerPowerups();
        yield return new WaitForSeconds(1f);

        if (_playerController.ThrowNumber == 0 || _forceRecheck)
        {
            Initialise();

            // do the initial calculation
            _playerController.RecalculateTrajectoryLine();

            if (_trajectoryLine.SegmentCount < 15 && _movePlayerPowerup)
            {
                // we are right next to a building, let's move
                // enable the powerup
                _movePlayerPowerup.GetComponent<Powerup>().UsePowerup();
                yield return new WaitForSeconds(1f);
                // find the highest point
                int highestIndex = -1;
                float highestYPos = Mathf.NegativeInfinity;
                foreach (int i in LevelManager.Instance.CPUActiveSpawnPointIndexList)
                {
                    if (LevelManager.Instance.GetSpawnPointAtIndex(i).y > highestYPos)
                    {
                        highestYPos = LevelManager.Instance.GetSpawnPointAtIndex(i).y;
                        highestIndex = i;
                    }
                }

                // move to the highest point
                if (highestIndex != Mathf.NegativeInfinity)
                {
                    // find out how far to move
                    int indexDifference = highestIndex - PlayerManager.Instance.Players[_playerId].SpawnPointIndex;
                    for (int i = 0; i < Mathf.Abs(indexDifference); i++)
                    {
                        // incrementally move
                        _playerController.MovePlayerMovementSpriteWithInput(Mathf.Sign(indexDifference));
                        yield return new WaitForSeconds(0.75f);
                    }
                    // confim the position we have moved to
                    _playerController.ConfirmMovementPowerupPosition();
                }
                // or bail out
                else
                    _movePlayerPowerup.GetComponent<Powerup>().UsePowerup();
            }

            _currentMinAngle = CalculateMinAngle();
            Vector2 angleAndPower = CalculateTrajectory();

            if (angleAndPower == Vector2.zero)
            {
                // we didn't find a proper result, so try something a little different
                angleAndPower = GetMinimumGroundHitsAngleAndPower();
                _forceRecheck = true;

                // if we are in this situation it means we have a building in the way, see if we have a triple bomb to use
                if (_tripleBombPowerup)
                {
                    _tripleBombPowerup.GetComponent<Powerup>().UsePowerup();
                    _powerupUsed = true;
                }
            }

            _perfectAngle = angleAndPower.x;
            _perfectPower = angleAndPower.y;
        }

        float randomAngle = Random.Range(_perfectAngle - _cpuVariability, _perfectAngle + _cpuVariability);
        float randomPower = Random.Range(_perfectPower - _cpuVariability, _perfectPower + _cpuVariability);

        _playerController.UpdateAngleAndPower(randomAngle, randomPower);

        #region Powerup Checks
        #region Trajectory Line
        // if we have a trajectory line, use it
        if (_trajectoryLinePowerup)
        {
            _trajectoryLinePowerup.GetComponent<Powerup>().UsePowerup();

            // if we hit the player, skip the recalculation
            if (!_trajectoryLine.HitPlayer)
            {
                // the AI now 'knows' where the banana will land, if it falls short, make the minimum power the current power and re-estimate
                if ((_playerId == 0 && _trajectoryLine.LastSegment.x < _targetPlayer.x) || (_playerId == 1 && _trajectoryLine.LastSegment.x > _targetPlayer.x))
                {
                    randomPower = Random.Range(randomPower, _perfectAngle + _cpuVariability);
                }
                // if the trajectory line shows we will go long, make the maximum power the current power and re-estimate
                if ((_playerId == 0 && _trajectoryLine.LastSegment.x > _targetPlayer.x) || (_playerId == 1 && _trajectoryLine.LastSegment.x < _targetPlayer.x))
                {
                    randomPower = Random.Range(_perfectAngle - _cpuVariability, randomPower);
                }
            }
            else
                _powerupUsed = true;

            yield return new WaitForSeconds(1f);

            _playerController.UpdateAngleAndPower(randomAngle, randomPower);
        }
        #endregion
        #region Shield
        if (_shieldPowerup)
        {
            float rand = Random.Range(0f, 1f); //Debug.Log("Shield: " + rand);
            // if we have the powerup use it if we guess right in a 50/50
            if (rand > 0.5f) _shieldPowerup.GetComponent<Powerup>().UsePowerup();
            else if (_playerController.ThrowNumber > 0)
            {
                // if we missed the 50/50, check to see if the last throw from the player is close to us
                // if it is, shield, if not, don't
                float lastLandingPositionX = PlayerManager.Instance.Players[_otherPlayerId].PlayerController.LastProjectileLandingPositionX;

                if (Mathf.Abs(lastLandingPositionX - transform.position.x) < 2f)
                    _shieldPowerup.GetComponent<Powerup>().UsePowerup();
            }
        }
        #endregion
        #region Big Bomb
        if (_bigBombPowerup && !_powerupUsed)
        {
            float rand = Random.Range(0f, 1f); //Debug.Log(" Big Bomb: " + rand);
            // if we have the powerup use it if we guess right in a 50/50
            if (rand > 0.5f)
            {
                _bigBombPowerup.GetComponent<Powerup>().UsePowerup();
                _powerupUsed = true;
            }
            else if (_playerController.ThrowNumber > 0)
            {
                // if we missed the 50/50, check to see if the last throw from the player is close to us
                // if it is, shield, if not, don't
                float lastLandingPositionX = PlayerManager.Instance.Players[_otherPlayerId].PlayerController.LastProjectileLandingPositionX;

                if (Mathf.Abs(lastLandingPositionX - transform.position.x) < 2f)
                {
                    _bigBombPowerup.GetComponent<Powerup>().UsePowerup();
                    _powerupUsed = true;
                }
            }
        }
        #endregion
        #region Scatter Bomb
        if (_scatterBombPowerup && !_powerupUsed)
        {
            float rand = Random.Range(0f, 1f); //Debug.Log("Scatter bomb: " + rand);
            // if we have the powerup use it if we guess right in a 50/50
            if (rand > 0.5f)
            {
                _scatterBombPowerup.GetComponent<Powerup>().UsePowerup();
                _powerupUsed = true;
            }
            else if (_playerController.ThrowNumber > 0)
            {
                // if we missed the 50/50, check to see if the last throw from the player is close to us
                // if it is, shield, if not, don't
                float lastLandingPositionX = PlayerManager.Instance.Players[_otherPlayerId].PlayerController.LastProjectileLandingPositionX;

                if (Mathf.Abs(lastLandingPositionX - transform.position.x) < 2f)
                {
                    _scatterBombPowerup.GetComponent<Powerup>().UsePowerup();
                    _powerupUsed = true;
                }
            }
        }
        #endregion
        #endregion

        yield return new WaitForSeconds(0.75f);

        _playerController.StartLaunchProjectile();
    }

    private void CheckPlayerPowerups()
    {
        _powerupUsed = false;
        _bigBombPowerup = PlayerManager.Instance.GetPlayerPowerup(PlayerManager.Instance.CurrentPlayerId, "Powerup_BigBomb");
        _movePlayerPowerup = PlayerManager.Instance.GetPlayerPowerup(PlayerManager.Instance.CurrentPlayerId, "Powerup_MovePlayer");
        _shieldPowerup = PlayerManager.Instance.GetPlayerPowerup(PlayerManager.Instance.CurrentPlayerId, "Powerup_Shield");
        _trajectoryLinePowerup = PlayerManager.Instance.GetPlayerPowerup(PlayerManager.Instance.CurrentPlayerId, "Powerup_TrajectoryLine");
        _tripleBombPowerup = PlayerManager.Instance.GetPlayerPowerup(PlayerManager.Instance.CurrentPlayerId, "Powerup_TripleBomb");
        _scatterBombPowerup = PlayerManager.Instance.GetPlayerPowerup(PlayerManager.Instance.CurrentPlayerId, "Powerup_TripleBombVariablePower");
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
            // if we hit the player with the current values, see if we hit fewer buildings than last time
            groundHits = CheckAngleAndPowerForHit(bestAngle, bestPower, true);

            if (groundHits > 0)
            {
                // we hit the target, bail and celebrate
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
                    bestAngle -= _increments[2];
                }
                // if current power is not at max, increase it
                else if (bestPower != _maxPower)
                {
                    bestPower = Mathf.Clamp(bestPower + _increments[1], 0, _maxPower);
                }
            }

            // if we go long, decrease the power
            if ((_playerId == 0 && _trajectoryLine.LastSegment.x > _targetPlayer.x) || (_playerId == 1 && _trajectoryLine.LastSegment.x < _targetPlayer.x))
            {
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
                    return new Vector2(bestAngle, bestPower);
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

                // if we have hit the max power and still cant hit the player, bail and up the angle
                if (bestPower >= _maxPower)
                {
                    break;
                }

                // if it isnt, up the power
                bestPower += currentPowerIncrement;
                iterations++;
                totalIterations++;
            }

            // if we have hit the max angle and still cant hit the player, bail and throw up and error, i will write something to handle it
            // probably a different check function that doesnt stop when it hits a building, but tries to hit the minimum number of buildings
            if (bestAngle >= _maxAngle)
            {
                return Vector2.zero;
            }

            // if we still dont hit the player with that angle and any power, up the angle
            bestAngle += _increments[0];
            currentPowerIncrementIndex = 2;
            currentPowerIncrement = _increments[currentPowerIncrementIndex];
            bestPower = _minPower;
            iterations = 0;
            totalIterations++;
        }

        return new Vector2(bestAngle, bestPower);
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

    public void ForceRecalculatePerfectShot()
    {
        _forceRecheck = true;
    }
}
