using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreenUI : MonoBehaviour
{
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Button _showColourPickerButton;
    [SerializeField] private Button _confirmColourButton;
    [SerializeField] private Button _resetColourButton;
    [SerializeField] private FlexibleColorPicker _colourPicker;
    [SerializeField] private TMP_Text _uiScaleText;
    [SerializeField] private Slider _uiScaleSlider;

    public void ShowSettingsScreen()
    {
        _confirmColourButton.gameObject.SetActive(false);

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        _musicVolumeSlider.value = musicVolume;
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);
        _sfxVolumeSlider.value = sfxVolume;

        string savedColourString = PlayerPrefs.GetString("BackgroundColour", ColorUtility.ToHtmlStringRGBA(GameManager.Instance.DefaultBackgroundColour));
        ColorUtility.TryParseHtmlString("#" + savedColourString, out Color savedColour);
        _colourPicker.SetColor(savedColour);

        _uiScaleSlider.value = PlayerPrefs.GetFloat("UIScale", 1f);
        _uiScaleText.text = (_uiScaleSlider.value * 100).ToString("F0") + "%";
    }

    public void ResetBackgroundColour()
    {
        _colourPicker.SetColor(GameManager.Instance.DefaultBackgroundColour);
        ConfirmBackgroundColour();
    }

    public void ShowColourPicker()
    {
        _colourPicker.gameObject.SetActive(true);
        _showColourPickerButton.gameObject.SetActive(false);
        _confirmColourButton.gameObject.SetActive(true);
        _resetColourButton.gameObject.SetActive(true);
    }

    public void ConfirmBackgroundColour()
    {
        Color color = _colourPicker.color;

        Camera.main.backgroundColor = color;
        PlayerPrefs.SetString("BackgroundColour", ColorUtility.ToHtmlStringRGBA(color));
        _confirmColourButton.gameObject.SetActive(false);
        _showColourPickerButton.gameObject.SetActive(true);
        _colourPicker.gameObject.SetActive(false);
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
}
