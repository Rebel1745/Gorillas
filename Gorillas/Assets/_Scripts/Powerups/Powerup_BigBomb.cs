using UnityEngine;

public class Powerup_BigBomb : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetBigBomb(_powerupEnabled);
        GameObject scatterBomb = PlayerManager.Instance.GetPlayerPowerup(PlayerManager.Instance.CurrentPlayerId, "Powerup_TripleBombVariablePower");

        if (_powerupEnabled)
        {
            _powerupButton.image.color = _inUseColour;
            if (scatterBomb) scatterBomb.GetComponent<Powerup>().EnableDisableButton(false);
        }
        else
        {
            _powerupButton.image.color = _defaultColour;
            if (scatterBomb) scatterBomb.GetComponent<Powerup>().EnableDisableButton(true);
        }
    }
}
