using UnityEngine;

public class UI_SliderFeedback : MonoBehaviour
{
    [Header("Slider Feedback Settings")]
    [SerializeField] private UI_Slider slider;
    [SerializeField] private TMPro.TextMeshProUGUI value_text;

    [Header("Value showing Settings")]
    [SerializeField] private bool show_percentage = false;
    [SerializeField] private bool show_as_integer = true;
    [SerializeField] private int decimal_places = 2;
    [SerializeField] private string suffix = "";

    private Setting setting;

    void Start()
    {
        // we get the slider setting
        setting = SettingsManager.Instance.GetSetting(slider.SettingName);
        if (setting == null) { return; }
        setting.OnValueChanged += update_text;
        update_text(setting.value);
    }
    private void update_text(float value)
    {
        string text = "";
        if (show_percentage)
        {
            float percentage = slider.CurrentPercentage * 100f;
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
    void OnDestroy()
    {
        if (setting == null) { return; }
        setting.OnValueChanged -= update_text;
    }
}