#if UNITY_EDITOR
using UnityEditor;
#endif

using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
// using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
#pragma warning disable 4014

public class PauseMenuBackgroundEffect : MonoBehaviour
{
    [Header("Background effect")]
    [SerializeField] protected Image bg;
    [SerializeField] protected Vector2Int bg_alpha_range = new Vector2Int(0, 245);

    [Header("Bloom Transition")]
    [SerializeField] private Bloom bloom;
    [SerializeField] private float pause_bloom_intensity;
    private float base_bloom_intensity;

    [Header("Chromatic Aberration")]
    [SerializeField] private ChromaticAberration chromatic_aberration;
    [SerializeField] private float chromatic_aberration_intensity;
    private float base_chromatic_aberration_intensity;


    private bool is_very_early_init_done = false;

    private void Awake()
    {
        var postProcessVolume = GameObject.Find("/utils/post_processing").GetComponent<Volume>();
        postProcessVolume.profile.TryGet(out bloom);
        postProcessVolume.profile.TryGet(out chromatic_aberration);

        // on sauvegarde l'intensité de base du bloom
        base_bloom_intensity = bloom.intensity.value;
        base_chromatic_aberration_intensity = chromatic_aberration.intensity.value;

        // on set le bg alpha
        bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0f);
    }

    private void Start() { is_very_early_init_done = true; }

    // SHOW / HIDE
    public async Awaitable TransitionAlpha(bool show, float duration,float override_final_alpha = default)
    {
        float start_alpha = bg.color.a;
        float end_alpha = show ? bg_alpha_range.y / 255f : bg_alpha_range.x / 255f;

        if (override_final_alpha != default)
        {
            end_alpha = override_final_alpha;
        }

        await Tween.Custom(start_alpha, end_alpha, duration: duration,
                onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx), useUnscaledTime: true);

        /* if (show)
        {
            ActivateEffect(duration);
            await Tween.Custom(bg_alpha_range.x / 255f, bg_alpha_range.y / 255f, duration: duration,
                onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx), useUnscaledTime: true);
        }
        else
        {
            DisableEffect(duration);
            await Tween.Custom(bg_alpha_range.y / 255f, bg_alpha_range.x / 255f, duration: duration,
                onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx), useUnscaledTime: true);
        } */
    }
    public async Awaitable TransitionEffect(bool show, float duration = 0.2f)
    {
        if (show)
        {
            await ActivateEffect(duration);
        }
        else
        {
            await DisableEffect(duration);
        }
    }

    private async Awaitable ActivateEffect(float transition_duration = 0.2f)
    {
        await Sequence.Create(useUnscaledTime: true)
            .Group(Tween.Custom(base_bloom_intensity, pause_bloom_intensity, duration: transition_duration,
                onValueChange: ctx => bloom.intensity.Override(ctx)))
            .Group(Tween.Custom(base_chromatic_aberration_intensity, chromatic_aberration_intensity, duration: transition_duration,
                onValueChange: ctx => chromatic_aberration.intensity.Override(ctx)));

        /* .OnComplete(() =>
        {
            bloom.intensity.Override(pause_bloom_intensity);
        }); */
    }
    private async Awaitable DisableEffect(float transition_duration = 0.2f)
    {
        if (!is_very_early_init_done) { return; }

        await Sequence.Create(useUnscaledTime: true)
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


#if UNITY_EDITOR
    [CustomEditor(typeof(PauseMenuBackgroundEffect))]
    public class UI_PauseMenuBackgroundEffectEditor : Editor
    {
        private bool bg_showed = false;
        public override void OnInspectorGUI()
        {
            PauseMenuBackgroundEffect effect = (PauseMenuBackgroundEffect)target;
            if (!bg_showed && GUILayout.Button("Show Background"))
            {
                effect.bg.color = new Color(effect.bg.color.r, effect.bg.color.g, effect.bg.color.b, effect.bg_alpha_range.y / 255f);
                bg_showed = true;
            }
            if (bg_showed && GUILayout.Button("Hide Background"))
            {
                effect.bg.color = new Color(effect.bg.color.r, effect.bg.color.g, effect.bg.color.b, effect.bg_alpha_range.x / 255f);
                bg_showed = false;
            }

            DrawDefaultInspector();
        }
    }
#endif
}