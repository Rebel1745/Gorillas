using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Powerup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected int _remainingUses = 1;
    [SerializeField] protected Button _powerupButton;
    [SerializeField] protected TMP_Text _powerupNumberText;
    [SerializeField] protected string _powerupTitle;
    [SerializeField] protected string _powerupText;
    [SerializeField] protected Color _defaultColour = new(169, 169, 169);
    [SerializeField] protected Color _inUseColour = new(100, 200, 100);
    [SerializeField] protected Color _usedColour = new(200, 100, 100);
    protected bool _powerupEnabled = false;

    public virtual void UsePowerup()
    {
        _powerupEnabled = !_powerupEnabled;

        InputManager.Instance.SetCurrentPowerupButton(_powerupButton);

        if (_powerupEnabled)
            _remainingUses--;
        else
            _remainingUses++;

        UpdatePowerupNumberText();
    }

    public void EnableDisableButton()
    {
        EnableDisableButton(!_powerupButton.enabled);
    }

    public void EnableDisableButton(bool enabled)
    {
        _powerupButton.enabled = enabled;
        _powerupEnabled = !enabled;
        UpdatePowerupNumberText();

        if (enabled)
        {
            _powerupButton.image.color = _defaultColour;
        }
        else
        {
            _powerupButton.image.color = _usedColour;
        }

        if (_remainingUses == 0 && !_powerupEnabled)
            RemoveButton();
    }

    protected void RemoveButton()
    {
        PlayerManager.Instance.RemovePlayerPowerup(gameObject);
        HideTooltip();
        Destroy(gameObject);
    }

    public void AddPowerupUse()
    {
        _remainingUses++;
        UpdatePowerupNumberText();
    }

    private void UpdatePowerupNumberText()
    {
        if (_remainingUses > 1)
        {
            _powerupNumberText.text = _remainingUses.ToString();
            _powerupNumberText.gameObject.SetActive(true);
        }
        else
            _powerupNumberText.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_powerupButton.enabled)
            PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.ShowTooltip(_powerupTitle, _powerupText);
        else
            HideTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void HideTooltip()
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.HideTooltip();
    }
}
