#pragma warning disable 4014
using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditorInternal;


[Serializable] public class PoolTransitionSettings
{
    public float Duration = 0.05f; // default duration of the transition
    public bool CanBeHidden = true; // if true, the pool can be hidden when switching to another pool
    public bool CanBeCanceled = false; // if true, the UI_Manager will save the last pool and go back to it with B
    public bool UsePersoInputs = true; // if true, the UI_Manager will activate the inputs.perso when the pool is showed
    public float TimeScale = 1f; // time scale when the pool is showed
    public float BackgroundAlpha = 0f; // alpha of the background when the pool is showed

    public bool StopTime => TimeScale == 0f;
    public bool HasBackground => BackgroundAlpha > 0f;

    public PoolTransitionSettings(float duration = 0.1f,bool can_be_hidden = true, bool can_be_canceled = false, bool use_perso_inputs = true, float time_scale = 1f, float background_alpha = 0f)
    {
        Duration = duration;
        CanBeHidden = can_be_hidden;
        CanBeCanceled = can_be_canceled;
        UsePersoInputs = use_perso_inputs;
        TimeScale = time_scale;
        BackgroundAlpha = background_alpha;
    }
    public static PoolTransitionSettings InGameDefault => new PoolTransitionSettings();
    public static PoolTransitionSettings InMenuDefault => new PoolTransitionSettings(can_be_canceled: true, use_perso_inputs: false, time_scale: 0f, background_alpha: 0.96f);
}

public class UI_Pool : MonoBehaviour
{
    [Header("Pool parameters")]
    public string Reference = "pool";
    public bool Showed = false;
    [SerializeField] protected bool in_transition = false;

    [Header("Transition parameters")]
    public PoolTransitionSettings TransitionSettings = PoolTransitionSettings.InGameDefault;
    public virtual bool Available => !in_transition;


    [Header("Pool navigation parameters")]
    [SerializeField] protected float angle_threshold = 45f;
    [SerializeField] protected float angle_multiplicator = 0f;

    [Header("UI Elements")]
    [SerializeField] protected List<GameObject> ui_elements = new List<GameObject>();
    public List<GameObject> UIElements => ui_elements;

    [Header("Logs")]
    [SerializeField] protected bool log = false;

    // START
    protected virtual void Start()
    {
        // we hide the pool
        hide_instantly();
    }

    // SHOW / HIDE
    public virtual async Awaitable Show(List<GameObject> dont_show = null)
    {
        in_transition = true;

        await show_pool(dont_show);
        in_transition = false;
    }
    public virtual async Awaitable Hide(List<GameObject> dont_hide = null)
    {
        in_transition = true;
        await hide_pool(dont_hide);

        in_transition = false;
    }
    private void hide_instantly()
    {
        float duration = TransitionSettings.Duration;
        TransitionSettings.Duration = 0f;
        hide_pool();
        TransitionSettings.Duration = duration;
        in_transition = false;
    }

    // LOW SHOWING
    protected virtual async Awaitable show_pool(List<GameObject> dont_show = null)
    {
        if (TransitionSettings.UsePersoInputs) { InputManager.Instance.EnablePersoInputs(); }
        await System.Threading.Tasks.Task.Delay((int)(TransitionSettings.Duration * 1000));

        // on affiche tous les éléments
        if (log) { Debug.Log("(UI_Pool) showing pool : " + Reference); }
        foreach (GameObject ui in ui_elements)
        {
            if (dont_show != null && dont_show.Contains(ui)) { continue; }
            ui.SetActive(true);
        }
        Showed = true;

        // on active les perso inputs si on doit les activer
        if (!TransitionSettings.UsePersoInputs) { InputManager.Instance.DisablePersoInputs(); }
    }
    protected virtual async Awaitable hide_pool(List<GameObject> dont_hide = null)
    {
        // on cache tous les éléments du pool
        if (log) { Debug.Log("(UI_Pool) hiding pool : " + Reference + $"(duration : {(int)(TransitionSettings.Duration * 1000)})"); }
        foreach (GameObject ui in ui_elements)
        {
            if (dont_hide != null && dont_hide.Contains(ui)) { continue; }
            ui.SetActive(false);
        }

        Showed = false;

        if (TransitionSettings.Duration > 0f) { await System.Threading.Tasks.Task.Delay((int)(TransitionSettings.Duration * 1000)); }
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