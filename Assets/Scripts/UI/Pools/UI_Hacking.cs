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

    // NAVIGATION INPUT
    [SerializeField] private InputActionReference navigationInput;
    private InputAction navigationAction;
    private event Action<InputAction.CallbackContext> navigationCallback;

    [Header("Components")]
    [SerializeField] private HackNavigator navigator;

    // START
    protected override void Start()
    {
        // we check if we have a navigator
        if (navigator == null)
        {
            Debug.LogError("(UI_Hacking) No HackNavigator found on the UI_Hacking component. Please assign one in the inspector.");
            return;
        }

        // we get the action & create the callbacks
        exploitAction = InputManager.Instance.GetAction(exploitInput);
        exploitCallback = ctx => HandleExploitInput(ctx.ReadValue<float>());

        // we get the navigation action & create the callbacks
        navigationAction = InputManager.Instance.GetAction(navigationInput);
        navigationCallback = ctx => navigator.HandleHackNavigationInput(ctx.ReadValue<Vector2>());

        if (log) { Debug.Log("(UI_Hacking) started & callbacks created"); }

        base.Start();
    }

    // EXPLOIT
    private void HandleExploitInput(float input)
    {
        if (log) { Debug.Log("(UI_GameOver) revive input received : " + input); }

        if (input > 0.5f) { return; } // we only handle the input when the value is below 0.5f

        // we find the laptop
        Laptop laptop = Perso.Instance.Laptop;
        if (laptop == null) { return; }

        // we use the laptop
        Perso.Instance.OnHack();
    }

    // SHOW / HIDE
    protected override async Awaitable show_pool(float duration)
    {
        // we set the callbacks
        exploitAction.performed += exploitCallback;
        await base.show_pool(duration);


        // we set the callbacks & enable HackNavigator
        navigationAction.performed += navigationCallback;
        navigator.Enable();
    }
    protected override async Awaitable hide_pool(float duration)
    {
        // we disable navigator
        navigator.Disable();
        navigationAction.performed -= navigationCallback;

        // we remove the callbacks
        exploitAction.performed -= exploitCallback;
        await base.hide_pool(duration);
    }

}