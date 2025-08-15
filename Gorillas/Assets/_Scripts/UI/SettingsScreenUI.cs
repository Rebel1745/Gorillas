using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreenUI : MonoBehaviour
{
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Button _showColourPickerButton;
    [SerializeField] private Button _confirmColourButton;
    [SerializeField] private Button _resetColourButton;
    [SerializeField] private Button _showOutlineColourPickerButton;
    [SerializeField] private Button _confirmOutlineColourButton;
    [SerializeField] private Button _resetOutlineColourButton;
    [SerializeField] private FlexibleColorPicker _colourPicker;
    [SerializeField] private TMP_Text _uiScaleText;
    [SerializeField] private Slider _uiScaleSlider;
    [SerializeField] private TMP_Dropdown _player1UITypeDropdown;
    [SerializeField] private TMP_Dropdown _player2UITypeDropdown;

    public void ShowSettingsScreen()
    {
        _confirmColourButton.gameObject.SetActive(false);
        _confirmOutlineColourButton.gameObject.SetActive(false);

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        _musicVolumeSlider.value = musicVolume;
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);
        _sfxVolumeSlider.value = sfxVolume;

        float defaultScale = 1f;
        if (GameManager.Instance.IsMobile) defaultScale = 1.5f;
        _uiScaleSlider.value = PlayerPrefs.GetFloat("UIScale", defaultScale);
        _uiScaleText.text = (_uiScaleSlider.value * 100).ToString("F0") + "%";

        string defaultUIType = GameManager.Instance.IsMobile ? "InputBoxes" : "Sliders";
        string uiTypeText = PlayerPrefs.GetString("UIType0", defaultUIType);
        if (uiTypeText == "Sliders") _player1UITypeDropdown.value = 0;
        else _player1UITypeDropdown.value = 1;

        uiTypeText = PlayerPrefs.GetString("UIType1", defaultUIType);
        if (uiTypeText == "Sliders") _player2UITypeDropdown.value = 0;
        else _player2UITypeDropdown.value = 1;
    }

    public void ResetBackgroundColour()
    {
        _colourPicker.SetColor(GameManager.Instance.DefaultBackgroundColour);
        ConfirmBackgroundColour();
    }

    public void ShowBackgroundColourPicker()
    {
        string savedColourString = PlayerPrefs.GetString("BackgroundColour", ColorUtility.ToHtmlStringRGBA(GameManager.Instance.DefaultBackgroundColour));
        ColorUtility.TryParseHtmlString("#" + savedColourString, out Color savedColour);
        _colourPicker.SetColor(savedColour);

        _colourPicker.gameObject.SetActive(true);
        _showColourPickerButton.gameObject.SetActive(false);
        _confirmColourButton.gameObject.SetActive(true);
        _resetColourButton.gameObject.SetActive(true);
    }

    public void ConfirmBackgroundColour()
    {
        Color colour = _colourPicker.color;

        Camera.main.backgroundColor = colour;
        PlayerPrefs.SetString("BackgroundColour", ColorUtility.ToHtmlStringRGBA(colour));
        _showColourPickerButton.gameObject.SetActive(true);
        _confirmColourButton.gameObject.SetActive(false);
        _colourPicker.gameObject.SetActive(false);
    }

    public void ResetPlayerOutlineColour()
    {
        _colourPicker.SetColor(GameManager.Instance.DefaultPlayerOutlineColour);
        ConfirmPlayerOutlineColour();
    }

    public void ShowPlayerOutlineColourPicker()
    {
        string savedColourString = PlayerPrefs.GetString("PlayerOutlineColour", ColorUtility.ToHtmlStringRGBA(GameManager.Instance.DefaultPlayerOutlineColour));
        ColorUtility.TryParseHtmlString("#" + savedColourString, out Color savedColour);
        _colourPicker.SetColor(savedColour);

        _colourPicker.gameObject.SetActive(true);
        _showOutlineColourPickerButton.gameObject.SetActive(false);
        _confirmOutlineColourButton.gameObject.SetActive(true);
        _resetOutlineColourButton.gameObject.SetActive(true);
    }

    public void ConfirmPlayerOutlineColour()
    {
        Color colour = _colourPicker.color;

        PlayerPrefs.SetString("PlayerOutlineColour", ColorUtility.ToHtmlStringRGBA(colour));
        _showOutlineColourPickerButton.gameObject.SetActive(true);
        _confirmOutlineColourButton.gameObject.SetActive(false);
        _colourPicker.gameObject.SetActive(false);

        if (PlayerManager.Instance.Players[0].PlayerGameObject != null)
        {
            Material mat = PlayerManager.Instance.Players[0].PlayerGameObject.GetComponentInChildren<SpriteRenderer>().material;
            mat.SetColor("_SolidOutline", colour);
            mat = PlayerManager.Instance.Players[1].PlayerGameObject.GetComponentInChildren<SpriteRenderer>().material;
            mat.SetColor("_SolidOutline", colour);
        }
    }

    public void SetUIScale(float scale)
    {
        float scaleText = scale * 100;
        _uiScaleText.text = scaleText.ToString("F0") + "%";
        PlayerPrefs.SetFloat("UIScale", scale);

        for (int i = 0; i < 2; i++)
        {
            if (PlayerManager.Instance.Players[i].PlayerUI)
                PlayerManager.Instance.Players[i].PlayerUI.transform.localScale = new Vector3(scale, scale, 0);
        }
    }

    public void SetUIType1(int type)
    {
        string typeString;

        if (type == 0) typeString = "Sliders";
        else typeString = "TextBoxes";

        if (PlayerManager.Instance.Players[0].PlayerController != null)
            PlayerManager.Instance.Players[0].PlayerController.SetUIType(typeString);
    }

    public void SetUIType2(int type)
    {
        string typeString;

        if (type == 0) typeString = "Sliders";
        else typeString = "TextBoxes";

        if (PlayerManager.Instance.Players[1].PlayerController != null)
            PlayerManager.Instance.Players[1].PlayerController.SetUIType(typeString);
    }
}
