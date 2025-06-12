using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform _playerHolder;
    [SerializeField] private Transform _uICanvas;
    public PlayerDetails[] Players;
    [SerializeField, Range(0f, 1f)] private float _player1XPositionPercent = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _player2XPositionPercent = 0.8f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (_player2XPositionPercent <= _player1XPositionPercent)
            Debug.LogError("Player 2 cannot be to the left of player 1!  Adjust the percentages.");
    }

    public void SetupPlayers()
    {
        RemovePlayers();
        PlacePlayers();

        GameManager.Instance.UpdateGameState(GameState.SetupGame);
    }

    private void PlacePlayers()
    {
        int spawnPointIndex = Mathf.FloorToInt(LevelManager.Instance.TotalElementWidth * _player1XPositionPercent);
        Vector3 spawnPos = LevelManager.Instance.GetSpawnPointAtIndex(spawnPointIndex);
        spawnPos += new Vector3(0f, Players[0].PlayerPrefab.transform.localScale.y, 0f);

        // create player
        GameObject newPlayer = Instantiate(Players[0].PlayerPrefab, spawnPos, Quaternion.identity, _playerHolder);
        // create player UI
        GameObject newUI = Instantiate(Players[0].PlayerUI, _uICanvas);
        newUI.SetActive(false);
        Players[0].PlayerController = newPlayer.GetComponent<PlayerController>();
        Players[0].PlayerController.SetUIDetails(newUI);
        Players[0].PlayerUI = newUI;

        spawnPointIndex = Mathf.FloorToInt(LevelManager.Instance.TotalElementWidth * _player2XPositionPercent);
        spawnPos = LevelManager.Instance.GetSpawnPointAtIndex(spawnPointIndex);
        spawnPos += new Vector3(0f, Players[1].PlayerPrefab.transform.localScale.y, 0f);

        newPlayer = Instantiate(Players[1].PlayerPrefab, spawnPos, Quaternion.identity, _playerHolder);
        // create player UI
        newUI = Instantiate(Players[1].PlayerUI, _uICanvas);
        newUI.SetActive(false);
        Players[1].PlayerController = newPlayer.GetComponent<PlayerController>();
        Players[1].PlayerController.SetUIDetails(newUI);
        Players[1].PlayerUI = newUI;
    }

    private void RemovePlayers()
    {
        for (int i = 0; i < _playerHolder.childCount; i++)
        {
            Destroy(_playerHolder.GetChild(i).gameObject);
        }
    }
}

[System.Serializable]
public struct PlayerDetails
{
    public string Name;
    public bool IsCPU;
    public GameObject PlayerUI;
    public PlayerController PlayerController;
    public GameObject PlayerPrefab;
}
