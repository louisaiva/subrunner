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
        description += "\n\n\n";

        description += $"controller : ".AddColor(Color.grey) + $"<b>{whelper.controller.controlled_capable_id}</b>\n";

        /* description += $"<b>Levels</b> : ";

        // we get the levels from the LevelEngine static method since the world is not loaded, which means
        // data.levels_ids is necessary empty
        List<LevelData> levels_data = LevelEngine.LoadWorldLevelsData(world_data.id);
        List<string> levels_ids = levels_data.ConvertAll(level_data => level_data.id);
        if (levels_ids == null || levels_ids.Count == 0)
        {
            description += "no levels :\\\\\\";
            return description;
        }
        description += $"{levels_ids.Count} \n\n";
        foreach (string id in levels_ids)
        {
            description += $"{id}  -\n";
        } */
        return description;
    }

    private static Color get_color_version(string version)
    {
        try
        {
            int compare_to_current = AppManager.CompareVersion(version);
            if (compare_to_current == 0) { return Color.green; }
            if (Mathf.Abs(compare_to_current) > 100) { return Color.red; } // if the major version is different, it's a big deal
            if (Mathf.Abs(compare_to_current) > 10) { return Color.orangeRed; } // if the minor version is different, it's a bit of a deal
            // if (compare_to_current > 0) { return Color.lightGreen; } // the app version is newer than the world, should probably work fine
            return Color.orange; // world version is newer than the app, might cause issues, better be careful
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error occurred while comparing versions: '{version}' & '{Application.version}' \n{ex.Message}");
            return Color.gray; // default color in case of error
        }
    }
}