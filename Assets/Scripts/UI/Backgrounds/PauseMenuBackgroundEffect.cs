using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
// using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

public class PauseMenuBackgroundEffect : MonoBehaviour
{
    [Header("Bloom Transition")]
    [SerializeField] private Bloom bloom;
    [SerializeField] private float pause_bloom_intensity;
    private float base_bloom_intensity;

    [Header("Chromatic Aberration")]
    [SerializeField] private ChromaticAberration chromatic_aberration;
    [SerializeField] private float chromatic_aberration_intensity;
    private float base_chromatic_aberration_intensity;
    
    // [Header("Transition Parameters")]
    // [SerializeField] private float transition_duration = 0.2f;
    private bool is_very_early_init_done = false;

    private void Awake()
    {
        var postProcessVolume = GameObject.Find("/utils/post_processing").GetComponent<Volume>();
        postProcessVolume.profile.TryGet(out bloom);
        postProcessVolume.profile.TryGet(out chromatic_aberration);

        // on sauvegarde l'intensité de base du bloom
        base_bloom_intensity = bloom.intensity.value;
        base_chromatic_aberration_intensity = chromatic_aberration.intensity.value;
    }

    private void Start() { is_very_early_init_done = true; }

    // ON ENABLE DISABLE
    public void ActivateEffect(float transition_duration = 0.2f)
    {
        Sequence.Create(useUnscaledTime: true)
            .Group(Tween.Custom(base_bloom_intensity, pause_bloom_intensity, duration: transition_duration,
                onValueChange: ctx => bloom.intensity.Override(ctx)))
            .Group(Tween.Custom(base_chromatic_aberration_intensity, chromatic_aberration_intensity, duration: transition_duration,
                onValueChange: ctx => chromatic_aberration.intensity.Override(ctx)));

            /* .OnComplete(() =>
            {
                bloom.intensity.Override(pause_bloom_intensity);
            }); */
    }
    public void DisableEffect(float transition_duration = 0.2f)
    {
        if (!is_very_early_init_done) { return; }

        Sequence.Create(useUnscaledTime: true)
            .Group(Tween.Custom(pause_bloom_intensity, base_bloom_intensity, duration: transition_duration,
                onValueChange: ctx => bloom.intensity.Override(ctx)))
            .Group(Tween.Custom(chromatic_aberration_intensity, base_chromatic_aberration_intensity, duration: transition_duration,
                onValueChange: ctx => chromatic_aberration.intensity.Override(ctx)));

        /* Tween.Custom(pause_bloom_intensity, base_bloom_intensity, duration: transition_duration,
                onValueChange: ctx => bloom.intensity.Override(ctx), useUnscaledTime: true)
            .OnComplete(() => bloom.intensity.Override(base_bloom_intensity)); */

        // on reset le bloom
        // bloom.intensity.Override(base_bloom_intensity);

        // on enlève la chromatic aberration
        // chromatic_aberration.active = false;
    }
}