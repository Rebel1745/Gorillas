using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform _playerHolder;
    public PlayerDetails[] Players;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetupPlayers()
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
