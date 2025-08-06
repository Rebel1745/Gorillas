using UnityEngine;

public class Powerup_TripleBomb : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        GameObject scatterBomb = PlayerManager.Instance.GetPlayerPowerup(PlayerManager.Instance.CurrentPlayerId, "Powerup_TripleBombVariablePower");

        if (_powerupEnabled)
        {
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetProjectileBurst(3);
            _powerupButton.image.color = _inUseColour;
            if (scatterBomb) scatterBomb.GetComponent<Powerup>().EnableDisableButton(false);
        }
        else
        {
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetProjectileBurst(1);
            _powerupButton.image.color = _defaultColour;
            if (scatterBomb) scatterBomb.GetComponent<Powerup>().EnableDisableButton(true);
        }

    }
}
