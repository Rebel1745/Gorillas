using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private Transform _levelElementHolder;
    [SerializeField] private Transform _explosionMaskHolder;
    [SerializeField] private GameObject[] _levelElements;
    [SerializeField] private int _minimumDistanceBetweenPlayers = 7;
    public int MinimumDistanceBetweenPlayers { get { return _minimumDistanceBetweenPlayers; } }
    [SerializeField] private int _maximumDistanceBetweenPlayers = 30;
    private int _distanceBetweenPlayers;
    [SerializeField] private float _minimumBuildingHeight;
    [SerializeField] private float _maximumBuildingHeight;
    private List<LevelElementDetails> _levelElementDetailsList = new();
    private int _numberOfLevelElements;
    private float _totalElementWidth = 0f;
    public float TotalElementWidth { get { return _totalElementWidth; } }
    private List<Vector3> _playerSpawnPointList = new();
    private List<GameObject> _playerSpawnPointArrows = new();
    private List<GameObject> _levelElementGOs = new();
    private GameObject _levelElementToMove = null;
    private bool _moveLevelElement = false;
    private float _moveLevelElementByValue = 0f;
    private Dictionary<int, int> _spawnPointToGameObjectIndexes = new();
    private List<int> _cpuActiveSpawnPointIndexList = new();
    public List<int> CPUActiveSpawnPointIndexList { get { return _cpuActiveSpawnPointIndexList; } }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        if (_moveLevelElement)
            MoveLevelElement();
    }

    public void BuildLevel()
    {
        ClearCurrentLevel();

        // time to build the level
        ChooseElements();
        PlaceElements();

        GameManager.Instance.UpdateGameState(GameState.SetupPlayers);
    }

    private void PlaceElements()
    {
        float startingXPos = -_totalElementWidth / 2.0f;
        float xOffset = 0f;
        Vector3 newPos;
        GameObject newElement;
        Color randomBuildingColour;
        int ledIndex = 0;
        NetworkObject elementNO;

        foreach (LevelElementDetails led in _levelElementDetailsList)
        {
            newPos = new(startingXPos + xOffset + (led.ElementWidth / 2), led.ElementHeight, 0f);
            newElement = Instantiate(led.ElementPrefab, newPos, Quaternion.identity);
            randomBuildingColour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
            newElement.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().color = randomBuildingColour;
            elementNO = newElement.GetComponent<NetworkObject>();
            elementNO.Spawn(true);
            elementNO.TrySetParent(_levelElementHolder, true);
            xOffset += led.ElementWidth;

            // spawn points are the second child of the building (first is GFX)
            for (int i = 0; i < newElement.transform.GetChild(1).childCount; i++)
            {
                _playerSpawnPointList.Add(newElement.transform.GetChild(1).GetChild(i).transform.position);
                // first child of the spawn point is the arrow
                _playerSpawnPointArrows.Add(newElement.transform.GetChild(1).GetChild(i).GetChild(0).gameObject);
                _playerSpawnPointArrows[_playerSpawnPointArrows.Count - 1].GetComponentInChildren<MovePlayerArrow>().SetArrowIndex(_playerSpawnPointArrows.Count - 1);
                _spawnPointToGameObjectIndexes.Add(_playerSpawnPointArrows.Count - 1, ledIndex);
            }

            // building movement collider is the third child of the building
            newElement.transform.GetChild(2).GetChild(0).GetComponent<MoveBuildingArrow>().SetArrowIndex(ledIndex);
            _levelElementGOs.Add(newElement);

            ledIndex++;
        }
    }

    private void ChooseElements()
    {
        _distanceBetweenPlayers = Random.Range(_minimumDistanceBetweenPlayers, _maximumDistanceBetweenPlayers);
        _numberOfLevelElements = _distanceBetweenPlayers * 3;
        LevelElementDetails newLevelElementDetails;
        GameObject prefab;
        float prefabWidth;
        float prefabHeight;

        for (int i = 0; i <= _numberOfLevelElements; i++)
        {
            prefab = _levelElements[Random.Range(0, _levelElements.Length)];
            prefabWidth = prefab.transform.GetChild(0).transform.localScale.x;
            prefabHeight = Random.Range(_minimumBuildingHeight, _maximumBuildingHeight);

            newLevelElementDetails = new LevelElementDetails
            {
                ElementPrefab = prefab,
                ElementWidth = prefabWidth,
                ElementHeight = prefabHeight
            };

            _levelElementDetailsList.Add(newLevelElementDetails);
            _totalElementWidth += prefabWidth;
        }

        // if the total width is even, add an extra single building
        if (_totalElementWidth % 2 == 0)
        {
            prefab = _levelElements[0];
            prefabWidth = prefab.GetComponentInChildren<SpriteRenderer>().transform.localScale.x;
            prefabHeight = Random.Range(_minimumBuildingHeight, _maximumBuildingHeight);

            newLevelElementDetails = new LevelElementDetails
            {
                ElementPrefab = prefab,
                ElementWidth = prefabWidth,
                ElementHeight = prefabHeight
            };

            _levelElementDetailsList.Add(newLevelElementDetails);
            _totalElementWidth += prefabWidth;
        }
    }

    private void ClearCurrentLevel()
    {
        // loop through all of the level element game objects and destroy
        foreach (GameObject go in _levelElementGOs)
        {
            Destroy(go);
        }
        _levelElementGOs.Clear();

        _levelElementDetailsList.Clear();
        _playerSpawnPointList.Clear();
        _playerSpawnPointArrows.Clear();
        _spawnPointToGameObjectIndexes.Clear();
        _totalElementWidth = 0;

        // destroy the level elements
        for (int i = 0; i < _levelElementHolder.childCount; i++)
        {
            Destroy(_levelElementHolder.GetChild(i).gameObject);
        }

        // destroy the explosion masks
        for (int i = 0; i < _explosionMaskHolder.childCount; i++)
        {
            Destroy(_explosionMaskHolder.GetChild(i).gameObject);
        }
    }

    public Vector3 GetSpawnPointAtIndex(int index)
    {
        return _playerSpawnPointList[index];
    }

    public void ShowHideSpawnPointArrowsBetweenIndexes(int firstIndex, int currentIndex, int lastIndex, bool show)
    {
        _cpuActiveSpawnPointIndexList.Clear();

        for (int i = firstIndex; i <= lastIndex; i++)
        {
            if (i == currentIndex) continue;

            _playerSpawnPointArrows[i].SetActive(show);
            _cpuActiveSpawnPointIndexList.Add(i);
        }
    }

    public void GetFirstAndLastSpawnPoints(out Vector3 firstSpawnPoint, out Vector3 lastSpawnPoint, out int firstSpawnPointIndex, out int lastSpawnPointIndex)
    {
        firstSpawnPointIndex = (_playerSpawnPointList.Count / 2) - Mathf.CeilToInt(_distanceBetweenPlayers / 2f);
        lastSpawnPointIndex = (_playerSpawnPointList.Count / 2) + Mathf.FloorToInt(_distanceBetweenPlayers / 2f);

        firstSpawnPoint = GetSpawnPointAtIndex(firstSpawnPointIndex);
        lastSpawnPoint = GetSpawnPointAtIndex(lastSpawnPointIndex);
    }

    public void EnableDisableBuildingMovementColliders(bool enable)
    {
        int firstIndex = _spawnPointToGameObjectIndexes[PlayerManager.Instance.Players[0].SpawnPointIndex] + 1;
        int lastIndex = _spawnPointToGameObjectIndexes[PlayerManager.Instance.Players[1].SpawnPointIndex] - 1;

        for (int i = firstIndex; i <= lastIndex; i++)
        {
            _levelElementGOs[i].transform.GetChild(2).GetChild(0).gameObject.SetActive(enable);
        }
    }

    public void SetLevelElementMovementIndex(int index)
    {
        if (index == -1) _levelElementToMove = null;
        else _levelElementToMove = _levelElementGOs[index];
    }

    public void StartLevelElementMovement(float value)
    {
        _moveLevelElement = true;
        _moveLevelElementByValue = value; // * 0.1f;
    }

    public void StopLevelElementMovement()
    {
        _moveLevelElement = false;
        _moveLevelElementByValue = 0f;
    }

    private void MoveLevelElement()
    {
        if (_levelElementToMove == null) return;

        float yValue = Mathf.Clamp(_levelElementToMove.transform.position.y + _moveLevelElementByValue * Time.deltaTime, _minimumBuildingHeight - 1, _maximumBuildingHeight + 1);
        _levelElementToMove.transform.position = new(_levelElementToMove.transform.position.x, yValue, _levelElementToMove.transform.position.z);
    }
}

public struct LevelElementDetails
{
    public GameObject ElementPrefab;
    public float ElementWidth;
    public float ElementHeight;
}
