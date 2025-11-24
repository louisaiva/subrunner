using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PostProcessManager : MonoBehaviour
{
    [Header("Scenes base settings")]
    [SerializeField] private List<SceneBasePostProcessSettings> scenes_base_settings;

    [Header("Volume")]
    private Volume _volume;
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

    [Header("Post Process Effects")]
    private Bloom bloom;
    private ChromaticAberration chromatic_aberration;
    private Tonemapping tonemapping;

    [Header("Logs")]
    [SerializeField] private bool log;

    // AWAKE & SINGLETON LOGIC
    public static PostProcessManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // on récupère les différents effects de post processing
        Volume.profile.TryGet(out bloom);
        Volume.profile.TryGet(out chromatic_aberration);
        Volume.profile.TryGet(out tonemapping);

        // on set les settings de base de la scene
        bloom.intensity.value = GetDefaultBloom();
        chromatic_aberration.intensity.value = GetDefaultChroma();
        if (log) { Debug.Log("(PostProcessManager) Awake done, resetted post process effects to scene setting : bloom = " + bloom.intensity.value + ", chromatic_aberration = " + chromatic_aberration.intensity.value); }

        SetToneMapping(aces:title_screen);
    }


    [Header("Tweens")]
    private Tween? bloom_tween = null;
    
    public async Awaitable TransitionBloom(float bloom_target, float duration = 0.2f)
    {
        // checks if already tweening we stop it
        if (bloom_tween != null && bloom_tween.Value.isAlive) { bloom_tween.Value.Stop(); }

        // checks if same value
        if (bloom.intensity.value == bloom_target) { return; }

        // checks null duration
        if (duration <= 0f)
        {
            bloom.intensity.Override(bloom_target);
            return;
        }

        // tweening
        bloom_tween = Tween.Custom(bloom.intensity.value,
                        bloom_target,
                        duration: duration,
                        useUnscaledTime: true,
                        onValueChange: ctx => bloom.intensity.Override(ctx));
        while (bloom_tween.Value.isAlive) { await Task.Yield(); }
        bloom_tween = null;
    }
    
    public float Bloom
    {
        get
        {
            if (bloom == null) { return 0f; }
            return bloom.intensity.value;
        }
    }
    public void SetToneMapping(bool aces=false)
    {
        tonemapping.mode.Override(aces ? TonemappingMode.ACES : TonemappingMode.None);
    }



    // CHROMA
    private Tween? chroma_tween = null;

    public float Chroma
    {
        get
        {
            if (chromatic_aberration == null) { return 0f; }
            return chromatic_aberration.intensity.value;
        }
    }
    public async Awaitable TransitionChroma(float chroma, float duration = 0.2f)
    {
        // checks if already tweening we stop it
        if (chroma_tween != null && chroma_tween.Value.isAlive) { chroma_tween.Value.Stop(); }

        chroma += calculate_perso_additive_chroma();

        // checks if same value
        if (chromatic_aberration.intensity.value == chroma) { return; }

        // checks null duration
        if (duration <= 0f)
        {
            chromatic_aberration.intensity.Override(chroma);
            return;
        }

        // tweening
        chroma_tween = Tween.Custom(chromatic_aberration.intensity.value,
                        chroma,
                        duration: duration,
                        useUnscaledTime: true,
                        onValueChange: ctx => chromatic_aberration.intensity.Override(ctx));
        while (chroma_tween.Value.isAlive) { await Task.Yield(); }
    }

    // BASE CHROMA (different than chroma because this is based on scene + perso life percentage)
    private float calculate_perso_additive_chroma()
    {
        // checks if we don't have a Perso.Instance it is null
        if (Perso.Instance == null) { return 0f; }

        // on calcule le chroma en fonction de la vie du perso
        float life = Perso.Instance.LifePourcent;
        if (life >= 0.5f) { return 0f; } // pas de chroma si on est au dessus de 50% de vie
        // chroma = -life + 0.5 -> bcz when life = 0 we want to have chroma = 0.5
        return -life + 0.5f; 
    }
    public void UpdateChroma()
    {
        if (chroma_tween != null && chroma_tween.Value.isAlive) { return; } // on ne met à jour le chroma que si on n'est pas en train de le tween
        float target_chroma = GetDefaultChroma() + calculate_perso_additive_chroma();
        chromatic_aberration.intensity.Override(target_chroma);
    }


    // GET DEFAULT SCENE SETTINGS
    private bool title_screen => SceneManager.GetActiveScene().name == "subrunner-title-screen";
    private bool clean_scene => SceneManager.GetActiveScene().name == "subrunner-clean";
    public float GetDefaultChroma()
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (var s in scenes_base_settings)
        {
            if (s.scene_name != scene.name) { continue; }
            if (scene.name == "subrunner-clean")
            {
                // on return chroma
            }
            return s.chromatic_aberration_intensity;
        }
        return 0f;
    }
    public float GetDefaultBloom()
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (var s in scenes_base_settings)
        {
            if (s.scene_name != scene.name) { continue; }
            return s.bloom_intensity;
        }
        return 0f;
    }
}

[System.Serializable]
public class SceneBasePostProcessSettings
{
    public string scene_name;
    public float bloom_intensity = 0f;
    public float chromatic_aberration_intensity = 0f;
}