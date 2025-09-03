using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public GameState State { get; private set; }
    public GameState PreviousState { get; private set; }

    [Header("Current Player Info")]
    public int CurrentPlayerId { get; private set; }
    public int OtherPlayerId { get { return (CurrentPlayerId + 1) % 2; } }
    public bool IsCurrentPlayerCPU = false;
    private int[] _playerScores;
    private int[] _gamesLostInARow;
    private int[] _playerMisses;
    [SerializeField] private TMP_Text _playerScoreText;
    private int _numberOfRounds = 3;
    private int _currentRound;
    public int CurrentRound { get { return _currentRound; } }
    private bool _usePowerups = false;
    public bool UsePowerups { get { return _usePowerups; } }
    [SerializeField] private float _timeBetweenRounds = 3f;
    [SerializeField] private AudioClip _mainMenuMusic;
    [SerializeField] private TMP_InputField _numberOfRoundsInput;
    [SerializeField] private Toggle _usePowerupsToggle;
    [SerializeField] private Color _defaultBackgroundColour;
    public Color DefaultBackgroundColour { get { return _defaultBackgroundColour; } }
    [SerializeField] private Color _defaultPlayerOutlineColour;
    public Color DefaultPlayerOutlineColour { get { return _defaultPlayerOutlineColour; } }
    private bool _isMobile;
    public bool IsMobile { get { return _isMobile; } }

    // multiplayer stuff
    private Lobby _lobby;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateGameState(GameState.StartScreen);

        LobbyManager.Instance.OnGameStarted += LobbyManager_StartMultiplayerGame;
    }

    private void LobbyManager_StartMultiplayerGame(object sender, LobbyManager.LobbyEventArgs e)
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.MultiplayerUI, false);

        if (!IsServer) return;

        _lobby = e.lobby;
        /*foreach (Player p in _lobby.Players)
        {
            Debug.Log($"PlayerId: {p.Id} - Name: {p.Data[LobbyManager.Instance.Key_Player_Name].Value}");
        }*/
        UpdateGameState(GameState.InitialiseGame);
    }

    /*public override void OnNetworkSpawn()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.MultiplayerUI, false);

        if (!IsServer) return;

        // if the player spawned is the player, start building the level
        UpdateGameState(GameState.InitialiseGame);
    }*/

    public void UpdateGameState(GameState newState, float delay = 0f)
    {
        PreviousState = State;
        State = newState;

        switch (newState)
        {
            case GameState.StartScreen:
                ShowStartScreen();
                break;
            case GameState.GameSetupScreen:
                ShowGameSetupScreen();
                break;
            case GameState.SettingsScreen:
                ShowSettingsScreen();
                break;
            case GameState.InitialiseGame:
                InitialiseGame();
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
            case GameState.WaitingForMovement:
                WaitingForMovement();
                break;
            case GameState.WaitingForBuildingMovement:
                WaitingForBuildingMovement();
                break;
            case GameState.NextTurn:
                StartCoroutine(nameof(NextTurn), delay);
                break;
            case GameState.RoundComplete:
                StartCoroutine(RoundComplete(_timeBetweenRounds));
                break;
            case GameState.GameOver:
                StartCoroutine(nameof(GameOver), delay);
                break;
            case GameState.MultiplayerScreen:
                ShowMultiplayerScreen();
                break;
        }
    }

    private void ShowMultiplayerScreen()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.MultiplayerUI, true);
    }

    public void RevertToPreviousState()
    {
        UpdateGameState(PreviousState);
    }

    private void ShowSettingsScreen()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.StartScreenUI, false);
        UIManager.Instance.SettingsScreenUI.GetComponent<SettingsScreenUI>().ShowSettingsScreen();
    }

    private void WaitingForBuildingMovement()
    {
        InputManager.Instance.EnableDisableBuildingMovementControls(true);
    }

    private void WaitingForMovement()
    {
        int maximumMovementDistance = 3; //Random.Range(0, 4);
        PlayerManager.Instance.Players[CurrentPlayerId].PlayerController.ShowHideMovementPowerupIndicators(maximumMovementDistance, true);
        InputManager.Instance.EnableDisableMovementPowerupControls(true);
    }

    private void InitialiseGame()
    {
        _playerScores = new int[2];
        _gamesLostInARow = new int[2];
        _playerMisses = new int[2];
        UpdateScoreboard();
        _currentRound = 0;
        CurrentPlayerId = 0;
        UpdateGameState(GameState.BuildLevel);
    }

    IEnumerator GameOver(float delay)
    {
        //Debug.Log($"{_playerMisses[0] - _playerScores[0]} - {_playerMisses[1] - _playerScores[1]}");
        yield return new WaitForSeconds(delay);

        UIManager.Instance.ShowHideUIElement(UIManager.Instance.ScoreBoardUI, false);
        // show game over screen with positions
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.GameOverUI, true);
        UIManager.Instance.GameOverUI.GetComponent<GameOverUI>().SetGameOverDetails(_playerScores);
    }

    private void ShowGameSetupScreen()
    {
        _numberOfRounds = PlayerPrefs.GetInt("NumberOfRounds", 9);
        _numberOfRoundsInput.text = _numberOfRounds.ToString();

        int usePowerups = PlayerPrefs.GetInt("UsePowerups", 1);
        if (usePowerups == 0) _usePowerups = false;
        else _usePowerups = true;
        _usePowerupsToggle.isOn = _usePowerups;

        UIManager.Instance.ShowHideUIElement(UIManager.Instance.GameSetupScreenUI, true);
    }

    private void ShowStartScreen()
    {
        // check to see if we are on mobile or computer
        _isMobile = PlatformChecker.Instance.IsRunningOnMobile();

        string savedColourString = PlayerPrefs.GetString("BackgroundColour", ColorUtility.ToHtmlStringRGBA(_defaultBackgroundColour));
        ColorUtility.TryParseHtmlString("#" + savedColourString, out Color savedColour);
        Camera.main.backgroundColor = savedColour;

        InputManager.Instance.EnableDisableUIControls(true);
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.StartScreenUI, true);
        AudioManager.Instance.PlayBackgroundMusic(_mainMenuMusic);
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
        PlayerManager.Instance.SetupPlayers(_lobby.Players[0].Data[LobbyManager.Instance.Key_Player_Name].Value, _lobby.Players[1].Data[LobbyManager.Instance.Key_Player_Name].Value);
    }

    private void BuildLevel()
    {
        LevelManager.Instance.BuildLevel();
    }

    private void SetupGame()
    {
        Debug.Log("SetupGame");
        // if this is the first round, move on
        if (_currentRound == 0)
            UpdateCurrentPlayerDetails(CurrentPlayerId);
        // if not, move to the next player immediately
        else
            UpdateGameState(GameState.NextTurn);

        UIManager.Instance.ShowHideUIElement(UIManager.Instance.ScoreBoardUI, true);
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.GameUI, true);
        //InputManager.Instance.EnableDisableControls(true);
        InputManager.Instance.EnableDisableGameplayControls(true);
    }

    private void UpdateCurrentPlayerDetails(int newPlayerId)
    {
        Debug.Log("UpdateCurrentPlayerDetails");
        PlayerManager.Instance.UpdatePreviousPlayer(CurrentPlayerId);

        // update current player details
        CurrentPlayerId = newPlayerId;
        IsCurrentPlayerCPU = PlayerManager.Instance.Players[CurrentPlayerId].IsCPU;
        PlayerManager.Instance.UpdateCurrentPlayer(CurrentPlayerId);
    }

    private IEnumerator NextTurn(float delay)
    {
        yield return new WaitForSeconds(delay);

        _playerMisses[CurrentPlayerId]++;

        // advance player
        int newPlayerId = OtherPlayerId;
        UpdateCurrentPlayerDetails(newPlayerId);

        //UpdateGameState(GameState.WaitingForLaunch);
    }

    public void UpdateScore(int playerId)
    {
        int otherPlayerId = (playerId + 1) % 2;
        _playerScores[playerId]++;
        // increment games lost in a row for the other player and reset for scoring player
        _gamesLostInARow[playerId] = 0;
        _gamesLostInARow[otherPlayerId] += 1;

        // as player has scored, give the other player random powerups based on the games lost in a row
        for (int i = 0; i < _gamesLostInARow[otherPlayerId]; i++)
        {
            PlayerManager.Instance.AddRandomPlayerPowerup(otherPlayerId);
        }

        UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        _playerScoreText.text = _playerScores[0].ToString() + " - " + _playerScores[1].ToString();
    }

    public void SetNumberOfRounds(string numberOfRounds)
    {
        _numberOfRounds = int.Parse(numberOfRounds);
        PlayerPrefs.SetInt("NumberOfRounds", _numberOfRounds);
    }

    public void SetUsePowerupsToggle(bool usePowerups)
    {
        _usePowerups = usePowerups;
        if (usePowerups) PlayerPrefs.SetInt("UsePowerups", 1);
        else PlayerPrefs.SetInt("UsePowerups", 0);
    }
}

public enum GameState
{
    StartScreen,
    SettingsScreen,
    GameSetupScreen,
    InitialiseGame,
    BuildLevel,
    SetupPlayers,
    SetupGame,
    WaitingForLaunch,
    WaitingForDetonation,
    WaitingForMovement,
    WaitingForBuildingMovement,
    NextTurn,
    RoundComplete,
    GameOver,
    MultiplayerScreen
}
