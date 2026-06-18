using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_WorldSlot : UI_EventButton, Descriptable
{
    private WorldDataHelper whelper;
    private WorldData wdata => whelper != null ? whelper.world : null;

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI name_text;

    // INITIALIZATION & DESTRUCTION
    public void Initialize(WorldDataHelper world_data)
    {
        this.whelper = world_data;

        // we set the icon sprite and color
        if (world_data == null) { return; }
        
        Sprite icon_sprite = WorldManager.Instance.GetIconSprite(wdata.icon_path, wdata.icon_name);
        if (icon_sprite != null) { btn_icon.sprite = icon_sprite; }
        btn_icon.color = wdata.color;
        this.baseColor = wdata.color;
        name_text.text = wdata.id;
    }

    // CLICK HANDLER
    public void SelectAndLoadWorld()
    {
        WorldManager.Instance.SelectWorld(wdata);
        SceneLoader.Instance.LoadGame();
    }


    // DESCRIPTABLE
    public string Name => wdata != null ? wdata.id : "/!\\ no world data /!\\";
    public string Description => get_description();
    private string get_description()
    {
        if (wdata == null) { return "No world data."; }

        string description = "";
        description += $"<b>subrunner {wdata.game_version}</b>\n".AddColor(get_color_version(wdata.game_version));

        description += "\n\n";
        description += $"modified : ".AddColor(Color.grey) + $"<b>{wdata.last_update_date}</b>\n";
        description += $"created : ".AddColor(Color.grey) + $"<b>{wdata.creation_date}</b>\n";

        description += "\n\n";
        description += $"controller : ".AddColor(Color.grey) + $"<b>{whelper.controller.controlled_capable_id}</b>\n";
        description += "\n\n";
        description += $"intro done : ".AddColor(Color.grey) + (wdata.story_data?.intro_done == true
                    ? "<b>YES</b>".AddColor(Color.green)
                    : "<b>NOPE</b>".AddColor(Color.red))
                    + "\n";
        description += $"met qwin : ".AddColor(Color.grey) + (wdata.story_data?.met_qwin == true
                    ? "<b>YES</b>".AddColor(Color.green)
                    : "<b>NOPE</b>".AddColor(Color.red))
                    + "\n";
        description += $"ate pasta : ".AddColor(Color.grey) + (wdata.story_data?.ate_pasta == true
                    ? "<b>YES</b>".AddColor(Color.green)
                    : "<b>NOPE</b>".AddColor(Color.red))
                    + "\n";

        return description;
    }

    private static Color get_color_version(string version)
    {
        try
        {
            int compare_to_current = AppManager.CompareVersion(version);
            if (compare_to_current == 0) { return Color.green; }
            if (Mathf.Abs(compare_to_current) > 100) { return Color.red; } // if the major version is different, it's a big deal
            if (Mathf.Abs(compare_to_current) > 30) { return Color.orangeRed; } // if the minor version is different, it's a bit of a deal
            if (Mathf.Abs(compare_to_current) > 10) { return Color.orange; } // if the minor version is different, it's a bit of a deal
            return Color.yellow; // world version is newer than the app, might cause issues, better be careful
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error occurred while comparing versions: '{version}' & '{Application.version}' \n{ex.Message}");
            return Color.gray; // default color in case of error
        }
    }
}