using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPlayerName : MonoBehaviour
{
    public static EditPlayerName Instance { get; private set; }

    public event EventHandler OnNameChanged;

    [SerializeField] private Button _authenticateButton;
    [SerializeField] private TextMeshProUGUI _playerNameText;

    private string playerName = "";


    private void Awake()
    {
        Instance = this;

        _authenticateButton.onClick.AddListener(() =>
        {
            playerName = _playerNameText.text;

            OnNameChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    private void Start()
    {
        OnNameChanged += EditPlayerName_OnNameChanged;
    }

    private void EditPlayerName_OnNameChanged(object sender, EventArgs e)
    {
        LobbyManager.Instance.UpdatePlayerName(GetPlayerName());
    }

    public string GetPlayerName()
    {
        return playerName;
    }
}
