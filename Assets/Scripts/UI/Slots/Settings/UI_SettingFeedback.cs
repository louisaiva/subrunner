using UnityEngine;

public class UI_SettingFeedback : MonoBehaviour
{
    [Header("Slider Feedback Settings")]
    private UI_SettingSlot setting_slot;
    private TMPro.TextMeshProUGUI value_text;

    [Header("Value showing Settings")]
    [SerializeField] private bool is_string_value = false;
    [SerializeField] private bool show_percentage = false;
    [SerializeField] private bool show_as_integer = true;
    [SerializeField] private int decimal_places = 2;
    [SerializeField] private string suffix = "";

    private Setting setting;

    private void Start()
    {
        // we get the setting slot & the tmp
        setting_slot = GetComponentInParent<UI_SettingSlot>(includeInactive: true);
        value_text = GetComponent<TMPro.TextMeshProUGUI>();

        // we get the slider setting
        setting = SettingsManager.Instance.GetSetting(setting_slot.SettingName);
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
        string text = "";
        if (is_string_value)
        {
            StringSetting string_setting = setting as StringSetting;
            if (string_setting == null) { return; }
            text = string_setting.GetStringValue();
            value_text.text = text;
            return;
        }
        if (show_percentage)
        {
            float percentage = setting.GetPercentage() * 100f;
            if (show_as_integer)
            {
                text = Mathf.RoundToInt(percentage).ToString() + " %";
            }
            else
            {
                text = percentage.ToString("F" + decimal_places) + " %";
            }
        }
        else
        {
            if (show_as_integer)
            {
                text = Mathf.RoundToInt(value).ToString();
            }
            else
            {
                text = value.ToString("F" + decimal_places);
            }
        }
        text += suffix;
        value_text.text = text;
    }
    private void OnDestroy()
    {
        if (setting == null) { return; }
        setting.OnValueChanged -= update_text;
    }
}