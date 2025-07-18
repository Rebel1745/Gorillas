using UnityEngine;

public class Powerup_MovePlayer : Powerup
{
    public override void UsePowerup()
    {
        GameManager.Instance.UpdateGameState(GameState.WaitingForMovement);
        UIManager.Instance.EnableDisableButton(_powerupButton, false);

        base.UsePowerup();
    }
}
