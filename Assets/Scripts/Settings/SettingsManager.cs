using System;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Settings")]
    public List<Setting> settings = new List<Setting>();
    public List<StepSetting> steps = new List<StepSetting>();
    public List<StringSetting> strings = new List<StringSetting>();

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

        // fullscreen
        GetSetting("fullscreen").OnValueChanged += (ctx) => AppManager.Instance.fullscreen(ctx >= 0.5f);
        AppManager.Instance.fullscreen(GetValue("fullscreen") >= 0.5f);

        // skin
        // (GetSetting("skin") as StringSetting).OnStringChanged += (skin) => Perso.Instance.SetSkin(skin);
        // Perso.Instance.SetSkin((GetSetting("skin") as StringSetting).GetStringValue());
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
        // Logic to get the setting based on its name
        for (int i = 0; i < settings.Count; i++)
        {
            if (settings[i].name != settingName) { continue; }
            return settings[i];
        }
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i].name != settingName) { continue; }
            return steps[i];
        }
        for (int i = 0; i < strings.Count; i++)
        {
            if (strings[i].name != settingName) { continue; }
            return strings[i];
        }
        if (log) { Debug.LogWarning($"(SettingsManager) Setting {settingName} not found!"); }
        return null; // Default value if not found
    }

    // FACTORY SETTINGS
    private void reset_factory_settings()
    {
        if (log) { Debug.Log("(SettingsManager) resetting to factory settings"); }


        // gameplay
        SetSetting("skin", 0f); // skin
        SetSetting("ghost_mode", 0f); // ghost

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
    [SerializeField] protected float _value;
    public virtual float value 
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

    public float GetPercentage()
    {
        if (max_value - min_value == 0f) { return 0f; }
        return (_value - min_value) / (max_value - min_value);
    }
}

[Serializable] public class StepSetting : Setting
{
    public int steps = 2;
    public StepSetting(string name, float value, int steps) : base(name, value)
    {
        this.steps = steps;
    }

    public List<float> GetPossibleValues()
    {
        List<float> values = new List<float>();
        if (steps < 2) 
        { 
            values.Add(min_value);
            return values;
        }

        float step_size = (max_value - min_value) / (steps - 1);
        for (int i = 0; i < steps; i++)
        {
            values.Add(min_value + i * step_size);
        }
        return values;
    }
    public float GetClosestStepValue(float value)
    {
        List<float> possible_values = GetPossibleValues();
        float closest_value = possible_values[0];
        float closest_distance = Mathf.Abs(value - closest_value);

        for (int i = 1; i < possible_values.Count; i++)
        {
            float distance = Mathf.Abs(value - possible_values[i]);
            if (distance < closest_distance)
            {
                closest_distance = distance;
                closest_value = possible_values[i];
            }
        }

        return closest_value;
    }

}

[Serializable] public class StringSetting : StepSetting
{
    public List<string> possible_values = new List<string>();
    public Action<string> OnStringChanged;

    public override float value
    {
        get { return _value; }
        set
        {
            if (_value == value) { return; }

            _value = value;
            OnValueChanged?.Invoke(_value);
            OnStringChanged?.Invoke(GetStringValue());
        }
    }

    public StringSetting(string name, List<string> values) : base(name, 0f, values.Count)
    {
        this.possible_values = values;
        
        // we restrict the string setting to work with ints so we set the min & max value accordingly
        min_value = 0f;
        max_value = values.Count - 1;
    }
    public string GetStringValue()
    {
        int index = Mathf.Clamp(Mathf.RoundToInt(value), 0, possible_values.Count - 1);
        return possible_values[index];
    }
}