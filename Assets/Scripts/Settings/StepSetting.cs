using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StepSetting", menuName = "Settings/StepSetting", order = 3)]
[Serializable] public class StepSetting : Setting
{
    public int steps = 2;

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
    public int GetClosestStepIndex(float value)
    {
        List<float> possible_values = GetPossibleValues();
        float closest_value = GetClosestStepValue(value);
        return possible_values.IndexOf(closest_value);
    }

    // WE SCROLL THROUGH STEPS

    /// <summary>
    /// this method scrolls until the end and GET STUCK AT THE END (not looping).
    /// For looping, use the ScrollLooping method instead.
    /// </summary>
    /// <param name="amount"></param>
    public void Scroll(int amount = 1)
    {
        List<float> possible_values = GetPossibleValues();
        int current_index = GetClosestStepIndex(Value);
        Value = possible_values[Mathf.Clamp(current_index + amount, 0, possible_values.Count - 1)];
    }
    /// <summary>
    /// this method scrolls until the end and LOOP BACK AT THE BEGGINNING.
    /// For Not looping, use the other method, well i think you got it now
    /// </summary>
    /// <param name="amount"></param>
    public void ScrollLooping(int amount = 1)
    {
        List<float> possible_values = GetPossibleValues();
        int current_index = GetClosestStepIndex(Value);
        int next_index = (current_index + amount) % possible_values.Count;
        Value = possible_values[next_index];
    }

    // CLONING
    public override Setting Clone()
    {
        StepSetting clone = CreateInstance<StepSetting>();
        copy_to(clone);
        return clone;
    }
    protected override void copy_to(Setting target)
    {
        base.copy_to(target);
        StepSetting step_target = (StepSetting)target;
        step_target.steps = steps;
    }
}