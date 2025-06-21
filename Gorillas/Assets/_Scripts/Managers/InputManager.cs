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
    }

    private void ShowTrajectoryLine(InputAction.CallbackContext context)
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.ShowTrajectoryLine();
    }

    private void UpdatePower(InputAction.CallbackContext context)
    {
        if (context.canceled)
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StopPowerChange();
        else
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StartPowerChange(context.ReadValue<float>());
    }

    private void UpdateAngle(InputAction.CallbackContext context)
    {
        if (context.canceled)
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StopAngleChange();
        else
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StartAngleChange(context.ReadValue<float>());
    }

    private void LaunchProjectile(InputAction.CallbackContext context)
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.LaunchProjectile();
    }

    public void EnableDisableControls(bool enabled)
    {
        if (enabled)
            _inputActions.Gameplay.Enable();
        else
            _inputActions.Gameplay.Disable();
    }
}
