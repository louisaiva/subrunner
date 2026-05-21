using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UI_SettingsBuilder : MonoBehaviour
{

    [Header("Transforms")]

    private Dictionary<string, UI_Panel> settings_panels;

    [Header("Colors")]
    private Dictionary<string, Color> category_colors;

    [Header("References")]
    [SerializeField] private Transform panel_bar;

    [Header("Prefabs")]
    [SerializeField] private GameObject toggle_prefab;
    [SerializeField] private GameObject slider_prefab;
    [SerializeField] private GameObject step_slider_prefab;

    [Header("Logs")]
    [SerializeField] private bool log = false;

    public void Init()
    {
        settings_panels = new Dictionary<string, UI_Panel>();
        category_colors = new Dictionary<string, Color>();

        // we grab all the panels
        UI_Panel[] panels = GetComponentsInChildren<UI_Panel>(includeInactive: true);
        for (int i = 0; i < panels.Length; i++)
        {
            settings_panels[panels[i].name] = panels[i];
        }

        // and the colorants
        for (int i = 0; i < panel_bar.childCount; i++)
        {
            Transform child = panel_bar.GetChild(i);
            // get the colorant
            Colorant colorant = child.GetComponent<Colorant>();
            if (colorant == null) { continue; }

            category_colors[colorant.gameObject.name] = colorant.HoverColor;
        }

        // we build settings from the manager
        BuildFromManager();
    }

    // BUILDING
    public void BuildFromManager()
    {
        // grab manager settings
        Dictionary<string, List<Setting>> settings = SettingsManager.Instance.Settings;

        List<string> keys = new List<string>(settings.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            string category = keys[i];
            List<Setting> category_settings = settings[category];
            build_panel(category_settings, category);
            if (log) { Debug.Log($"(UI_SettingsBuilder) Built panel for category: {category}"); }
        }
    }
    public void build_panel(List<Setting> data, string category)
    {
        // get the parent and color
        if (!settings_panels.TryGetValue(category, out UI_Panel panel))
        {
            Debug.LogError($"(UI_SettingsBuilder) Panel not found for settings category: {category}");
            return;
        }
        if (!category_colors.TryGetValue(category, out Color category_color))
        {
            Debug.LogWarning($"(UI_SettingsBuilder) Color not found for settings category: {category}");
            category_color = Color.lightGray;
        }


        // we build settings from the data into the parent
        for (int i = 0; i < data.Count; i++)
        {
            // get the setting
            Setting setting = data[i];

            // instantiate a UI_Slot for this setting and applies things to it
            UI_SettingSlot slot = create_slot_for_setting(setting, panel.transform);
            slot.SetColors(Color.white, category_color);
            slot.SettingName = setting.name;

            // get & set label text
            TextMeshProUGUI label_text = slot.gameObject.transform.Find("label").GetComponent<TextMeshProUGUI>();
            label_text.text = setting.label;
        }
    }

    // LOW LEVEL BUILDING
    private UI_SettingSlot create_slot_for_setting(Setting setting, Transform parent)
    {
        // we create a slot for the setting in the parent
        GameObject prefab_to_instantiate = null;

        // we choose the prefab based on the setting type
        if (setting is StepSetting)
        {
            prefab_to_instantiate = step_slider_prefab;
        }
        else if (setting.show_settings.is_toggle)
        {
            prefab_to_instantiate = toggle_prefab;
        }
        else
        {
            prefab_to_instantiate = slider_prefab;
        }

        // we instantiate the prefab
        GameObject slot_go = Instantiate(prefab_to_instantiate, parent);
        UI_SettingSlot slot = slot_go.GetComponent<UI_SettingSlot>();
        return slot;
    }
}