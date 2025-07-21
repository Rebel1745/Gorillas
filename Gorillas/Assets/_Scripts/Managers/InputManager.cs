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

        _inputActions.Gameplay.Power.started += UpdatePower;
        _inputActions.Gameplay.Power.canceled += UpdatePower;

        _inputActions.Gameplay.Angle.started += UpdateAngle;
        _inputActions.Gameplay.Angle.canceled += UpdateAngle;

        _inputActions.Gameplay.LaunchProjectile.started += LaunchProjectile;

        _inputActions.MovementPowerup.Direction.started += MovementPowerupDirection;
        _inputActions.MovementPowerup.Confirm.started += MovementPowerupConfirm;
        _inputActions.MovementPowerup.Cancel.started += MovementPowerupCancel;
    }

    private void OnDisable()
    {
        _inputActions.Gameplay.Power.started -= UpdatePower;
        _inputActions.Gameplay.Power.canceled -= UpdatePower;

        _inputActions.Gameplay.Angle.started -= UpdateAngle;
        _inputActions.Gameplay.Angle.canceled -= UpdateAngle;

        _inputActions.Gameplay.LaunchProjectile.started -= LaunchProjectile;

        _inputActions.MovementPowerup.Direction.started -= MovementPowerupDirection;
        _inputActions.MovementPowerup.Confirm.started -= MovementPowerupConfirm;
        _inputActions.MovementPowerup.Cancel.started -= MovementPowerupCancel;
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

        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StartLaunchProjectile();
    }

    private void MovementPowerupConfirm(InputAction.CallbackContext context)
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.ConfirmMovementPowerupPosition();
    }

    private void MovementPowerupDirection(InputAction.CallbackContext context)
    {
        Debug.Log("Direction");
    }

    private void MovementPowerupCancel(InputAction.CallbackContext context)
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.CancelMovementPowerupPosition();
    }

    public void EnableDisableUIControls(bool enabled)
    {
        if (enabled)
            _inputActions.UI.Enable();
        else
            _inputActions.UI.Disable();
    }

    public void EnableDisableGameplayControls(bool enabled)
    {
        if (enabled)
            _inputActions.Gameplay.Enable();
        else
            _inputActions.Gameplay.Disable();
    }

    public void EnableDisableMovementPowerupControls(bool enabled)
    {
        if (enabled)
            _inputActions.MovementPowerup.Enable();
        else
            _inputActions.MovementPowerup.Disable();
    }
}
