using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingsSaveData", menuName = "Settings/Settings Save Data", order = 1)]
public class SettingsSaveData : ScriptableObject
{
    [Header("Category Name")]
    public string category_name;

    [Header("Settings Data")]
    public List<Setting> settings = new List<Setting>();

    // CLONING
    public List<Setting> Clone()
    {
        List<Setting> cloned_settings = new List<Setting>();
        for (int i = 0; i < settings.Count; i++)
        {
            Setting original = settings[i];
            Setting clone = original.Clone();
            cloned_settings.Add(clone);
        }
        return cloned_settings;
    }
}