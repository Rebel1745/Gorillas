using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State { get; private set; }
    public GameState PreviousState { get; private set; }

    [Header("Current Player Info")]
    public int CurrentPlayerId { get; private set; }
    public bool IsCurrentPlayerCPU = false;
    private int[] _playerScores;
    [SerializeField] private TMP_Text _playerScoreText;
    [SerializeField] private int _numberOfRounds = 3;
    private int _currentRound;
    [SerializeField] private float _timeBetweenRounds = 3f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateGameState(GameState.StartScreen);
        _playerScores = new int[2];
        UpdateScoreboard();
        _currentRound = 0;
        CurrentPlayerId = 0;
    }

    public void UpdateGameState(GameState newState, float delay = 0f)
    {
        PreviousState = State;
        State = newState;

        switch (newState)
        {
            case GameState.StartScreen:
                ShowStartScreen();
                break;
            case GameState.SettingsScreen:
                ShowSettingsScreen();
                break;
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
                StartCoroutine(nameof(NextTurn), delay);
                break;
            case GameState.RoundComplete:
                StartCoroutine(RoundComplete(_timeBetweenRounds));
                break;
            case GameState.GameOver:
                break;
        }
    }

    private void ShowSettingsScreen()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.SettingsScreenUI, true);
    }

    private void ShowStartScreen()
    {
        InputManager.Instance.EnableDisableControls(false);
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.StartScreenUI, true);
    }

    private IEnumerator RoundComplete(float delay)
    {
        yield return new WaitForSeconds(delay);

        _currentRound++;

        CameraManager.Instance.ResetCamera();

        if (_currentRound == _numberOfRounds)
            UpdateGameState(GameState.GameOver);
        else
            UpdateGameState(GameState.BuildLevel);
    }

    private void SetupPlayers()
    {
        PlayerManager.Instance.SetupPlayers();
    }

    private void BuildLevel()
    {
        LevelManager.Instance.BuildLevel();
    }

    private void SetupGame()
    {
        // if this is the first round, move on
        if (_currentRound == 0)
            UpdateCurrentPlayerDetails(CurrentPlayerId);
        // if not, move to the next player immediately
        else
            UpdateGameState(GameState.NextTurn);

        UIManager.Instance.ShowHideUIElement(UIManager.Instance.ScoreBoardUI, true);
        InputManager.Instance.EnableDisableControls(true);

    }

    private void UpdateCurrentPlayerDetails(int newPlayerId)
    {
        PlayerManager.Instance.UpdatePreviousPlayer(CurrentPlayerId);

        // update current player details
        CurrentPlayerId = newPlayerId;
        IsCurrentPlayerCPU = PlayerManager.Instance.Players[CurrentPlayerId].IsCPU;
        PlayerManager.Instance.UpdateCurrentPlayer(CurrentPlayerId);

        UpdateGameState(GameState.WaitingForLaunch);
    }

    private IEnumerator NextTurn(float delay)
    {
        yield return new WaitForSeconds(delay);

        // advance player
        int newPlayerId = (CurrentPlayerId + 1) % 2;
        UpdateCurrentPlayerDetails(newPlayerId);

        UpdateGameState(GameState.WaitingForLaunch);
    }

    public void UpdateScore(int playerId)
    {
        _playerScores[playerId]++;
        UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        _playerScoreText.text = _playerScores[0].ToString() + " - " + _playerScores[1].ToString();
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
    RoundComplete,
    GameOver
}
