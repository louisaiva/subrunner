using System;
using UnityEngine;

public class UI_SofaPool : UI_SlottablePool
{

    [Header("Sofa & Controller")]
    [SerializeField] private SitCapacity sit_capacity;

    public void ShowSofaUI(SitCapacity sit_capacity, bool save_game = true)
    {
        this.sit_capacity = sit_capacity;

        // then we show the ui_sofa
        UI_Manager.Instance.StackPool("sofa");

        // we save the game if needed
        if (save_game) { SaveEngine.SaveDynamicWorld(); }
    }

    // EXITING SOFA (HAPPENS WHEN SWITCHING TO ANOTHER UI_POOL)
    protected override void after_removed_from_stack()
    {
        base.after_removed_from_stack();
        if (sit_capacity == null) { return; }

        // we make the player exit the sofa
        sit_capacity.ExitSofa();
        sit_capacity = null;
    }
}