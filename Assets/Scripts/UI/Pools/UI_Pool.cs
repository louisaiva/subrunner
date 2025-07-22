#pragma warning disable 4014
using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UI_Pool : MonoBehaviour
{
    [Header("Pool paramaters")]
    public string Reference = "pool";
    public bool Showed = false;
    private bool in_transition = false;
    public bool CanBeHidden = true; // if true, the pool can be hidden when switching to another pool
    public bool CanBeCanceled = false; // if true, the UI_Manager will switch to hud when pressed & released
    public bool UsePersoInputs = true; // if true, the UI_Manager will activate the inputs.perso when the pool is showed
    public bool StopTime = true;
    public bool HasBackground = true; // if true, the pool has a background effect
    public bool Available => !in_transition;


    [Header("Pool navigation parameters")]
    [SerializeField] protected float angle_threshold = 45f;
    [SerializeField] protected float angle_multiplicator = 0f;

    [Header("UI Elements")]
    // [SerializeField] protected Tween bg_tween;
    [SerializeField] protected List<GameObject> ui_elements = new List<GameObject>();

    [Header("Logs")]
    [SerializeField] protected bool log = false;

    // START
    protected virtual void Start()
    {
        // we get the inputs
        Hide(0f);
    }

    // SHOW / HIDE
    public virtual async Awaitable Show(float duration)
    {
        in_transition = true;

        await show_pool(duration);
        in_transition = false;
    }
    public virtual async Awaitable Hide(float duration)
    {
        in_transition = true;
        await hide_pool(duration);
        // await System.Threading.Tasks.Task.Delay((int)(duration * 1000));

        in_transition = false;
    }

    // LOW SHOWING
    protected virtual async Awaitable show_pool(float duration)
    {
        await System.Threading.Tasks.Task.Delay((int)(duration * 1000));

        // on affiche tous les éléments
        if (log) { Debug.Log("(UI_Pool) showing pool : " + Reference); }
        foreach (GameObject ui in ui_elements) { ui.SetActive(true); }
        Showed = true;

        // s'il a une activate action, on désactive les inputs.perso
        if (UsePersoInputs) { InputManager.Instance.inputs.perso.Enable(); }
        else { InputManager.Instance.inputs.perso.Disable(); }
    }
    protected virtual async Awaitable hide_pool(float duration)
    {
        // on cache tous les éléments du pool
        if (log) { Debug.Log("(UI_Pool) hiding pool : " + Reference); }
        foreach (GameObject ui in ui_elements)
        {
            ui.SetActive(false);
        }

        Showed = false;

        await System.Threading.Tasks.Task.Delay((int)(duration * 1000));
    }

    // REGISTER ELEMENTS
    public void RegisterToPool(GameObject ui_element)
    {
        // if the gameobject is null, we return
        if (ui_element == null) { return; }

        // we check if the element is already in the pool
        else if (ui_elements.Contains(ui_element))
        {
            if (log) { Debug.LogWarning("(UI_Manager) " + ui_element.name + " tried to register to a pool it's already in : " + Reference); }
            return;
        }

        if (log) { Debug.Log("(UI_Manager) " + ui_element.name + " just registered to pool : " + Reference); }

        // on ajoute l'élément au pool
        ui_elements.Add(ui_element);

        // we show/hide the element if the pool is showed
        ui_element.SetActive(Showed);
    }
    public void QuitPool(GameObject ui_element)
    {
        // if the gameobject is null, we return
        if (ui_element == null) { return; }

        // we check if the element is already in the pool
        else if (!ui_elements.Contains(ui_element))
        {
            if (log) { Debug.LogWarning("(UI_Manager) " + ui_element.name + " tried to quit a pool it's not in : " + Reference); }
            return;
        }

        if (log) { Debug.Log("(UI_Manager) " + ui_element.name + " just quit pool : " + Reference); }

        // on enlève l'élément du pool
        ui_elements.Remove(ui_element);
    }
}