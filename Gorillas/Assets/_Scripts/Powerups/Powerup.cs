using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Powerup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected int _remainingUses = 1;
    [SerializeField] protected Button _powerupButton;
    [SerializeField] protected string _powerupTitle;
    [SerializeField] protected string _powerupText;

    public virtual void UsePowerup()
    {
        InputManager.Instance.SetCurrentPowerupButton(_powerupButton);
        string powerupName = transform.name + "(Clone)";
        _remainingUses--;

        _powerupButton.enabled = false;

        if (_remainingUses == 0)
        {
            PlayerManager.Instance.RemovePlayerPowerup(gameObject);
            Destroy(gameObject);
        }
    }

    public void AddPowerupUse()
    {
        _remainingUses++;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.ShowTooltip(_powerupTitle, _powerupText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.HideTooltip();
    }
}
