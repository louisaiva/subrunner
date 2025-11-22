using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StringSetting", menuName = "Settings/StringSetting", order = 4)]
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
            OnStringChanged?.Invoke(ToString());
        }
    }
    public override string ToString()
    {
        int index = Mathf.Clamp(Mathf.RoundToInt(value), 0, possible_values.Count - 1);
        return possible_values[index];
    }


    // CLONING
    public override Setting Clone()
    {
        StringSetting clone = CreateInstance<StringSetting>();
        copy_to(clone);
        return clone;
    }
    protected override void copy_to(Setting target)
    {
        base.copy_to(target);
        StringSetting string_target = (StringSetting)target;
        string_target.possible_values = new List<string>(possible_values);
    }
}