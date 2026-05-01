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
        description += $"<b>Created :</b> {world_data.creation_date}\n";
        description += $"<b>Modified :</b> {world_data.last_update_date}\n";
        description += "\n\n\n";
        description += $"<b>Levels</b> : ";
        if (world_data.levels_ids == null || world_data.levels_ids.Count == 0)
        {
            description += "no levels :\\\\\\";
            return description;
        }
        description += $"{world_data.levels_ids.Count} \n\n";
        foreach (string id in world_data.levels_ids)
        {
            description += $"{id}  -\n";
        }
        return description;
    }


}