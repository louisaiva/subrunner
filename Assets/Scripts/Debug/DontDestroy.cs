using UnityEngine;

/// <summary>
/// ensures this gameObject is not destroyed when switching scenes
/// </summary>
public class DontDestroy : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}