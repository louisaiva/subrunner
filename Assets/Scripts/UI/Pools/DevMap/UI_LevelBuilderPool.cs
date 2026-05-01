using System.Collections;

public class UI_LevelBuilderPool : UI_SlottablePool
{
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