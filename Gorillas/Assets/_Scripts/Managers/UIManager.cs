using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject StartScreenUI;
    public GameObject SettingsScreenUI;
    public GameObject ScoreBoardUI;

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
        button.enabled = enabled;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowSettingsScreen()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.StartScreenUI, false);
        GameManager.Instance.UpdateGameState(GameState.SettingsScreen);
    }

    public void StartGame()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.SettingsScreenUI, false);
        GameManager.Instance.UpdateGameState(GameState.BuildLevel);
    }

    public void PlayAgain()
    {

    }
}
