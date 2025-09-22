using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UI_Hacking : UI_Pool
{
    
    public bool log_enabling = false;

    public override bool Available
    {
        get
        {
            if (in_transition) { return false; }
            if (/* Perso.Instance != null &&  */!Perso.Instance.Alive) { return true; }
            if (UI_LaptopItemSlot.Instance == null || !UI_LaptopItemSlot.Instance.HasLaptop) { return false; }
            return true;
        }
    }

    [Header("Transition parameters")]
    public float final_timescale = 0.5f;
    public float bg_final_alpha = 0.5f;


    [Header("Inputs")]

    // EXPLOIT INPUT
    [SerializeField] private InputActionReference exploitInput;
    private InputAction exploitAction;
    private event Action<InputAction.CallbackContext> exploitCallback;

    // START
    protected override void Start()
    {
        // we get the action & create the callbacks
        exploitAction = InputManager.Instance.GetAction(exploitInput);
        // exploitCallback = ctx => HandleExploitInput(ctx.ReadValue<float>());

        base.Start();

        if (log) { Debug.Log("(UI_Hacking) started & callbacks created"); }
    }

    // EXPLOIT
    /* private void HandleExploitInput(float input)
    {
        if (log) { Debug.Log("(UI_Hacking) hack input received : " + input); }

        // we use the laptop
        Controller.Instance.PIC.OnHack();
    } */

    // SHOW / HIDE
    protected override async Awaitable show_pool(float duration, List<GameObject> dont_show = null)
    {
        if (log_enabling) { Debug.Log("(UI_Hacking) trying to show_pool"); }

        // we set the callbacks
        // exploitAction.performed += exploitCallback;
        await base.show_pool(duration, dont_show);

        if (log_enabling) { Debug.Log("(UI_Hacking) pool showed, trying to enable navigators"); }

        // we set the callbacks & enable HackableNavigator
        Controller.Instance.HackableNavigator.Enable();
        if (log_enabling) { Debug.Log("(UI_Hacking) hackable navigator enabled"); }

        Controller.Instance.ExploitNavigator.Enable();
        if (log_enabling) { Debug.Log("(UI_Hacking) exploit navigator enabled"); }

        if (log_enabling) { Debug.Log("(UI_Hacking) showing pool : navigator enabled & callbacks set"); }
    }
    protected override async Awaitable hide_pool(float duration, List<GameObject> dont_hide = null)
    {
        if (log_enabling) { Debug.Log("(UI_Hacking) trying to hide_pool"); }

        // we disable navigator
        Controller.Instance.HackableNavigator.Disable();
        if (log_enabling) { Debug.Log("(UI_Hacking) hackable navigator disabled"); }
        Controller.Instance.ExploitNavigator.Disable();
        if (log_enabling) { Debug.Log("(UI_Hacking) exploit navigator disabled"); }

        // if (log_enabling) { Debug.Log("(UI_Hacking) navigator disabled"); }

        // we remove the callbacks
        // exploitAction.performed -= exploitCallback;

        // if (log_enabling) { Debug.Log("(UI_Hacking) callbacks removed"); }

        await base.hide_pool(duration, dont_hide);

        if (log_enabling) { Debug.Log("(UI_Hacking) hiding pool : navigator disabled & callbacks removed"); }
    }
}