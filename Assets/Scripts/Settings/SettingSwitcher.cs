using UnityEngine;

/// <summary>
/// simple class that enables/disables itself based on a setting value
/// </summary>
public class SettingSwitcher : MonoBehaviour
{
    [SerializeField] private string setting_name;
    [SerializeField] private float value_to_enable = 1f; // if above or equal, enable, else disable

    private void Start()
    {
        SettingsManager.Instance.RegisterCallback(setting_name, OnSettingChanged);
    }

    private void OnDestroy()
    {
        SettingsManager.Instance?.UnregisterCallback(setting_name, OnSettingChanged);
    }

    private void OnSettingChanged(Setting setting)
    {
        if (setting.Value >= value_to_enable)
        {
            if (!gameObject.activeSelf) { gameObject.SetActive(true); }
        }
        else
        {
            if (gameObject.activeSelf) { gameObject.SetActive(false); }
        }
    }
}