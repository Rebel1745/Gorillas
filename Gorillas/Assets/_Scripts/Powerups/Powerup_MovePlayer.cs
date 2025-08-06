using UnityEngine;
using UnityEngine.UI;

public class Powerup_MovePlayer : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        if (_powerupEnabled)
        {
            _powerupButton.image.color = _inUseColour;
            GameManager.Instance.UpdateGameState(GameState.WaitingForMovement);
        }
        else
        {
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.CancelMovementPowerupPosition();
            InputManager.Instance.EnableDisableGameplayControls(true);
            _powerupButton.image.color = _defaultColour;
        }
    }
}
