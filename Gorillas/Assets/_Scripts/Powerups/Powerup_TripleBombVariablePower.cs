using UnityEngine;

public class Powerup_TripleBombVariablePower : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        GameObject tripleBomb = PlayerManager.Instance.GetPlayerPowerup(PlayerManager.Instance.CurrentPlayerId, "Powerup_TripleBomb");

        if (_powerupEnabled)
        {
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetProjectileBurst(3);
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetVariablePower();
            _powerupButton.image.color = _inUseColour;
            if (tripleBomb) tripleBomb.GetComponent<Powerup>().EnableDisableButton(false);
        }
        else
        {
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetProjectileBurst(1);
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.ResetVariablePower();
            _powerupButton.image.color = _defaultColour;
            if (tripleBomb) tripleBomb.GetComponent<Powerup>().EnableDisableButton(true);
        }
    }
}
