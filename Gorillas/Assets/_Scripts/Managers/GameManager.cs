using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State { get; private set; }
    public GameState PreviousState { get; private set; }

    [Header("Current Player Info")]
    public int CurrentPlayerId { get; private set; }
    public bool IsCurrentPlayerCPU = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateGameState(GameState.BuildLevel);
    }

    public void UpdateGameState(GameState newState, float delay = 0f)
    {
        PreviousState = State;
        State = newState;

        switch (newState)
        {
            case GameState.BuildLevel:
                BuildLevel();
                break;
            case GameState.SetupPlayers:
                SetupPlayers();
                break;
            case GameState.SetupGame:
                SetupGame();
                break;
            case GameState.WaitingForLaunch:
                break;
            case GameState.WaitingForDetonation:
                break;
            case GameState.NextTurn:
                StartCoroutine(nameof(NextTurn), 1f);
                break;
            case GameState.GameOver:
                break;
        }
    }

    private void SetupPlayers()
    {
        PlayerManager.Instance.SetupPlayers();
    }

    private void BuildLevel()
    {
        LevelManager.Instance.BuildLevel();
    }

    void SetupGame()
    {
        CurrentPlayerId = 0;
        UpdateCurrentPlayerDetails(CurrentPlayerId);
    }

    void UpdateCurrentPlayerDetails(int newPlayerId)
    {
        // hide current players UI
        PlayerManager.Instance.Players[CurrentPlayerId].PlayerUI.SetActive(false);
        // reset their animation
        PlayerManager.Instance.SetPlayerAnimation(CurrentPlayerId, "Idle");

        // update current player details
        CurrentPlayerId = newPlayerId;
        IsCurrentPlayerCPU = PlayerManager.Instance.Players[CurrentPlayerId].IsCPU;
        PlayerManager.Instance.Players[CurrentPlayerId].PlayerUI.SetActive(true);
        PlayerManager.Instance.Players[CurrentPlayerId].PlayerController.SetLaunchButtonActive(true);
        PlayerManager.Instance.SetPlayerAnimation(CurrentPlayerId, "Idle");

        UpdateGameState(GameState.WaitingForLaunch);
    }

    IEnumerator NextTurn(float delay)
    {
        yield return new WaitForSeconds(delay);

        // advance player
        int newPlayerId = (CurrentPlayerId + 1) % 2;
        UpdateCurrentPlayerDetails(newPlayerId);

        UpdateGameState(GameState.WaitingForLaunch);
    }
}

public enum GameState
{
    StartScreen,
    SettingsScreen,
    PauseScreen,
    BuildLevel,
    SetupPlayers,
    SetupGame,
    WaitingForLaunch,
    WaitingForDetonation,
    NextTurn,
    GameOver
}
