using System.Collections;
using System.Numerics;

public class UI_LevelBuilderPool : UI_SlottablePool, Descriptable
{
    public string Name => "Level Builder";
    public string Description => get_description();
    private string get_description()
    {
        string description = "";
        description += $"level : <b>{LevelBuilder.StaticInstance.TargetedWorld}/{LevelBuilder.StaticInstance.TargetedLevel}</b>\n\n";
        description += $"current tool : <b>{LevelBuilder.StaticInstance.tool_type}</b>\n";
        description += $"current cell : <b>({LevelBuilder.StaticInstance.SelectedCell.x}, {LevelBuilder.StaticInstance.SelectedCell.y})</b>\n";
        return description;
    }

    // ENABLING
    protected override void on_added_to_stack()
    {
        LevelBuilder.StaticInstance?.gameObject.SetActive(true);
    }
    protected override void on_removed_from_stack()
    {
        LevelBuilder.StaticInstance?.gameObject.SetActive(false);
    }
}