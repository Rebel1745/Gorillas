using UnityEngine;

public class Powerup_TrippleBomb : Powerup
{
    public override void UsePowerup()
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetProjectileBurst(3);
        UIManager.Instance.EnableDisableButton(_powerupButton, false);

        base.UsePowerup();
    }
}
