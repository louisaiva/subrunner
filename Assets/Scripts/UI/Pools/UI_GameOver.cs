using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class UI_GameOver : UI_Pool
{
    // [Header("Transition parameters")]
    // public float final_timescale = 0.1f;
    // public float transition_duration = 2f;

    [Header("Perso revive parameters")]
    [SerializeField] private GameObject perso_prefab;
    // [SerializeField] private Transform perso_spawn_point;

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

        // if (perso_spawn_point == null) { Debug.LogError("(UI_GameOver) perso_spawn_point is not assigned! Please assign it in the inspector."); }

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
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f)
    {
        // we set the callbacks
        reviveAction.performed += reviveCallback;

        // we update the text
        oh_no_text.text = "oh n";
        for (int i = 0; i < Perso.deaths; i++)
        {
            oh_no_text.text += "o";
        }

        yield return base.show_coroutine(dont_show, duration_override);
    }

    // ENABLING
    protected override IEnumerator disable_coroutine()
    {
        // we remove the callbacks
        reviveAction.performed -= reviveCallback;

        // we get the spawn point
        Vector3 perso_spawn_point = Vector3.zero;
        if (World.Instance.spawn_point != null)
        {
            perso_spawn_point = World.Instance.spawn_point.position;
        }

        // we instantiate the perso prefab at the spawn point
        // var task = ;
        // yield return new WaitUntil(() => task.completed);

        bool instantiated = false;
        var instantiation = InstantiateAsync(perso_prefab, perso_spawn_point, Quaternion.identity);
        instantiation.completed += (op) => instantiated = true;
        yield return new WaitUntil(() => instantiated);
        GameObject[] perso = instantiation.Result;
        perso[0].name = "perso";
    }


}