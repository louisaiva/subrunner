using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Setting", menuName = "Settings/Setting", order = 2)]
[Serializable] public class Setting : ScriptableObject, Descriptable
{
    [SerializeField] protected float _value;
    public virtual float Value
    {
        get { return _value; }
        set
        {
            if (_value == value) { return; }

            _value = value;
            OnValueChanged?.Invoke(_value);
            OnSettingChanged?.Invoke(this);
        }
    }
    public virtual void SetValueWithoutNotify(float value)
    {
        _value = value;
    }

    [Header("Framing")]
    public float min_value;
    public float max_value;

    [Header("Show Settings")]
    public SettingShowSettings show_settings;
    public string label = "";
    public string description = "";
    public string Name { get { return name; } }
    public string Description { get { return description; } }


    public Action<float> OnValueChanged;
    public Action<Setting> OnSettingChanged;

    public float GetPercentage()
    {
        if (max_value - min_value == 0f) { return 0f; }
        return (_value - min_value) / (max_value - min_value);
    }
    public override string ToString()
    {
        // we show toggle
        if (show_settings.is_toggle)
        {
            return (_value >= 0.5f) ? "On" : "Off";
        }

        // we show percentage
        if (show_settings.show_percentage)
        {
            float percentage = GetPercentage() * 100f;
            if (show_settings.show_as_integer)
            {
                return Mathf.RoundToInt(percentage).ToString() + " %";
            }
            else
            {
                return percentage.ToString("F" + show_settings.decimal_places) + " %";
            }
        }

        // we show integer
        if (show_settings.show_as_integer)
        {
            return Mathf.RoundToInt(Value).ToString();
        }

        // we show float
        return Value.ToString("F" + show_settings.decimal_places);
    }
    
    // CLONING
    public virtual Setting Clone()
    {
        Setting clone = CreateInstance<Setting>();
        copy_to(clone);
        return clone;
    }
    protected virtual void copy_to(Setting target)
    {
        target.name = this.name;

        // sets _value, min & max
        target._value = _value;
        target.min_value = min_value;
        target.max_value = max_value;

        // show settings
        SettingShowSettings sss = new SettingShowSettings();
        sss.is_toggle = show_settings.is_toggle;
        sss.show_percentage = show_settings.show_percentage;
        sss.show_as_integer = show_settings.show_as_integer;
        sss.decimal_places = show_settings.decimal_places;
        target.show_settings = sss;

        // label & desc
        target.label = label;
        target.description = description;
    }
}