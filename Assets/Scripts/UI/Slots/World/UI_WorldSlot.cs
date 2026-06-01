using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_WorldSlot : UI_EventButton, Descriptable
{
    private WorldData world_data;

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI name_text;

    // INITIALIZATION & DESTRUCTION
    public void Initialize(WorldData world_data)
    {
        this.world_data = world_data;

        // we set the icon sprite and color
        if (world_data == null) { return; }
        
        Sprite icon_sprite = WorldManager.Instance.GetIconSprite(world_data.icon_path, world_data.icon_name);
        if (icon_sprite != null) { btn_icon.sprite = icon_sprite; }
        btn_icon.color = world_data.color;
        this.baseColor = world_data.color;
        name_text.text = world_data.id;
    }

    // CLICK HANDLER
    public void SelectAndLoadWorld()
    {
        WorldManager.Instance.SelectWorld(world_data);
        SceneLoader.Instance.LoadGame();
    }


    // DESCRIPTABLE
    public string Name => world_data != null ? world_data.id : "/!\\ no world data /!\\";
    public string Description => get_description();
    private string get_description()
    {
        if (world_data == null) { return "No world data."; }

        string description = "";
        description += $"<b>subrunner {world_data.game_version}</b>\n";
        description += "\n\n";
        description += $"modified : <b>{world_data.last_update_date}</b>\n";
        description += $"created : <b>{world_data.creation_date}</b>\n";
        description += "\n\n\n";
        description += $"<b>Levels</b> : ";

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
        }
        return description;
    }


}