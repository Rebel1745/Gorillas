using UnityEngine;

public class Powerup_MoveBuilding : Powerup
{
    public override void UsePowerup()
    {
        GameManager.Instance.UpdateGameState(GameState.WaitingForBuildingMovement);
        LevelManager.Instance.EnableDisableBuildingMovementColliders(true);
        UIManager.Instance.EnableDisableButton(_powerupButton, false);

        base.UsePowerup();
    }
}
