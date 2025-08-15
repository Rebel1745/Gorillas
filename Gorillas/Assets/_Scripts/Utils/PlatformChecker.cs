using System.Runtime.InteropServices;
using UnityEngine;

public class PlatformChecker : MonoBehaviour
{
    public static PlatformChecker Instance { get; private set; }
    [DllImport("__Internal")]
    private static extern bool IsMobile();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public bool IsRunningOnMobile()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        return IsMobile();
#else
        return false;
#endif
    }
}