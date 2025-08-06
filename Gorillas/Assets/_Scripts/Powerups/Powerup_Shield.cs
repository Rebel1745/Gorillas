using UnityEngine;

public class Powerup_Shield : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.ShowHideShield(_powerupEnabled);

        if (_powerupEnabled) _powerupButton.image.color = _inUseColour;
        else _powerupButton.image.color = _defaultColour;
    }
}
