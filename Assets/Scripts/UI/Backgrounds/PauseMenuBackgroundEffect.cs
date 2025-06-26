using UnityEngine;
using UnityEngine.Rendering;
// using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

public class PauseMenuBackgroundEffect : MonoBehaviour
{
    [Header("Post Processing")]
    [SerializeField] private Bloom bloom;
    [SerializeField] private ChromaticAberration chromatic_aberration;
    [SerializeField] private float pause_bloom_intensity;
    private float base_bloom_intensity;

    private void Awake()
    {
        var postProcessVolume = GameObject.Find("/utils/post_processing").GetComponent<Volume>();
        postProcessVolume.profile.TryGet(out bloom);
        postProcessVolume.profile.TryGet(out chromatic_aberration);

        // on sauvegarde l'intensité de base du bloom
        base_bloom_intensity = bloom.intensity.value;
    }

    // ON ENABLE
    private void OnEnable()
    {
        // on passe le post processing à 
        bloom.intensity.Override(pause_bloom_intensity);

        // on active la chromatic aberration
        chromatic_aberration.active = true;
    }
    // ON Disable
    private void OnDisable()
    {
        // on reset le bloom
        bloom.intensity.Override(base_bloom_intensity);

        // on enlève la chromatic aberration
        chromatic_aberration.active = false;
    }
}