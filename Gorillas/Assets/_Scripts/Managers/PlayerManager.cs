using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform _playerHolder;
    [SerializeField] private Transform _uICanvas;
    public PlayerDetails[] Players;
    private int _currentPlayerId;
    public int CurrentPlayerId { get { return _currentPlayerId; } }
    public bool IsCurrentPlayerCPU;

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
        PlayerConfig pc = Players[0].PlayerConfig.GetComponent<PlayerConfig>();
        // create player UI
        GameObject newUI = Instantiate(Players[0].PlayerUI, _uICanvas);
        newUI.SetActive(false);
        Players[0].Name = pc.PlayerName;
        Players[0].IsCPU = pc.isCPU;
        Players[0].PlayerGameObject = newPlayer;
        Players[0].PlayerController = newPlayer.GetComponent<PlayerController>();
        Players[0].PlayerUI = newUI;
        Players[0].PlayerAnimator = newPlayer.GetComponentInChildren<Animator>();
        Players[0].PlayerLineRenderer = newPlayer.GetComponent<LineRenderer>();
        Players[0].PlayerController.SetPlayerDetails(0, newUI, Players[0]);
        Players[0].PlayerAIController = newPlayer.GetComponent<AIController>();
        Players[0].ThrowDirection = 1;

        pc.SavePlayerDetails();

        CameraManager.Instance.AddPlayer(newPlayer.transform.position);

        newPlayer = Instantiate(Players[1].PlayerPrefab, lastSpawnPoint, Quaternion.identity, _playerHolder);
        newPlayer.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        newPlayer.transform.GetChild(1).transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        pc = Players[1].PlayerConfig.GetComponent<PlayerConfig>();
        // create player UI
        newUI = Instantiate(Players[1].PlayerUI, _uICanvas);
        newUI.SetActive(false);
        Players[1].Name = pc.PlayerName;
        Players[1].IsCPU = pc.isCPU;
        Players[1].PlayerGameObject = newPlayer;
        Players[1].PlayerController = newPlayer.GetComponent<PlayerController>();
        Players[1].PlayerUI = newUI;
        Players[1].PlayerAnimator = newPlayer.GetComponentInChildren<Animator>();
        Players[1].PlayerLineRenderer = newPlayer.GetComponent<LineRenderer>();
        Players[1].PlayerController.SetPlayerDetails(1, newUI, Players[1]);
        Players[1].PlayerAIController = newPlayer.GetComponent<AIController>();
        Players[1].ThrowDirection = -1;

        pc.SavePlayerDetails();

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
        // reset their animation
        SetPlayerAnimation(playerId, "Idle");
    }

    public void UpdateCurrentPlayer(int playerId)
    {
        _currentPlayerId = playerId;
        UIManager.Instance.ShowHideUIElement(Players[playerId].PlayerUI, true);
        IsCurrentPlayerCPU = Players[playerId].IsCPU;
        StartCoroutine(Players[playerId].PlayerController.CalculateTrajectoryLine());
        SetPlayerAnimation(playerId, "Idle");

        if (!IsCurrentPlayerCPU)
            Players[playerId].PlayerLineRenderer.enabled = true;
        else
            StartCoroutine(Players[playerId].PlayerAIController.DoAI(Players[playerId].PlayerController));
    }
}

[System.Serializable]
public struct PlayerDetails
{
    public GameObject PlayerConfig;
    public string Name;
    public bool IsCPU;
    public GameObject PlayerUI;
    public PlayerController PlayerController;
    public GameObject PlayerPrefab;
    public Animator PlayerAnimator;
    public GameObject PlayerGameObject;
    public LineRenderer PlayerLineRenderer;
    public AIController PlayerAIController;
    public int ThrowDirection; // 1 for left - right, -1 for right to left
}
