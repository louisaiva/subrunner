using UnityEngine;

public class UI_HUD : UI_Pool
{
    public UI_Notifier Notifier;
    public UI_ItemBar ItemBar;

    [Header("Color sweepers")]
    public UI_GraphicColorSweeper HealthBarSweeper;
    public UI_GraphicColorSweeper XP_BarSweeper;
    public UI_GraphicColorSweeper XP_IconSweeper;

    protected override void before_showing()
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

    // ENTRY POINTS
    public void PersoTookDamage() { HealthBarSweeper.Run(); }
    public void PersoHealed() { HealthBarSweeper.Run(); }
    public void PersoGrabbedXP() { XP_BarSweeper.Run(); }
    public void PersoLeveledUP(int level, int upgrade_points)
    {
        // we run the level up color sweep
        XP_IconSweeper.Run();

        // todo : here we can check the upgrade points and replace the image of "exp" with a tmp text with the nb of upgr.
    }
    public void PersoReleasedUP(int up_waiting)
    {
        // if we have no more upgrade waiting, we stop the color sweep
        if (up_waiting <= 0) { XP_IconSweeper.Stop(); }
    }
}