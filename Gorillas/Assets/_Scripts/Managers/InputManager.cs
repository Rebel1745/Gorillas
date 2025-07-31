using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private PlayerInput _inputActions;
    private Button _currentPowerupButton;

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

        _inputActions.BuildingMovement.Direction.started += StartLevelElementMovement;
        _inputActions.BuildingMovement.Direction.canceled += StopLevelElementMovement;
        _inputActions.BuildingMovement.Confirm.started += BuildingMovementConfirm;
        _inputActions.BuildingMovement.Cancel.started += BuildingMovementCancel;
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

        _inputActions.BuildingMovement.Direction.started -= StartLevelElementMovement;
        _inputActions.BuildingMovement.Direction.canceled -= StopLevelElementMovement;
        _inputActions.BuildingMovement.Confirm.started -= BuildingMovementConfirm;
        _inputActions.BuildingMovement.Cancel.started -= BuildingMovementCancel;
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
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.MovePlayerMovementSpriteWithInput(context.ReadValue<float>());
    }

    private void MovementPowerupCancel(InputAction.CallbackContext context)
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.CancelMovementPowerupPosition();
        EnableDisableGameplayControls(true);
    }

    private void BuildingMovementConfirm(InputAction.CallbackContext context)
    {
        //PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.ConfirmBuildingMovementPosition();
    }

    private void StartLevelElementMovement(InputAction.CallbackContext context)
    {
        LevelManager.Instance.StartLevelElementMovement(context.ReadValue<float>());
    }

    private void StopLevelElementMovement(InputAction.CallbackContext context)
    {
        LevelManager.Instance.StopLevelElementMovement();
    }

    private void BuildingMovementCancel(InputAction.CallbackContext context)
    {
        LevelManager.Instance.EnableDisableBuildingMovementColliders(false);
        EnableDisableCurrentPowerupButton(true);
        EnableDisableGameplayControls(true);
    }

    public void SetCurrentPowerupButton(Button button)
    {
        _currentPowerupButton = button;
    }

    public void EnableDisableCurrentPowerupButton(bool enable)
    {
        UIManager.Instance.EnableDisableButton(_currentPowerupButton, enable);
    }

    public void EnableDisableUIControls(bool enabled)
    {
        if (enabled)
        {
            _inputActions.Gameplay.Disable();
            _inputActions.MovementPowerup.Disable();
            _inputActions.BuildingMovement.Disable();
            _inputActions.UI.Enable();
        }
        else
            _inputActions.UI.Disable();
    }

    public void EnableDisableGameplayControls(bool enabled)
    {
        if (enabled)
        {
            _inputActions.UI.Disable();
            _inputActions.MovementPowerup.Disable();
            _inputActions.BuildingMovement.Disable();
            _inputActions.Gameplay.Enable();
        }
        else
            _inputActions.Gameplay.Disable();
    }

    public void EnableDisableMovementPowerupControls(bool enabled)
    {
        if (enabled)
        {
            _inputActions.UI.Disable();
            _inputActions.Gameplay.Disable();
            _inputActions.BuildingMovement.Disable();
            _inputActions.MovementPowerup.Enable();
        }
        else
            _inputActions.MovementPowerup.Disable();
    }

    public void EnableDisableBuildingMovementControls(bool enabled)
    {
        if (enabled)
        {
            _inputActions.UI.Disable();
            _inputActions.Gameplay.Disable();
            _inputActions.MovementPowerup.Disable();
            _inputActions.BuildingMovement.Enable();
        }
        else
            _inputActions.BuildingMovement.Disable();
    }
}
