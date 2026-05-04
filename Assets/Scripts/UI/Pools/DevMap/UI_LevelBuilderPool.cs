
public class UI_LevelBuilderPool : UI_SlottablePool, Descriptable
{
    public string Name => "Level Builder";
    public string Description => get_description();
    private string get_description()
    {
        string description = "";
        description += $"level : <b>{WorldBuilder.StaticTargetedWorld}/{WorldBuilder.LevelBuilder.TargetedLevel}</b>\n\n";
        description += $"current tool : <b>{WorldBuilder.LevelBuilder.tool_type}</b>\n";
        description += $"current cell : <b>({WorldBuilder.LevelBuilder.SelectedCell.x}, {WorldBuilder.LevelBuilder.SelectedCell.y})</b>\n";
        return description;
    }

    // ENABLING
    protected override void on_added_to_stack()
    {
        WorldBuilder.LevelBuilder?.gameObject.SetActive(true);
    }
    protected override void on_removed_from_stack()
    {
        WorldBuilder.LevelBuilder?.gameObject.SetActive(false);
    }
}