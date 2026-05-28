using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class UI_GameOver : UI_Pool
{
    
    [Header("Perso revive parameters")]
    [SerializeField] private GameObject perso_prefab;

    [Header("Inputs")]
    [SerializeField] private InputActionReference reviveInput;
    private InputAction reviveAction;
    private event Action<InputAction.CallbackContext> reviveCallback; // revive Callback is for reviving items when inside a big inventory -> X

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI oh_no_text;
    
    // START
    private void Start()
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
        UI_Manager.Instance.SwitchToHUD(force:true);
    }


    // SHOWING
    protected override void before_adding_to_stack()
    {
        // we update the text
        oh_no_text.text = "oh n";
        for (int i = 0; i < Perso.Deaths; i++)
        {
            oh_no_text.text += "o";
        }
    }
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        // we set the callbacks
        reviveAction.performed += reviveCallback;

        yield return base.show_coroutine(dont_show, duration_override);
    }

    // DISABLING
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();

        // we remove the callbacks
        reviveAction.performed -= reviveCallback;

        // we respawn the perso
        if (Controller.LazyInstance == null) { yield break; } // if there is no controller, we do nothing
        Controller.LazyInstance.RespawnPerso();


        // we get the spawn point
        /* Vector3 perso_spawn_point = Vector3.zero;
        /* if (World2.Instance.spawn_point != null)
        {
            perso_spawn_point = World2.Instance.spawn_point.position;
        }  // todo update this with respawn

        // we instantiate the perso prefab at the spawn point
        bool instantiated = false;
        var instantiation = InstantiateAsync(perso_prefab, perso_spawn_point, Quaternion.identity);
        instantiation.completed += (op) => instantiated = true;
        yield return new WaitUntil(() => instantiated);
        GameObject[] perso = instantiation.Result;
        perso[0].name = "bob"; */
    }

}