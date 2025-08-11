#pragma warning disable 4014
using UnityEngine;
using PrimeTween;
using UnityEngine.UI;
using System;

public class Transitioner : MonoBehaviour
{

    [Header("Transition parameters")]
    [SerializeField] protected TransitionType transitype = TransitionType.Fade;
    [SerializeField] protected float hidden_value = 0f;
    [SerializeField] protected float shown_value = 1f;
    [SerializeField] protected Ease ease_show = Ease.Default;
    [SerializeField] protected Ease ease_hide = Ease.Default;
    [SerializeField] protected float default_duration = 1f;
    // public bool Available = true;
    // public bool Shown => get_current_value() >= shown_value;
    // public bool Hidden => get_current_value() <= hidden_value;

    [Header("Transition state")]
    public bool Shown = false;
    public bool Hidden = true;
    public bool Available => (Hidden || Shown) && !(Hidden && Shown);

    [Header("On Enable transition")]
    [SerializeField] protected bool transition_on_enable = true;
    public bool ShouldBeVisibleOnEnable = true;
    protected Action<float> updater;

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // AWAKE
    private void Awake()
    {
        // Initialize the updater based on the transition type
        if (transitype == TransitionType.Fade)
        {
            updater = value => GetComponent<CanvasGroup>().alpha = value;
        }
        else if (transitype == TransitionType.Popup)
        {
            updater = value => GetComponent<RectTransform>().localScale = new Vector3(value, value, 1f);
        }

        // reset initial staets
        if (transitype == TransitionType.Fade)
        {
            GetComponent<CanvasGroup>().alpha = hidden_value;
        }
        else if (transitype == TransitionType.Popup)
        {
            GetComponent<RectTransform>().localScale = new Vector3(hidden_value, hidden_value, 1f);
        }
    }

    // ON ENABLE / DISABLE
    private void OnEnable()
    {
        if (!transition_on_enable) { return; }
        if (!ShouldBeVisibleOnEnable) { return; }
        Show();
    }
    private void OnDisable()
    {
        if (!transition_on_enable) { return; }
        Hide();
    }

    // TRANSITION
    public async Awaitable Show(float duration = default)
    {
        // if (!Available) { return; }
        if (Shown)
        {
            if (log) { Debug.Log($"(Transitioner) {name} is already shown, no need to transition"); }
            return;
        }
        Hidden = false;
        Shown = false;

        // transition
        if (log) { Debug.Log($"(Transitioner) Showing {name} with duration {duration}"); }
        await Transition(true, duration);

        // update states
        if (get_current_value() >= shown_value) { Shown = true; }
        else if (get_current_value() <= hidden_value) { Hidden = true; }
    }
    public async Awaitable Hide(float duration = default)
    {
        // if (!Available) { return; }
        if (Hidden)
        {
            if (log) { Debug.Log($"(Transitioner) {name} is already hidden, no need to transition"); }
            return;
        }
        Hidden = false;
        Shown = false;

        // transition
        if (log) { Debug.Log($"(Transitioner) Hiding {name} with duration {duration}"); }
        await Transition(false, duration);

        // update states
        if (get_current_value() <= hidden_value) { Hidden = true; }
        else if (get_current_value() >= shown_value) { Shown = true; }
    }
    public async Awaitable Transition(bool show, float duration = default)
    {
        // Available = false;
        // Shown = false;
        // Hidden = false;
        if (duration == default) { duration = default_duration; }

        Ease ease = show ? ease_show : ease_hide;

        float start_value = get_current_value();
        float end_value = show ? shown_value : hidden_value;

        if (log)
        {
            Debug.Log($"(Transitioner) Transitioning from {start_value} to {end_value} with duration {duration} and ease {ease}");
        }

        // then we make the transition happen
        await Tween.Custom(start_value, end_value, duration: duration,
                onValueChange: updater, useUnscaledTime: true, ease: ease);

        // Available = true;
        
        /* if (log)
        {
            Debug.Log($"(Transitioner) Transitioning from {start_value} to {end_value} with duration {duration} and ease {ease}");
        } */
    }
    private float get_current_value()
    {
        if (transitype == TransitionType.Fade)
        {
            return GetComponent<CanvasGroup>().alpha;
        }
        else if (transitype == TransitionType.Popup)
        {
            return GetComponent<RectTransform>().localScale.x; // Assuming uniform scaling
        }
        return 0f; // Default case, should not happen
    }
}

public enum TransitionType
{
    Fade,
    Popup
}