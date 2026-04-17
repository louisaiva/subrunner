using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UI_SofaPool : UI_SlottablePool
{

    [Header("Sofa & Controller")]
    [SerializeField] private SitCapacity sit_capacity;

    public void ShowSofaUI(SitCapacity sit_capacity)
    {
        this.sit_capacity = sit_capacity;

        // then we show the ui_sofa
        UI_Manager.Instance.SwitchTo("sofa");
    }

    // EXITING SOFA (HAPPENS WHEN SWITCHING TO ANOTHER UI_POOL)
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();
        if (sit_capacity == null) { yield break; }

        // we make the player exit the sofa
        sit_capacity.ExitSofa();
        sit_capacity = null;

        yield break;
    }
}