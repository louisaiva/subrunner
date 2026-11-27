using System;
using UnityEngine;

public class UI_SofaPool : UI_SlottablePool
{

    [Header("Sofa & Controller")]
    [SerializeField] private SitCapacity sit_capacity;

    [Header("UI_CMD")]
    [SerializeField] private UI_Cmd cmd;

    public void ShowSofaUI(SitCapacity sit_capacity, bool save_game = true)
    {
        this.sit_capacity = sit_capacity;

        // then we show the ui_sofa
        UI_Manager.Instance.SwitchTo("sofa");
    }

    protected override void after_added_to_stack()
    {
        cmd.Enable();
    }
    protected override void after_removed_from_stack()
    {
        cmd.Disable();

        // check if we still are on the sofa we exit it
        if (sit_capacity != null && sit_capacity.CurrentSofa != null) { sit_capacity.ExitSofa(); }

        sit_capacity = null;
    }


}