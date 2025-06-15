using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform _playerHolder;
    [SerializeField] private Transform _uICanvas;
    public PlayerDetails[] Players;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetupPlayers()
    {
        RemovePlayers();
        PlacePlayers();

        GameManager.Instance.UpdateGameState(GameState.SetupGame);
    }

    private void PlacePlayers()
    {
        LevelManager.Instance.GetFirstAndLastSpawnPoints(out Vector3 firstSpawnPoint, out Vector3 lastSpawnPoint);

        // create player
        GameObject newPlayer = Instantiate(Players[0].PlayerPrefab, firstSpawnPoint, Quaternion.identity, _playerHolder);
        // create player UI
        GameObject newUI = Instantiate(Players[0].PlayerUI, _uICanvas);
        newUI.SetActive(false);
        Players[0].PlayerGameObject = newPlayer;
        Players[0].PlayerController = newPlayer.GetComponent<PlayerController>();
        Players[0].PlayerUI = newUI;
        Players[0].PlayerAnimator = newPlayer.GetComponent<Animator>();
        Players[0].PlayerLineRenderer = newPlayer.GetComponent<LineRenderer>();
        Players[0].PlayerController.SetPlayerDetails(0, newUI, Players[0]);

        CameraManager.Instance.AddPlayer(newPlayer.transform.position);

        newPlayer = Instantiate(Players[1].PlayerPrefab, lastSpawnPoint, Quaternion.identity, _playerHolder);
        newPlayer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // create player UI
        newUI = Instantiate(Players[1].PlayerUI, _uICanvas);
        newUI.SetActive(false);
        Players[1].PlayerGameObject = newPlayer;
        Players[1].PlayerController = newPlayer.GetComponent<PlayerController>();
        Players[1].PlayerUI = newUI;
        Players[1].PlayerAnimator = newPlayer.GetComponent<Animator>();
        Players[1].PlayerLineRenderer = newPlayer.GetComponent<LineRenderer>();
        Players[1].PlayerController.SetPlayerDetails(1, newUI, Players[1]);

        CameraManager.Instance.AddPlayer(newPlayer.transform.position);
    }

    private void RemovePlayers()
    {
        for (int i = 0; i < _playerHolder.childCount; i++)
        {
            Destroy(_playerHolder.GetChild(i).gameObject);
        }
    }

    public void SetPlayerAnimation(int playerId, string animation)
    {
        Players[playerId].PlayerAnimator.Play(animation);
    }

    public void UpdatePreviousPlayer(int playerId)
    {
        // hide current players UI
        Players[playerId].PlayerUI.SetActive(false);
        Players[playerId].PlayerLineRenderer.enabled = false;
        // reset their animation
        SetPlayerAnimation(playerId, "Idle");
    }

    public void UpdateCurrentPlayer(int playerId)
    {
        Players[playerId].PlayerUI.SetActive(true);
        Players[playerId].PlayerLineRenderer.enabled = true;
        Players[playerId].PlayerController.SetLaunchButtonActive(true);
        Players[playerId].PlayerController.ShowTrajectoryLine();
        SetPlayerAnimation(playerId, "Idle");
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
    public Animator PlayerAnimator;
    public GameObject PlayerGameObject;
    public LineRenderer PlayerLineRenderer;
}
