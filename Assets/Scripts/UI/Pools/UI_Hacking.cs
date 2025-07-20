using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class UI_Hacking : UI_Pool
{
    [Header("Transition parameters")]
    public float final_timescale = 0.5f;
    public float bg_final_alpha = 0.5f;

    /* [Header("Inputs")]
    [SerializeField] private InputActionReference reviveInput;
    private InputAction reviveAction;
    private event Action<InputAction.CallbackContext> reviveCallback; */ // revive Callback is for reviving items when inside a big inventory -> X

    // REVIVE
    /* private void HandleReviveInput(float input)
    {
        if (log) { Debug.Log("(UI_GameOver) revive input received : " + input); }

        if (input > 0.5f) { return; } // we only handle the input when the value is below 0.5f

        // we switch to hud
        UI_Manager.Instance.SwitchTo("hud");
    } */

    /* protected override async Awaitable show_pool(float duration)
    {
        // we set the callbacks
        reviveAction.performed += reviveCallback;

        // we update the text
        oh_no_text.text = "oh n";
        for (int i = 0; i < Perso.deaths; i++)
        {
            oh_no_text.text += "o";
        }

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
    } */

}