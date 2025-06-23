using UnityEngine;

public class BananaTrajectorySolver
{
    private float _gravity = 9.8f;
    private float _targetDistanceX = 10f; // Horizontal distance
    private float _targetDistanceY = 2f;  // Vertical distance
    private float _minPower = 1f;
    private float _maxPower = 100f;
    private float _minAngle = 0f;
    private float _maxAngle = 90f;
    private float _tolerance = 0.01f;     // Acceptable error

    public void InitialiseValues(float targetDistanceX, float targetDistanceY)
    {
        _gravity = -Physics2D.gravity.y;
        _targetDistanceX = targetDistanceX;
        _targetDistanceY = targetDistanceY;
    }

    public float CalculatePower(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        float power = FindPowerUsingBinarySearch(angleRad) * 4;
        Debug.Log($"Required power: {power:F2} at angle: " + angle + " degrees");
        return power;
    }

    public float CalculateAngle(float power)
    {
        //power *= 0.25f;
        float angle = FindAngleUsingBinarySearch(power);
        Debug.Log($"Required angle: {angle:F2} degrees at power: {power}");
        return angle;
    }

    private float FindPowerUsingBinarySearch(float angleRad)
    {
        float low = _minPower;
        float high = _maxPower;
        float bestPower = 0f;

        for (int i = 0; i < 100; i++)
        {
            float mid = (low + high) / 2f;
            float time = _targetDistanceX / (mid * Mathf.Cos(angleRad));
            float calculatedY = mid * Mathf.Sin(angleRad) * time - 0.5f * _gravity * time * time;

            if (Mathf.Abs(calculatedY - _targetDistanceY) < _tolerance)
            {
                bestPower = mid;
                break;
            }
            else if (calculatedY < _targetDistanceY)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
        }

        return bestPower;
    }

    private float FindAngleUsingBinarySearch(float power)
    {
        float lowAngle = _minAngle * Mathf.Deg2Rad;
        float highAngle = _maxAngle * Mathf.Deg2Rad;
        float bestAngle = 0f;

        for (int i = 0; i < 100; i++)
        {
            float midAngle = (lowAngle + highAngle) / 2f;
            float time = _targetDistanceX / (power * Mathf.Cos(midAngle));
            float calculatedY = power * Mathf.Sin(midAngle) * time - 0.5f * _gravity * time * time;

            if (Mathf.Abs(calculatedY - _targetDistanceY) < _tolerance)
            {
                bestAngle = midAngle;
                break;
            }
            else if (calculatedY < _targetDistanceY)
            {
                lowAngle = midAngle;
            }
            else
            {
                highAngle = midAngle;
            }
        }

        return Mathf.Rad2Deg * bestAngle;
    }
}