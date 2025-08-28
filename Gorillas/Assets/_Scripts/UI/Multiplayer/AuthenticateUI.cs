using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour
{
    [SerializeField] private Button _authenticateButton;
    [SerializeField] private TMP_InputField _playerNameText;

    private void Awake()
    {
        _authenticateButton.onClick.AddListener(() =>
        {
            if (_playerNameText.text != "")
            {
                LobbyManager.Instance.Authenticate(_playerNameText.text);
                Hide();
            }
        });
    }

    private void Start()
    {
        _playerNameText.text = PlayerPrefs.GetString("PlayerName", "");
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}
