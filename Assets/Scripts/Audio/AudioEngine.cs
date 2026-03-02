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

    [Header("Audio buses")]
    public Bus master_bus;
    public Bus sfx_bus;
    public Bus music_bus;
    public Bus ambiance_bus;

    [Header("SETTINGS")]
    private StepSetting global_volume_setting;
    private StepSetting sfx_volume_setting;
    private StepSetting music_volume_setting;
    private StepSetting ambiance_volume_setting;

    // START & ONDESTROY
    protected virtual void Start()
    {
        register_settings();
    }
    protected virtual void OnDestroy()
    {
        unregister_settings();
    }

    // SOUND PLAY
    public void PlaySound(EventReference sound, Vector3 position)
    {
        RuntimeManager.PlayOneShot(sound, position);
    }
    public EventInstance CreateSound(EventReference sound)
    {
        EventInstance instance = RuntimeManager.CreateInstance(sound);
        return instance;
    }


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
    private void SetGlobalVolume(float volume) { master_bus.setVolume(volume/100f); }
    private void SetMusicVolume(float volume) { music_bus.setVolume(volume/100f); }
    private void SetAmbianceVolume(float volume) { ambiance_bus.setVolume(volume/100f); }
    private void SetSFXVolume(float volume) { sfx_bus.setVolume(volume/100f); }
}