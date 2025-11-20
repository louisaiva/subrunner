using UnityEngine;

public class UI_ToggleFeedback : MonoBehaviour
{
    [Header("Toggle Feedback Settings")]
    [SerializeField] private UI_ToggleSetting toggle;
    [SerializeField] private TMPro.TextMeshProUGUI value_text;

    private Setting setting;

    void Start()
    {
        // we get the toggle setting
        setting = SettingsManager.Instance.GetSetting(toggle.SettingName);
        if (setting == null) { return; }
        setting.OnValueChanged += update_text;
        update_text(setting.value);
    }
    private void update_text(float value)
    {
        string text = (value > 0.5f) ? "on" : "off";
        value_text.text = text;
    }
    void OnDestroy()
    {
        if (setting == null) { return; }
        setting.OnValueChanged -= update_text;
    }
}