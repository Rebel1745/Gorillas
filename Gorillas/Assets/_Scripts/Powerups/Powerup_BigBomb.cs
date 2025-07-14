using UnityEngine;

public class Powerup_BigBomb : Powerup
{
    public override void UsePowerup()
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetBigBomb();
        UIManager.Instance.EnableDisableButton(_powerupButton, false);

        base.UsePowerup();
    }
}
