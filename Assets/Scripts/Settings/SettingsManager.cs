using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Factory settings")]
    public SettingsSaveData factory_general;
    public SettingsSaveData factory_gameplay;
    public SettingsSaveData factory_graphics;
    public SettingsSaveData factory_ui;
    public SettingsSaveData factory_controls;

    [Header("Settings")]
    private Dictionary<string, List<Setting>> settings = new Dictionary<string, List<Setting>>();
    public Dictionary<string, List<Setting>> Settings => settings;

    [Header("Logs")]
    public bool log = false;

    // AWAKE
    private void Awake()
    {
        // Singleton & Dontdestroy logic
        if (Instance != null) { Destroy(transform.parent.gameObject); return; } // ensure we destroy the "dont_destroy" object and not only this object
        Instance = this;

        // reset factory settings to generate every settings
        reset_factory_settings();

        // then load local settings values
        LoadLocalSettings();
    }

    // START
    private void Start()
    {
        // on register certains callbacks directement

        // fullscreen
        GetSetting("fullscreen").OnValueChanged += (ctx) => AppManager.Instance.fullscreen(ctx >= 0.5f);
        AppManager.Instance.fullscreen(GetValue("fullscreen") >= 0.5f);

        // skin
        // (GetSetting("skin") as StringSetting).OnStringChanged += (skin) => Perso.Instance.SetSkin(skin);
        // Perso.Instance.SetSkin((GetSetting("skin") as StringSetting).ToString());
    }

    // SETTERS & GETTERS
    public void SetSetting(string settingName, float value)
    {
        // Logic to set the setting based on its name
        if (log) { Debug.Log($"(SettingsManager) Setting {settingName} set to {value}"); }
        Setting setting = GetSetting(settingName);
        if (setting == null) { return; }
        setting.value = value;
    }
    public float GetValue(string settingName)
    {
        // Logic to get the setting value based on its name
        Setting setting = GetSetting(settingName);
        if (setting != null) { return setting.value; }
        if (log) { Debug.LogWarning($"(SettingsManager) Setting {settingName} not found!"); }
        return -1f; // Default value if not found
    }
    public Setting GetSetting(string settingName)
    {
        // Logic to get the setting based on its name (which is a reference actually)
        List<string> panel_keys = new List<string>(settings.Keys);
        List<Setting> panel_settings;
        for (int p = 0; p < panel_keys.Count; p++)
        {
            panel_settings = settings[panel_keys[p]];
            for (int s = 0; s < panel_settings.Count; s++)
            {
                if (panel_settings[s].name != settingName) { continue; }
                return panel_settings[s];
            }
        }
        if (log) { Debug.LogWarning($"(SettingsManager) Setting {settingName} not found!"); }
        return null; // Default value if not found
    }


    // SAVE / LOAD SETTINGS
    public void SaveLocalSettings()
    {
        // convert all values to a Dictionary<string,float>
        Dictionary<string, float> settings_values = new Dictionary<string, float>();

        // we go through each panel
        List<string> panel_keys = new List<string>(settings.Keys);
        List<Setting> panel_settings;
        for (int p = 0; p < panel_keys.Count; p++)
        {
            panel_settings = settings[panel_keys[p]];
            for (int s = 0; s < panel_settings.Count; s++)
            {
                settings_values[panel_settings[s].name] = panel_settings[s].value;
            }
        }

        // we serialize to json
        string json = JsonConvert.SerializeObject(settings_values);

        // we save to Application.persistentDataPath
        if (log) { Debug.Log($"(SettingsManager) Saving local settings to {Application.persistentDataPath}/local_settings.json"); }
        using (FileStream stream = new FileStream(Application.persistentDataPath + "/local_settings.json", FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(json);
            }
        }

        // we save to PlayerPrefs
        // if (log) { Debug.Log("(SettingsManager) Saving local settings from PlayerPrefs"); }
        // PlayerPrefs.SetString("local_settings", json);
    }
    public void LoadLocalSettings()
    {
        string json = "";

        // we get the json string
        // if (!PlayerPrefs.HasKey("local_settings")) { return; }
        // if (log) { Debug.Log("(SettingsManager) Loading local settings from PlayerPrefs"); }
        // json = PlayerPrefs.GetString("local_settings");

        // we load settings from Application.persistentDataPath
        try 
        {
            using (FileStream stream = new FileStream(Application.persistentDataPath + "/local_settings.json", FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                }
            }
            if (log) { Debug.Log($"(SettingsManager) Loaded local settings from {Application.persistentDataPath}/local_settings.json"); }
        }
        catch (Exception e)
        {
            if (log) { Debug.LogWarning($"(SettingsManager) Failed to load local settings in {Application.persistentDataPath}/local_settings.json: {e.Message}"); }
            return;
        }
        if (string.IsNullOrEmpty(json)) { return; }

        // we deserialize to Dictionary<string,float>
        Dictionary<string, float> settings_values = JsonConvert.DeserializeObject<Dictionary<string, float>>(json);
        if (settings_values == null) { return; }

        // we go through each saved setting
        foreach (KeyValuePair<string, float> entry in settings_values)
        {
            if (log) { Debug.Log($"(SettingsManager) Loading setting {entry.Key} with value {entry.Value}"); }

            // we get the setting
            Setting setting = GetSetting(entry.Key);
            if (setting == null) { continue; }

            // checks if the value is in the bounds otherwise it will break things
            if (entry.Value < setting.min_value) { continue; }
            if (entry.Value > setting.max_value) { continue; }

            // we set the value
            setting.value = entry.Value;
        }
    }

    // RESET FACTORY SETTINGS
    public void reset_factory_settings()
    {
        // we clear current settings
        settings.Clear();

        // we build settings from each factory
        settings["general"] = factory_general.Clone();
        settings["gameplay"] = factory_gameplay.Clone();
        settings["graphics"] = factory_graphics.Clone();
        settings["ui"] = factory_ui.Clone();
        settings["controls"] = factory_controls.Clone();
    }
}

// SETTING SHOW SETTINGS
[Serializable] public struct SettingShowSettings
{
    public bool is_toggle; // is a toggle (on/off), if true overries below
    public bool show_percentage; // show as percentage, if true overrides below settings
    public bool show_as_integer; // show as integer, if true overrides below
    public int decimal_places; // number of decimal places to show if not integer
}
