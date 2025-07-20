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
    public bool Available = true;

    [Header("On Enable transition")]
    [SerializeField] protected bool transition_on_enable = true;
    protected Action<float> updater;

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

    private void OnEnable()
    {
        if (!transition_on_enable) { return; }
        Transition(true, default_duration);
    }

    // TRANSITION
    public async Awaitable Transition(bool show, float duration = default)
    {
        Available = false;
        if (duration == default)
        {
            duration = default_duration;
        }

        Ease ease = show ? ease_show : ease_hide;

        float start_value = get_current_value();
        float end_value = show ? shown_value : hidden_value;

        // then we make the transition happen
        await Tween.Custom(start_value, end_value, duration: duration,
                onValueChange: updater, useUnscaledTime: true, ease: ease);
                
        Available = true;
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