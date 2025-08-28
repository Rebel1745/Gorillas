using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using System;

public class MultiplayerUIManager : MonoBehaviour
{
    public static MultiplayerUIManager Instance { get; private set; }

    [SerializeField] private GameObject _authenticationUI;
    [SerializeField] private TMP_InputField _playerUsernameInput;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        SetPlayerName();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        /*LobbyManager.Instance.OnAuthenticated += (EventArgs) =>
        {
            _authenticationUI.SetActive(false);
        };*/
    }

    private void SetPlayerName()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "");

        if (playerName != "")
            _playerUsernameInput.text = playerName;
    }

    public void Authenticate()
    {
        if (_playerUsernameInput.text == "") return;

        LobbyManager.Instance.Authenticate(_playerUsernameInput.text);
    }
}
