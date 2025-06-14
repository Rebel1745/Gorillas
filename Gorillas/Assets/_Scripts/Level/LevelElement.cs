using System.Runtime.InteropServices.WindowsRuntime;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class LevelElement : MonoBehaviour
{
    public Transform[] PlayerSpawnPoints { get { return _playerSpawnPoints; } }
    [SerializeField] private Transform[] _playerSpawnPoints;
}
