using UnityEngine;

public class UI_HUD : UI_Pool
{
    public UI_Notifier Notifier;
    public UI_ItemBar ItemBar;

    protected override void before_adding_to_stack()
    {
        // check if we have a controller
        if (Controller.Capable == null || Controller.Capable.Inventory == null) { return; }
        ItemBar.AttachToInventory(Controller.Capable.Inventory);
    }
    protected override void after_removed_from_stack()
    {
        // we clear the item bar
        ItemBar.Clear();
    }
}