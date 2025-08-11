using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UI_Hacking : UI_Pool
{
    public override bool Available {
        get
        {
            if (in_transition) { return false; }
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
        exploitCallback = ctx => HandleExploitInput(ctx.ReadValue<float>());

        base.Start();

        if (log) { Debug.Log("(UI_Hacking) started & callbacks created"); }
    }

    // EXPLOIT
    private void HandleExploitInput(float input)
    {
        if (log) { Debug.Log("(UI_Hacking) hack input received : " + input); }

        // we use the laptop
        Perso.Instance.OnHack();
    }

    // SHOW / HIDE
    protected override async Awaitable show_pool(float duration)
    {
        // we set the callbacks
        exploitAction.performed += exploitCallback;
        await base.show_pool(duration);

        // we set the callbacks & enable HackableNavigator
        Perso.Instance.HackableNavigator.Enable();

        if (log) { Debug.Log("(UI_Hacking) showing pool : navigator enabled & callbacks set"); }
    }
    protected override async Awaitable hide_pool(float duration)
    {
        if (log) { Debug.Log("(UI_Hacking) trying to hide pool"); }

        // we disable navigator
        Perso.Instance.HackableNavigator.Disable();

        if (log) { Debug.Log("(UI_Hacking) navigator disabled"); }

        // we remove the callbacks
        exploitAction.performed -= exploitCallback;

        if (log) { Debug.Log("(UI_Hacking) callbacks removed"); }

        await base.hide_pool(duration);

        if (log) { Debug.Log("(UI_Hacking) hiding pool : navigator disabled & callbacks removed"); }
    }
}