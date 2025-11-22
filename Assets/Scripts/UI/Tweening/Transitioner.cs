#pragma warning disable 4014
using UnityEngine;
using PrimeTween;
using UnityEngine.UI;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class Transitioner : MonoBehaviour
{

    [Header("Transition parameters")]
    [SerializeField] protected TransitionType transitype = TransitionType.Fade;
    [SerializeField] protected float hidden_value = 0f;
    [SerializeField] protected float shown_value = 1f;
    [SerializeField] protected Ease ease_show = Ease.Default;
    [SerializeField] protected Ease ease_hide = Ease.Default;
    [SerializeField] protected float default_duration = 1f;
    [SerializeField] protected bool unscaled_time = true;
    [SerializeField] protected bool reset_on_awake = true;


    [Header("On Enable transition")]
    [SerializeField] protected bool transition_on_enable = true;
    public bool ShouldBeVisibleOnEnable = true;
    protected Action<float> updater;


    [Header("Logs")]
    [SerializeField] private bool log = false;

    // AWAKE
    private void Awake()
    {
        init_updater();

        if (!reset_on_awake) { return; }

        // reset initial states
        if (transitype == TransitionType.Fade)
        {
            GetComponent<CanvasGroup>().alpha = hidden_value;
        }
        else if (transitype == TransitionType.Popup)
        {
            GetComponent<RectTransform>().localScale = new Vector3(hidden_value, hidden_value, 1f);
        }
    }
    private void init_updater()
    {
        if (transitype == TransitionType.Fade)
        {
            updater = value => GetComponent<CanvasGroup>().alpha = value;
        }
        else if (transitype == TransitionType.Popup)
        {
            updater = value => GetComponent<RectTransform>().localScale = new Vector3(value, value, 1f);
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

    // SHOW / HIDE
    public async Awaitable Show(float duration = -99f)
    {
        if (Shown && !Tweening)
        {
            if (log) { Debug.Log($"(Transitioner) {name} is already shown, no need to transition"); }
            return;
        }

        // transition
        if (log) { Debug.Log($"(Transitioner) Showing {name} with duration {duration}"); }
        await transition(true, duration);
    }
    public async Awaitable Hide(float duration = -99f)
    {
        if (Hidden && !Tweening)
        {
            if (log) { Debug.Log($"(Transitioner) {name} is already hidden, no need to transition"); }
            return;
        }

        // transition
        if (log) { Debug.Log($"(Transitioner) Hiding {name} with duration {duration}"); }
        await transition(false, duration);
    }
    public bool Shown => get_current_value() >= shown_value;
    public bool Hidden => get_current_value() <= hidden_value;
    public bool Transitioning => !Shown && !Hidden;
    public bool Tweening => tween != null && tween.Value.isAlive;

    // TWEENING
    private Tween? tween = null;
    private async Awaitable transition(bool show, float duration = -99f)
    {
        if (duration < 0f) { duration = default_duration; }

        Ease ease = show ? ease_show : ease_hide;

        float start_value = get_current_value();
        float end_value = show ? shown_value : hidden_value;

        if (log) { Debug.Log($"(Transitioner) {name} Transitioning from {start_value} to {end_value} with duration {duration} and ease {ease}"); }

        // if we are already transitioning we stop the current tween
        if (tween != null && tween.Value.isAlive)
        {
            if (log) { Debug.Log($"(Transitioner) {name} is already transitioning, stopping current tween"); }
            tween.Value.Stop();
        }

        // checks if we have an updater
        if (updater == null)
        {
            if (log) { Debug.Log($"(Transitioner) {name} has no updater, initializing it"); }
            init_updater();
            if (updater == null)
            {
                Debug.LogError($"(Transitioner) {name} has no updater after initialization, cannot transition");
                return;
            }
        }

        // checks if transition duration is 0
        if (duration == 0f)
        {
            updater(end_value);
            return;
        }

        // then we make the transition happen
        tween = Tween.Custom(start_value, end_value, duration: duration,
            onValueChange: updater, useUnscaledTime: unscaled_time, ease: ease);

        while (tween.Value.isAlive) { await Task.Yield(); }

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

    // HIDE & DESTROY
    public async Awaitable HideAndDestroy(float duration = -99f)
    {
        if (duration < 0f) { duration = default_duration; }
        if (log) { Debug.Log($"(Transitioner) Hiding and destroying {name} with duration {duration}"); }

        // we hide the transitionner
        await Hide(duration);
        await Task.Delay(100); // wait a small time (100 ms) to ensure transition has happened
        Destroy(gameObject);
    }
    public async Awaitable HideAndDisable(float duration = -99f)
    {
        if (duration < 0f) { duration = default_duration; }
        if (log) { Debug.Log($"(Transitioner) Hiding and disabling {name} with duration {duration}"); }

        // we hide the transitionner
        await Hide(duration);
        await Task.Delay(100); // wait a small time (100 ms) to ensure transition has happened
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (tween != null && tween.Value.isAlive)
        {
            tween.Value.Stop();
        }
    }
}

public enum TransitionType
{
    Fade,
    Popup
}