using UnityEngine;

public class Powerup_TrippleBombVariablePower : Powerup
{
    public override void UsePowerup()
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetProjectileBurst(3);
        UIManager.Instance.EnableDisableButton(_powerupButton, false);

        base.UsePowerup();
    }
}
