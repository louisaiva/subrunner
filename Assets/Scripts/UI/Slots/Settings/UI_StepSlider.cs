using System.Collections.Generic;
using UnityEngine;


public class UI_StepSlider : UI_Slider
{
    public override void OnSlide(float amount)
    {
        // we get the possibles values
        StepSetting step_setting = SettingsManager.Instance?.GetSetting(SettingName) as StepSetting;
        if (step_setting == null) { return; }
        List<float> step_values = step_setting.GetPossibleValues();
        float current_value = step_setting.GetClosestStepValue(CurrentValue);

        // we check which one we are actually on to
        int current_step_index = step_values.IndexOf(current_value);

        // we check if we want next step or previous
        int next_index = current_step_index + (amount > 0f ? 1 : -1);
        next_index = Mathf.Clamp(next_index, 0, step_values.Count - 1);

        // we set the value
        set_value(step_values[next_index]);
    }
    protected override void set_value(float value)
    {
        // we snap to closest step
        StepSetting step_setting = SettingsManager.Instance?.GetSetting(SettingName) as StepSetting;
        if (step_setting == null) { return; }
        float closest_step_value = step_setting.GetClosestStepValue(value);
        base.set_value(closest_step_value);
    }
}