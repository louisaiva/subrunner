using System;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Settings")]
    public List<Setting> settings = new List<Setting>();

    [Header("Logs")]
    public bool log = false;

    // AWAKE
    private void Awake()
    {
        // Singleton & Dontdestroy logic
        if (Instance != null) { Destroy(transform.parent.gameObject); return; } // ensure we destroy the "dont_destroy" object and not only this object
        Instance = this;

        // we try to load local settings
        // todo

        // reset factory settings if saved_file not found
        reset_factory_settings();
    }

    // START
    private void Start()
    {
        // on register certains callbacks directement
        GetSetting("fullscreen").OnValueChanged += (ctx) => AppManager.Instance.fullscreen(ctx >= 0.5f);
        AppManager.Instance.fullscreen(GetValue("fullscreen") >= 0.5f);
    }

    // SETTERS & GETTERS
    public void SetSetting(string settingName, float value)
    {
        // Logic to set the setting based on its name
        if (log) { Debug.Log($"Setting {settingName} set to {value}"); }
        Setting setting = GetSetting(settingName);
        if (setting == null) { return; }
        setting.value = value;
    }
    public float GetValue(string settingName)
    {
        // Logic to get the setting value based on its name
        for (int i = 0; i < settings.Count; i++)
        {
            if (settings[i].name == settingName)
            {
                return settings[i].value;
            }
        }
        if (log) { Debug.LogWarning($"Setting {settingName} not found!"); }
        return -1f; // Default value if not found
    }
    public Setting GetSetting(string settingName)
    {
        // Logic to get the setting based on its name
        for (int i = 0; i < settings.Count; i++)
        {
            if (settings[i].name == settingName)
            {
                return settings[i];
            }
        }
        if (log) { Debug.LogWarning($"Setting {settingName} not found!"); }
        return null; // Default value if not found
    }

    // FACTORY SETTINGS
    private void reset_factory_settings()
    {
        if (log) { Debug.Log("(SettingsManager) resetting to factory settings"); }

        // graphics
        SetSetting("fullscreen", 1f); // fullscreen

        // screenshake & chroma screenshake
        SetSetting("screenshake_global_setting", 0.5f);
        SetSetting("screenshake_chroma_threshold", 0.6f);
        SetSetting("screenshake_chroma_duration_factor", 16f);
    }
}

[Serializable] public class Setting
{
    public string name;
    [SerializeField] private float _value;
    public float value 
    { 
        get { return _value; }
        set 
        {
            if (_value == value) { return; }
            
            _value = value;
            OnValueChanged?.Invoke(_value);
        }
    }

    [Header("Framing")]
    public float min_value;
    public float max_value;

    public Action<float> OnValueChanged;

    public Setting(string name, float value)
    {
        this.name = name;
        this.value = value;
    }
}