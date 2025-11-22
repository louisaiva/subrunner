using UnityEngine;

public class UI_ToggleFeedback : MonoBehaviour
{
    [Header("Toggle Feedback Settings")]
    private UI_ToggleSetting toggle;
    private TMPro.TextMeshProUGUI value_text;

    private Setting setting;

    private void Start()
    {
        // we get the setting slot & the tmp
        toggle = GetComponentInParent<UI_ToggleSetting>(includeInactive: true);
        value_text = GetComponent<TMPro.TextMeshProUGUI>();

        // we get the toggle setting
        setting = SettingsManager.Instance.GetSetting(toggle.SettingName);
        if (setting == null) { return; }
        setting.OnValueChanged += update_text;
        update_text(setting.value);
    }
    private void Update()
    {
        if (setting == null) { return; }
        update_text(setting.value);
    }
    private void update_text(float value)
    {
        string text = (value > 0.5f) ? "on" : "off";
        value_text.text = text;
    }
    private void OnDestroy()
    {
        if (setting == null) { return; }
        setting.OnValueChanged -= update_text;
    }
}