using UnityEngine;

[System.Serializable]
public struct PlayerDetails
{
    public GameObject PlayerConfig;
    public string Name;
    public bool IsCPU;
    public CPU_TYPE CPUType;
    public GameObject PlayerUIPrefab;
    public GameObject PlayerUI;
    public Transform PlayerUIPowerupHolder;
    public PlayerController PlayerController;
    public GameObject PlayerPrefab;
    public Animator PlayerAnimator;
    public GameObject PlayerGameObject;
    public LineRenderer PlayerLineRenderer;
    public AIController PlayerAIController;
    public int ThrowDirection; // 1 for left - right, -1 for right to left
    public bool AlwaysShowTrajectoryLine;
}

public enum CPU_TYPE
{
    Easy,
    Medium,
    Hard,
    Impossible
}