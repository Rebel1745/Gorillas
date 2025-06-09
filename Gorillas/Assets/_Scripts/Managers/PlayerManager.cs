using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public PlayerDetails[] Players;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }
}

[System.Serializable]
public struct PlayerDetails
{
    public string Name;
    public bool IsCPU;
    public GameObject PlayerUI;
    public PlayerController PlayerController;
}
