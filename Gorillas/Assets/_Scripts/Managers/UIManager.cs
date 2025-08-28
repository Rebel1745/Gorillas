using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject StartScreenUI;
    public GameObject GameSetupScreenUI;
    public GameObject SettingsScreenUI;
    public GameObject ScoreBoardUI;
    public GameObject GameOverUI;
    public GameObject GameUI;
    public GameObject MultiplayerUI;
    [SerializeField] private Button _settingsButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ShowHideUIElement(GameObject element, bool show)
    {
        element.SetActive(show);
    }

    public void EnableDisableButton(Button button, bool enabled)
    {
        if (button != null)
            button.enabled = enabled;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowGameSetupScreen()
    {
        ShowHideUIElement(StartScreenUI, false);
        GameManager.Instance.UpdateGameState(GameState.GameSetupScreen);
    }

    public void ShowMultiplayerScreen()
    {
        ShowHideUIElement(StartScreenUI, false);
        GameManager.Instance.UpdateGameState(GameState.MultiplayerScreen);
    }

    public void StartGame()
    {
        AudioManager.Instance.StopBackgroundMusic();
        ShowHideUIElement(GameSetupScreenUI, false);
        GameManager.Instance.UpdateGameState(GameState.InitialiseGame);
    }

    public void ShowStartMenu()
    {
        ShowHideUIElement(GameOverUI, false);
        GameManager.Instance.UpdateGameState(GameState.StartScreen);
    }

    public void ShowHideSettingsScreen(bool show)
    {
        _settingsButton.gameObject.SetActive(!show);
        if (show)
        {
            Time.timeScale = 0f;
            ShowHideUIElement(SettingsScreenUI, true);
            GameManager.Instance.UpdateGameState(GameState.SettingsScreen);
        }
        else
        {
            Time.timeScale = 1f;
            ShowHideUIElement(SettingsScreenUI, false);
            GameManager.Instance.RevertToPreviousState();
        }
    }
}
