using UnityEngine;

public class Powerup_TripleBombVariablePower : Powerup
{
    public override void UsePowerup()
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetProjectileBurst(3);
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetVariablePower();
        UIManager.Instance.EnableDisableButton(_powerupButton, false);

        base.UsePowerup();
    }
}
