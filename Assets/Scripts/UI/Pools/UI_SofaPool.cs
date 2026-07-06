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
        /* if (save_game)
        {
            SaveEngine.SaveDynamicWorld();
        } */
    }
}