using UnityEngine;
using FMODUnity;

public class AudioBank : MonoBehaviour
{
    public static AudioBank Instance;
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }



    [field: Header("Bob")]
    [field: SerializeField] public EventReference bob_walk { get; private set; }
    [field: SerializeField] public EventReference bob_dodge { get; private set; }

    [field: Header("Chest")]
    [field: SerializeField] public EventReference chest_open { get; private set; }
    [field: SerializeField] public EventReference chest_close { get; private set; }
}