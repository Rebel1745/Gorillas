using UnityEngine;
using UnityEngine.UI;

public class Powerup : MonoBehaviour
{
    protected int _remainingUses = 1;
    [SerializeField] protected Button _powerupButton;

    public virtual void UsePowerup()
    {
        string powerupName = transform.name + "(Clone)";
        _remainingUses--;

        _powerupButton.enabled = false;

        if (_remainingUses == 0)
        {
            PlayerManager.Instance.RemovePlayerPowerup(gameObject);
            Destroy(gameObject);
        }
    }

    public void AddPowerupUse()
    {
        _remainingUses++;
    }
}
