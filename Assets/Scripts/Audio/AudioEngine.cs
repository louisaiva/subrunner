using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Collections;


public class AudioEngine : MonoBehaviour
{

    // AWAKE
    public static AudioEngine Instance;
    protected virtual void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // we get the buses
        master_bus = RuntimeManager.GetBus("bus:/");
        sfx_bus = RuntimeManager.GetBus("bus:/sfx");
        music_bus = RuntimeManager.GetBus("bus:/music");
        ambiance_bus = RuntimeManager.GetBus("bus:/ambiance");
    }

    [Header("Wait delay at start")]
    [SerializeField] private float start_delay = 1f;
    [SerializeField] private bool can_play = false;

    [Header("Audio buses")]
    public Bus master_bus;
    public Bus sfx_bus;
    public Bus music_bus;
    public Bus ambiance_bus;

    [Header("Logs")]
    public bool log_play = false;
    public bool log_can_play_hover = false;

    // SETTINGS REGISTERING
    private void register_settings()
    {
        global_volume_setting = SettingsManager.Instance.GetSetting("global_volume") as StepSetting;
        sfx_volume_setting = SettingsManager.Instance.GetSetting("sfx_volume") as StepSetting;
        music_volume_setting = SettingsManager.Instance.GetSetting("music_volume") as StepSetting;
        ambiance_volume_setting = SettingsManager.Instance.GetSetting("ambiance_volume") as StepSetting;

        if (global_volume_setting != null) { global_volume_setting.OnValueChanged += SetGlobalVolume; SetGlobalVolume(global_volume_setting.Value); }
        if (sfx_volume_setting != null) { sfx_volume_setting.OnValueChanged += SetSFXVolume; SetSFXVolume(sfx_volume_setting.Value); }
        if (music_volume_setting != null) { music_volume_setting.OnValueChanged += SetMusicVolume; SetMusicVolume(music_volume_setting.Value); }
        if (ambiance_volume_setting != null) { ambiance_volume_setting.OnValueChanged += SetAmbianceVolume; SetAmbianceVolume(ambiance_volume_setting.Value); }
    }
    private void unregister_settings()
    {
        // enleve les callbacks des settings
        if (global_volume_setting != null) { global_volume_setting.OnValueChanged -= SetGlobalVolume; }
        if (sfx_volume_setting != null) { sfx_volume_setting.OnValueChanged -= SetSFXVolume; }
        if (music_volume_setting != null) { music_volume_setting.OnValueChanged -= SetMusicVolume; }
        if (ambiance_volume_setting != null) { ambiance_volume_setting.OnValueChanged -= SetAmbianceVolume; }
    }

    // SETTINGS
    private void SetGlobalVolume(float volume) { master_bus.setVolume(volume / 100f); }
    private void SetMusicVolume(float volume) { music_bus.setVolume(volume / 100f); }
    private void SetAmbianceVolume(float volume) { ambiance_bus.setVolume(volume / 100f); }
    private void SetSFXVolume(float volume) { sfx_bus.setVolume(volume / 100f); }

    [Header("SETTINGS")]
    private StepSetting global_volume_setting;
    private StepSetting sfx_volume_setting;
    private StepSetting music_volume_setting;
    private StepSetting ambiance_volume_setting;

    // START & ONDESTROY
    protected virtual void Start()
    {
        register_settings();
        
        StartCoroutine(enable_audio_after_delay(start_delay));
    }
    protected virtual void OnDestroy()
    {
        unregister_settings();
    }
    private IEnumerator enable_audio_after_delay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        can_play = true;
    }



    // ONE SHOT SOUND PLAY
    public void PlaySound(EventReference sound, GameObject player)
    {
        if (!can_play) { return; }
        RuntimeManager.PlayOneShotAttached(sound, player);
    }
    public void Play(string capacity, string skin, GameObject player)
    {
        if (!can_play) { return; }
        if (log_play) { Debug.Log($"(AudioEngine - Play) trying to play '{capacity}' & '{skin}' audio"); }
        // we get the right event ref from the bank
        EventReference event_ref = AudioBank.Instance.GetEventReference(capacity, skin);
        if (event_ref.Equals(default(EventReference))) { return; }
        PlaySound(event_ref, player);
    }
    public void Play(string capacity, Capable capable)
    {
        Play(capacity, capable.Skin, capable.gameObject);
    }

    // SOUND INSTANCE CREATION - gives more control to capacities
    public EventInstance CreateInstance(EventReference event_ref)
    {
        EventInstance instance = RuntimeManager.CreateInstance(event_ref);
        return instance;
    }
    public EventInstance CreateInstance(string capacity, string skin)
    {
        // we get the right event ref from the bank
        EventReference event_ref = AudioBank.Instance.GetEventReference(capacity, skin);
        if (event_ref.Equals(default(EventReference))) { return default; }
        return CreateInstance(event_ref);
    }
    public EventInstance CreateInstance(string capacity, Capable capable) { return CreateInstance(capacity, capable.Skin); }


    // ONE SHOT PLAY UI
    [Header("UI Audio")]
    [SerializeField] private float ui_cooldown = 0.1f; // Cooldown time in seconds to prevent overlapping UI sounds
    private Dictionary<string, EventInstance> ui_instances = new Dictionary<string, EventInstance>();
    private Dictionary<string, float> ui_last_played = new Dictionary<string, float>();
    public void PlayUI(string capa)
    {
        if (!can_play) { return; }
        if (log_play) { Debug.Log($"(AudioEngine - PlayUI) trying to play '{capa}' UI audio"); }

        // we check if we have an ui_instance for this sound
        if (!ui_instances.ContainsKey(capa))
        {
            // if not, we create it and add it to the dictionary
            EventReference event_ref = AudioBank.Instance.GetEventReference(capa, "ui_default");
            if (event_ref.Equals(default(EventReference))) { return; }
            EventInstance instance = RuntimeManager.CreateInstance(event_ref);
            ui_instances.Add(capa, instance);
        }
        
        // we check if we can play the sound (not already playing another ui sound)
        if (capa == "hover" && !can_play_hover()) { return; }

        // we play the sound
        ui_instances[capa].start();
        ui_last_played[capa] = Time.unscaledTime;
    }
    private bool can_play_hover()
    {
        // we check if we are already playing another ui sound
        foreach (KeyValuePair<string, float> pair in ui_last_played)
        {
            if (pair.Key == "hover") { continue; }
            if (Time.unscaledTime - pair.Value < ui_cooldown) // Adjust the time threshold as needed
            {
                if (log_can_play_hover) { Debug.Log($"(AudioEngine - can_play_hover) cannot play hover sound because '{pair.Key}' was played {Time.time - pair.Value} seconds ago"); }
                return false;
            }
        }
        return true;
    }

}