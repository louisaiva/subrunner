using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessManager : MonoBehaviour
{
    public static PostProcessManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    [SerializeField] private Volume _volume;
    public Volume Volume
    {
        get
        {
            if (_volume == null)
            {
                _volume = GetComponent<Volume>();
            }
            return _volume;
        }
    }


    [Header("Effects properties")]
    public float BaseBloom = 0f;
    public float BaseChroma = 0f;
}