using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UI_Hacking : UI_Pool
{
    // public bool log_enabling = false;

    /* public override bool Available
    {
        get
        {
            if (in_transition) { return false; }
            if (!Perso.Instance.Alive) { return false; }
            if (UI_LaptopItemSlot.Instance == null || !UI_LaptopItemSlot.Instance.HasLaptop) { return false; }
            return true;
        }
    } */

    // [Header("Transition parameters")]
    // public float final_timescale = 0.5f;
    // public float bg_final_alpha = 0.5f;

    // START
    private void Start()
    {
        // on met le callback de pour afficher ui_hacking
        Perso.Instance.OnDeviceChanged += HandlePersoDeviceChanged;
    }

    // ON PERSO DEVICE CHANGED
    private void HandlePersoDeviceChanged(Device new_device)
    {
        if (new_device == null)
        {
            // on cache hacking
            UI_Manager.Instance.UnstackFromHUD("hacking");
            return;
        }

        // on affiche hacking
        UI_Manager.Instance.StackOnHUD("hacking");
    }


    // SHOW / HIDE
    /* protected override IEnumerator show_coroutine(List<GameObject> dont_show = null,float duration_override = -1f)
    {
        if (log_enabling) { Debug.Log("(UI_Hacking) trying to show_pool"); }

        yield return base.show_coroutine(dont_show, duration_override);

        if (log_enabling) { Debug.Log("(UI_Hacking) pool showed, trying to enable navigators"); }

        // we enable HackableNavigator
        Controller.Instance.HackableNavigator.Enable();
        if (log_enabling) { Debug.Log("(UI_Hacking) hackable navigator enabled"); }

        // Controller.Instance.ExploitNavigator.Enable();
        if (log_enabling) { Debug.Log("(UI_Hacking) exploit navigator enabled"); }

        if (log_enabling) { Debug.Log("(UI_Hacking) showing pool : navigator enabled & callbacks set"); }
    }
    protected override IEnumerator hide_coroutine(List<GameObject> dont_hide = null, float duration_override = -1f)
    {
        if (log_enabling) { Debug.Log("(UI_Hacking) trying to hide_pool"); }

        // we disable navigator
        Controller.Instance.HackableNavigator.Disable();
        if (log_enabling) { Debug.Log("(UI_Hacking) hackable navigator disabled"); }
        // Controller.Instance.ExploitNavigator.Disable();
        if (log_enabling) { Debug.Log("(UI_Hacking) exploit navigator disabled"); }

        // if (log_enabling) { Debug.Log("(UI_Hacking) navigator disabled"); }

        yield return base.hide_coroutine(dont_hide, duration_override);

        if (log_enabling) { Debug.Log("(UI_Hacking) hiding pool : navigator disabled & callbacks removed"); }
    } */


    // ENABLING
    /* protected override IEnumerator enable_coroutine()
    {
        // we enable HackableNavigator
        Controller.Instance.HackableNavigator.Enable();
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        // we disable navigator
        Controller.Instance.HackableNavigator.Disable();
        yield break;
    } */

}