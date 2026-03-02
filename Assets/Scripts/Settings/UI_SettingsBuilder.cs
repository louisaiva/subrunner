using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_SettingsBuilder : MonoBehaviour
{
    private SettingsManager manager;

    [Header("Transforms")]
    private Transform general_parent;
    private Transform gameplay_parent;
    private Transform graphics_parent;
    private Transform audio_parent;
    private Transform ui_parent;
    private Transform controls_parent;

    [Header("Colors")]
    private Color general_color;
    private Color gameplay_color;
    private Color graphics_color;
    private Color audio_color;
    private Color ui_color;
    private Color controls_color;

    [Header("Prefabs")]
    [SerializeField] private GameObject toggle_prefab;
    [SerializeField] private GameObject slider_prefab;
    [SerializeField] private GameObject step_slider_prefab;

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // AWAKE
    public void Start()
    {
        // any initialization logic needed during Awake
        // made a specific method instead of Awake() because i want the SettingsManager to have
        // full control on the builder (don't want the builder to Awake() after SettingsManager.Awake(),
        // otherwise it would break the building)

        // todo i think it would be better to handle this better bcz adding a setting panel is a nightmare now

        manager = SettingsManager.Instance;

        // get the transforms
        general_parent = transform.Find("general");
        gameplay_parent = transform.Find("gameplay");
        graphics_parent = transform.Find("graphics");
        audio_parent = transform.Find("audio");
        ui_parent = transform.Find("ui");
        controls_parent = transform.Find("controls");

        // get the colors from the panel bar
        general_color = transform.Find("panel_bar/general").GetComponent<Colorant>().HoverColor;
        gameplay_color = transform.Find("panel_bar/gameplay").GetComponent<Colorant>().HoverColor;
        graphics_color = transform.Find("panel_bar/graphics").GetComponent<Colorant>().HoverColor;
        audio_color = transform.Find("panel_bar/audio").GetComponent<Colorant>().HoverColor;
        ui_color = transform.Find("panel_bar/ui").GetComponent<Colorant>().HoverColor;
        controls_color = transform.Find("panel_bar/controls").GetComponent<Colorant>().HoverColor;

        // we build settings from the manager
        BuildFromManager();
    }

    // BUILDING
    public void BuildFromManager()
    {
        // build settings from manager's settings
        Dictionary<string, List<Setting>> settings = manager.Settings;

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
        // get the category transform & color
        Transform parent = getCategoryParent(category);
        Color category_color = getCategoryColor(category);

        // we build settings from the data into the parent
        for (int i = 0; i < data.Count; i++)
        {
            // get the setting
            Setting setting = data[i];

            // instantiate a UI_Slot for this setting and applies things to it
            UI_SettingSlot slot = create_slot_for_setting(setting, parent);
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
    private Color getCategoryColor(string category)
    {
        switch (category)
        {
            case "general": return general_color;
            case "gameplay": return gameplay_color;
            case "graphics": return graphics_color;
            case "audio": return audio_color;
            case "ui": return ui_color;
            case "controls": return controls_color;
            default: return Color.white;
        }
    }
    private Transform getCategoryParent(string category)
    {
        switch (category)
        {
            case "general": return general_parent;
            case "gameplay": return gameplay_parent;
            case "graphics": return graphics_parent;
            case "audio": return audio_parent;
            case "ui": return ui_parent;
            case "controls": return controls_parent;
            default: return null;
        }
    }
}