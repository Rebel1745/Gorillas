using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private Transform _levelElementHolder;
    [SerializeField] private GameObject[] _levelElements;
    [SerializeField] private int _numberOfLevelElements = 18;
    [SerializeField] private float _minHeight;
    [SerializeField] private float _maxHeight;
    private List<LevelElementDetails> _levelElementDetailsList = new();
    private float _totalElementWidth = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
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

        foreach (LevelElementDetails led in _levelElementDetailsList)
        {
            newPos = new(startingXPos + xOffset + (led.ElementWidth / 2), led.ElementHeight, 0f);
            Instantiate(led.ElementPrefab, newPos, Quaternion.identity, _levelElementHolder);
            xOffset += led.ElementWidth;
        }
    }

    private void ChooseElements()
    {
        LevelElementDetails newLevelElementDetails;
        GameObject prefab;
        float prefabWidth;
        float prefabHeight;

        for (int i = 0; i <= _numberOfLevelElements; i++)
        {
            prefab = _levelElements[UnityEngine.Random.Range(0, _levelElements.Length)];
            prefabWidth = prefab.GetComponentInChildren<SpriteRenderer>().transform.localScale.x;
            prefabHeight = UnityEngine.Random.Range(_minHeight, _maxHeight);

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
        _levelElementDetailsList.Clear();
        _totalElementWidth = 0;

        for (int i = 0; i < _levelElementHolder.childCount; i++)
        {
            Destroy(_levelElementHolder.GetChild(i).gameObject);
        }
    }
}

public struct LevelElementDetails
{
    public GameObject ElementPrefab;
    public float ElementWidth;
    public float ElementHeight;
}
