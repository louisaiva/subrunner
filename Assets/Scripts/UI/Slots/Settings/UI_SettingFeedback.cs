using System;
using UnityEngine;

public class UI_SettingFeedback : MonoBehaviour
{
    [Header("Slider Feedback Settings")]
    private UI_SettingSlot setting_slot;
    private TMPro.TextMeshProUGUI value_text;
    private Action<float> update_callback;
    private Setting setting;

    // START
    private void Start()
    {
        // we get the setting slot & the tmp
        setting_slot = GetComponentInParent<UI_SettingSlot>(includeInactive: true);
        value_text = GetComponent<TMPro.TextMeshProUGUI>();

        // we get the slider setting
        setting = SettingsManager.Instance.GetSetting(setting_slot.SettingName);
        if (setting == null) { return; }
        update_callback = (float _) => Update();
        setting.OnValueChanged += update_callback;
        Update();
    }
    private void OnDestroy()
    {
        if (setting == null) { return; }
        setting.OnValueChanged -= update_callback;
    }
    
    // UPDATE
    private void Update()
    {
        if (setting == null) { return; }
        value_text.text = setting.ToString();
    }
}
