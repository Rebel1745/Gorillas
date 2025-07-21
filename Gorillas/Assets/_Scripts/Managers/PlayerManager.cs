using System.Collections.Generic;
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
    [SerializeField] private GameObject[] _availablePowerups;
    private List<GameObject>[] _playerPowerups;
    private List<string>[] _playerPowerupNames;
    private GameObject _player1UI;
    private GameObject _player2UI;

    // USED FOR DEBUG, DELETE WHEN NOT NEEDED
    private List<GameObject> _player1Powerups;
    private List<GameObject> _player2Powerups;
    private List<string> _player1PowerupNames;
    private List<string> _player2PowerupNames;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // ALL DEBUG STUFF
        _playerPowerups = new List<GameObject>[2];
        _playerPowerups[0] = new();
        _playerPowerups[1] = new();

        _playerPowerupNames = new List<string>[2];
        _playerPowerupNames[0] = new();
        _playerPowerupNames[1] = new();
    }

    public void SetupPlayers()
    {
        if (GameManager.Instance.CurrentRound == 0)
            RemovePlayers();

        PlacePlayers();

        GameManager.Instance.UpdateGameState(GameState.SetupGame);
    }

    private void PlacePlayers()
    {
        LevelManager.Instance.GetFirstAndLastSpawnPoints(out Vector3 firstSpawnPoint, out Vector3 lastSpawnPoint, out int firstSpawnPointIndex, out int lastSpawnPointIndex);

        if (GameManager.Instance.CurrentRound == 0)
        {
            // create player
            GameObject newPlayer = Instantiate(Players[0].PlayerPrefab, firstSpawnPoint, Quaternion.identity, _playerHolder);
            PlayerConfig pc = Players[0].PlayerConfig.GetComponent<PlayerConfig>();
            // if we already have a UI, destroy it and create a new one
            if (_player1UI != null)
                Destroy(_player1UI);

            if (_player2UI != null)
                Destroy(_player2UI);

            // create new player UI
            _player1UI = Instantiate(Players[0].PlayerUIPrefab, _uICanvas);
            _player2UI = Instantiate(Players[1].PlayerUIPrefab, _uICanvas);

            newPlayer.name = pc.PlayerName;
            Players[0].Name = pc.PlayerName;
            Players[0].IsCPU = pc.isCPU;
            Players[0].CPUType = (CPU_TYPE)pc.CPUType;
            Players[0].PlayerGameObject = newPlayer;
            Players[0].PlayerController = newPlayer.GetComponent<PlayerController>();
            Players[0].PlayerAnimator = newPlayer.GetComponentInChildren<Animator>();
            Players[0].PlayerLineRenderer = newPlayer.GetComponent<LineRenderer>();
            Players[0].PlayerAIController = newPlayer.GetComponent<AIController>();
            Players[0].PlayerUI = _player1UI;
            Players[0].PlayerUIPowerupHolder = _player1UI.transform.GetChild(1);
            Players[0].ThrowDirection = 1;
            Players[0].SpawnPointIndex = firstSpawnPointIndex;
            pc.SavePlayerDetails();
            Players[0].PlayerController.SetPlayerDetails(0, Players[0]);

            newPlayer = Instantiate(Players[1].PlayerPrefab, lastSpawnPoint, Quaternion.identity, _playerHolder);
            newPlayer.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            newPlayer.transform.GetChild(1).transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            pc = Players[1].PlayerConfig.GetComponent<PlayerConfig>();

            // MORE DEBUG STUFF TO DELETE LATER
            for (int i = 0; i < 50; i++)
            {
                AddRandomPlayerPowerup(0);
            }

            newPlayer.name = pc.PlayerName;
            Players[1].Name = pc.PlayerName;
            Players[1].IsCPU = pc.isCPU;
            Players[1].CPUType = (CPU_TYPE)pc.CPUType;
            Players[1].PlayerGameObject = newPlayer;
            Players[1].PlayerController = newPlayer.GetComponent<PlayerController>();
            Players[1].PlayerAnimator = newPlayer.GetComponentInChildren<Animator>();
            Players[1].PlayerLineRenderer = newPlayer.GetComponent<LineRenderer>();
            Players[1].PlayerAIController = newPlayer.GetComponent<AIController>();
            Players[1].PlayerUI = _player2UI;
            Players[1].PlayerUIPowerupHolder = _player2UI.transform.GetChild(1);
            Players[1].ThrowDirection = -1;
            Players[1].SpawnPointIndex = lastSpawnPointIndex;
            pc.SavePlayerDetails();
            Players[1].PlayerController.SetPlayerDetails(1, Players[1]);
        }
        else
        {
            Players[0].PlayerController.PlacePlayerAndEnable(firstSpawnPoint);

            Players[1].PlayerController.PlacePlayerAndEnable(lastSpawnPoint);
        }

        // MORE DEBUG STUFF TO DELETE LATER
        for (int i = 0; i < 50; i++)
        {
            AddRandomPlayerPowerup(1);
        }


        Players[0].PlayerUI.SetActive(false);
        CameraManager.Instance.AddPlayer(Players[0].PlayerGameObject.transform.position);
        if (Players[0].PlayerController.IsShieldActive)
            Players[0].PlayerController.HideShield();


        Players[1].PlayerUI.SetActive(false);
        CameraManager.Instance.AddPlayer(Players[1].PlayerGameObject.transform.position);
        if (Players[1].PlayerController.IsShieldActive)
            Players[1].PlayerController.HideShield();

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

        // if we had a shield on, turn it off
        if (Players[playerId].PlayerController.IsShieldActive)
            Players[playerId].PlayerController.HideShield();

        if (!IsCurrentPlayerCPU)
            Players[playerId].PlayerLineRenderer.enabled = true;
        else
            StartCoroutine(Players[playerId].PlayerAIController.DoAI());

        GameManager.Instance.UpdateGameState(GameState.WaitingForLaunch);
    }

    public void AddRandomPlayerPowerup(int playerId)
    {
        int randomPowerupIndex = Random.Range(0, _availablePowerups.Length);
        GameObject powerup = _availablePowerups[randomPowerupIndex];
        string puName = powerup.name + "(Clone)";
        List<GameObject> ppuList = _playerPowerups[playerId];
        List<string> ppuNameList = _playerPowerupNames[playerId];

        if (ppuNameList.Contains(puName))
        {
            ppuList[ppuNameList.IndexOf(puName)].GetComponent<Powerup>().AddPowerupUse();
        }
        else
        {
            GameObject pu = Instantiate(powerup, Players[playerId].PlayerUIPowerupHolder);
            ppuList.Add(pu);
            ppuNameList.Add(pu.name);
            // DEBUG STUFF
            if (playerId == 0)
            {
                _player1Powerups = ppuList;
                _player1PowerupNames = ppuNameList;
            }
            else
            {
                _player2Powerups = ppuList;
                _player2PowerupNames = ppuNameList;
            }
        }
    }

    public void RemovePlayerPowerup(GameObject powerup)
    {
        string puName = powerup.name;
        List<GameObject> ppuList = _playerPowerups[CurrentPlayerId];
        List<string> ppuNameList = _playerPowerupNames[CurrentPlayerId];

        ppuList.Remove(powerup);
        ppuNameList.Remove(puName);
        // DEBUG STUFF
        if (CurrentPlayerId == 0)
        {
            _player1Powerups = ppuList;
            _player1PowerupNames = ppuNameList;
        }
        else
        {
            _player2Powerups = ppuList;
            _player2PowerupNames = ppuNameList;
        }
    }
}