using TMPro;
using UnityEngine;

public class UI_LevelSlot : UI_EventButton, Descriptable
{
    private string world_id;
    private LevelData level_data;

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI name_text;

    // INITIALIZATION & DESTRUCTION
    public void Initialize(string world_id, LevelData lvl_data)
    {
        this.world_id = world_id;
        this.level_data = lvl_data;

        // we set the level name
        if (lvl_data == null) { return; }
        name_text.text = lvl_data.id;
    }

    // CLICK HANDLER
    public void EditLevel()
    {
        LevelBuilder.StaticInstance?.EditLevel(world_id, level_data.id);
        UI_Manager.Instance.StackPool("dev_level_builder");
    }


    // DESCRIPTABLE
    public string Name => level_data != null ? level_data.id : "/!\\ no level data /!\\";
    public string Description => get_description();
    private string get_description()
    {
        if (level_data == null) { return "No level data."; }

        string description = $"<b>Rooms</b> : ";
        if (level_data.rooms_ids == null || level_data.rooms_ids.Count == 0)
        {
            description += "no rooms :\\\\\\";
            return description;
        }
        description += $"{level_data.rooms_ids.Count} \n\n";
        foreach (string id in level_data.rooms_ids)
        {
            description += $"{id}  -\n";
        }
        return description;
    }
}