using UnityEngine;

public class BananaTrajectorySolver
{
    private float _gravity = 9.8f;
    private float _targetDistanceX = 10f; // Horizontal distance
    private float _targetDistanceY = 2f;  // Vertical distance
    private float _minPower = 5f;
    private float _maxPower = 20f;
    private float _minAngle = 43f;
    private float _maxAngle = 46f;
    private float _tolerance = 0.01f;     // Acceptable error

    public void InitialiseValues(float targetDistanceX, float targetDistanceY, float minAngle)
    {
        _gravity = -Physics2D.gravity.y;
        _targetDistanceX = targetDistanceX;
        _targetDistanceY = targetDistanceY;
        _minAngle = minAngle;
    }

    public Vector2 CalculateLaunchValues()
    {
        float bestPower = 0f;
        float bestAngle = 0f;
        float epsilon = 0.1f; // Precision threshold

        // Binary search for angle
        bestAngle = BinarySearchAngle(_minAngle, _maxAngle, epsilon);
        Debug.Log(bestAngle);
        // Calculate corresponding power for the found angle
        bestPower = CalculatePower(_targetDistanceX, _targetDistanceY, _gravity, bestAngle);

        return new Vector2(bestPower, bestAngle * Mathf.Rad2Deg);
    }

    private float BinarySearchAngle(float low, float high, float epsilon)
    {
        float angle = 0f;
        while (high - low > epsilon)
        {
            float mid = (low + high) / 2;
            float power = CalculatePower(_targetDistanceX, _targetDistanceY, _gravity, mid);
            if (power <= _maxPower)
            {
                angle = mid;
                high = mid;
            }
            else
            {
                low = mid;
            }
        }
        return angle;
    }

    private float CalculatePower(float distance, float heightDifference, float gravity, float angle)
    {
        float angleRad = Mathf.Deg2Rad * angle;
        float sin2A = Mathf.Sin(2 * angleRad);
        if (sin2A == 0f) return float.PositiveInfinity;

        float power = Mathf.Sqrt((distance * gravity) / sin2A);
        float verticalComponent = power * Mathf.Sin(angleRad);
        float time = distance / (power * Mathf.Cos(angleRad));
        float calculatedHeight = verticalComponent * time - 0.5f * gravity * time * time;

        // Adjust power to hit the target height
        float heightError = calculatedHeight - heightDifference;
        float correction = heightError / (gravity * time);
        power += correction;

        return power;
    }
}