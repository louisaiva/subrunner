using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;


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
    private void enable_audio() { can_play = true; }
    private void disable_audio() { can_play = false; }

    [Header("Audio buses")]
    public Bus master_bus;
    public Bus sfx_bus;
    public Bus music_bus;
    public Bus ambiance_bus;

    [Header("Logs")]
    public bool log_play = false;

    // SETTINGS REGISTERING
    private void register_settings()
    {
        global_volume_setting = SettingsManager.Instance.GetSetting("global_volume") as StepSetting;
        sfx_volume_setting = SettingsManager.Instance.GetSetting("sfx_volume") as StepSetting;
        music_volume_setting = SettingsManager.Instance.GetSetting("music_volume") as StepSetting;
        ambiance_volume_setting = SettingsManager.Instance.GetSetting("ambiance_volume") as StepSetting;

        if (global_volume_setting != null) { global_volume_setting.OnValueChanged += SetGlobalVolume; SetGlobalVolume(global_volume_setting.value); }
        if (sfx_volume_setting != null) { sfx_volume_setting.OnValueChanged += SetSFXVolume; SetSFXVolume(sfx_volume_setting.value); }
        if (music_volume_setting != null) { music_volume_setting.OnValueChanged += SetMusicVolume; SetMusicVolume(music_volume_setting.value); }
        if (ambiance_volume_setting != null) { ambiance_volume_setting.OnValueChanged += SetAmbianceVolume; SetAmbianceVolume(ambiance_volume_setting.value); }
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
        
        Invoke(nameof(enable_audio), start_delay);
    }
    protected virtual void OnDestroy()
    {
        unregister_settings();
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
    public void PlayUI(string capa)
    {
        if (!can_play) { return; }
        if (log_play) { Debug.Log($"(AudioEngine - PlayUI) trying to play '{capa}' UI audio"); }
        // we get the right event ref from the bank
        EventReference event_ref = AudioBank.Instance.GetEventReference(capa, "ui_default");
        if (event_ref.Equals(default(EventReference))) { return; }
        RuntimeManager.PlayOneShot(event_ref);
    }

}