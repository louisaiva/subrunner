using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_GameOver : UI_Pool
{
    [Header("Transition parameters")]
    public float final_timescale = 0.1f;
    public float transition_duration = 2f;

    [Header("Perso revive parameters")]
    [SerializeField] private GameObject perso_prefab;
    [SerializeField] private Transform perso_spawn_point;

    [Header("Inputs")]
    [SerializeField] private InputActionReference reviveInput;
    private InputAction reviveAction;
    private event Action<InputAction.CallbackContext> reviveCallback; // revive Callback is for reviving items when inside a big inventory -> X

    // START
    protected void Start()
    {
        // we create the callback
        reviveAction = InputManager.Instance.GetAction(reviveInput);
        reviveCallback = ctx => HandleReviveInput(ctx.ReadValue<float>());

        if (log) { Debug.Log("(UI_GameOver) started & callbacks created"); }
    }

    // REVIVE
    private void HandleReviveInput(float input)
    {
        if (log) { Debug.Log("(UI_GameOver) revive input received : " + input); }

        if (input > 0.5f) { return; } // we only handle the input when the value is below 0.5f

        // we switch to hud
        UI_Manager.Instance.SwitchTo("hud");
    }

    protected override async Awaitable show_pool(float duration)
    {
        // we set the callbacks
        reviveAction.performed += reviveCallback;

        await base.show_pool(duration);
    }

    protected override async Awaitable hide_pool(float duration)
    {
        // we remove the callbacks
        reviveAction.performed -= reviveCallback;

        // we instantiate the perso prefab at the spawn point
        GameObject[] perso = await InstantiateAsync(perso_prefab, perso_spawn_point.position, Quaternion.identity);
        perso[0].name = "perso";

        await base.hide_pool(duration);
    }

}