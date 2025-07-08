using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private PlayerInput _inputActions;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        _inputActions = new PlayerInput();
        _inputActions.Gameplay.Enable();

        _inputActions.Gameplay.ShowTrajectoryLine.started += ShowTrajectoryLine;

        _inputActions.Gameplay.Power.started += UpdatePower;
        _inputActions.Gameplay.Power.canceled += UpdatePower;

        _inputActions.Gameplay.Angle.started += UpdateAngle;
        _inputActions.Gameplay.Angle.canceled += UpdateAngle;

        _inputActions.Gameplay.LaunchProjectile.started += LaunchProjectile;

        _inputActions.Gameplay.RebuildLevel.started += RebuildLevel;
    }

    private void OnDisable()
    {
        _inputActions.Gameplay.Disable();

        _inputActions.Gameplay.ShowTrajectoryLine.started -= ShowTrajectoryLine;

        _inputActions.Gameplay.Power.started -= UpdatePower;
        _inputActions.Gameplay.Power.canceled -= UpdatePower;

        _inputActions.Gameplay.Angle.started -= UpdateAngle;
        _inputActions.Gameplay.Angle.canceled -= UpdateAngle;

        _inputActions.Gameplay.LaunchProjectile.started -= LaunchProjectile;

        _inputActions.Gameplay.RebuildLevel.started -= RebuildLevel;
    }

    private void ShowTrajectoryLine(InputAction.CallbackContext context)
    {
        /*if (GameManager.Instance.State == GameState.WaitingForLaunch && !PlayerManager.Instance.IsCurrentPlayerCPU)
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.ShowTrajectoryLine();*/
    }

    private void UpdatePower(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.State != GameState.WaitingForLaunch || PlayerManager.Instance.IsCurrentPlayerCPU) return;

        if (context.canceled)
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StopPowerChange();
        else
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StartPowerChange(context.ReadValue<float>());
    }

    private void UpdateAngle(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.State != GameState.WaitingForLaunch || PlayerManager.Instance.IsCurrentPlayerCPU) return;

        if (context.canceled)
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StopAngleChange();
        else
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StartAngleChange(context.ReadValue<float>());
    }

    private void LaunchProjectile(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.State != GameState.WaitingForLaunch || PlayerManager.Instance.IsCurrentPlayerCPU) return;

        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.LaunchProjectile();
    }

    private void RebuildLevel(InputAction.CallbackContext context)
    {
        //GameManager.Instance.UpdateGameState(GameState.InitialiseGame);
    }

    public void EnableDisableControls(bool enabled)
    {
        if (enabled)
            _inputActions.Gameplay.Enable();
        else
            _inputActions.Gameplay.Disable();
    }
}
