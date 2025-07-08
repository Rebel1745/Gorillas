using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State { get; private set; }
    public GameState PreviousState { get; private set; }

    [Header("Current Player Info")]
    public int CurrentPlayerId { get; private set; }
    public bool IsCurrentPlayerCPU = false;
    private int[] _playerScores;
    private int[] _playerMisses;
    [SerializeField] private TMP_Text _playerScoreText;
    private int _numberOfRounds = 3;
    private int _currentRound;
    [SerializeField] private float _timeBetweenRounds = 3f;
    [SerializeField] private AudioClip _mainMenuMusic;
    [SerializeField] private TMP_InputField _numberOfRoundsInput;
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateGameState(GameState.StartScreen);
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
            case GameState.NextTurn:
                StartCoroutine(nameof(NextTurn), delay);
                break;
            case GameState.RoundComplete:
                StartCoroutine(RoundComplete(_timeBetweenRounds));
                break;
            case GameState.GameOver:
                StartCoroutine(nameof(GameOver), delay);
                break;
        }
    }

    private void InitialiseGame()
    {
        _playerScores = new int[2];
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

    private void ShowSettingsScreen()
    {
        _numberOfRounds = PlayerPrefs.GetInt("NumberOfRounds", 9);
        _numberOfRoundsInput.text = _numberOfRounds.ToString();
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        _musicVolumeSlider.value = musicVolume;
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);
        _sfxVolumeSlider.value = sfxVolume;
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.SettingsScreenUI, true);
    }

    private void ShowStartScreen()
    {
        InputManager.Instance.EnableDisableControls(false);
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

        /*UpdateGameState(GameState.WaitingForLaunch);*/
    }

    private IEnumerator NextTurn(float delay)
    {
        yield return new WaitForSeconds(delay);

        _playerMisses[CurrentPlayerId]++;

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

    public void SetNumberOfRounds(string numberOfRounds)
    {
        _numberOfRounds = int.Parse(numberOfRounds);
        PlayerPrefs.SetInt("NumberOfRounds", _numberOfRounds);
    }
}

public enum GameState
{
    StartScreen,
    SettingsScreen,
    PauseScreen,
    InitialiseGame,
    BuildLevel,
    SetupPlayers,
    SetupGame,
    WaitingForLaunch,
    WaitingForDetonation,
    NextTurn,
    RoundComplete,
    GameOver
}
