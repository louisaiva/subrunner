using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

[Serializable]
public class EndlessInput<T> where T : struct
{
    public string name;
    [SerializeField] private bool waiting = false;
    [SerializeField] private bool holding = false;
    private Coroutine holdCoroutine = null;
    private bool unscaled_time = false;

    // event
    public event Action<T> OnHold; // triggered once the threshold is completed
    public event Action<T> OnEndless; // triggered each time the input is repeated while holding
    public event Action<InputAction.CallbackContext> OnResetted; // triggered once, when the input is released
    public event Action<InputAction.CallbackContext> OnStarted; // triggered once, when the input is first pressed


    // timers
    // WaitForSecondsRealtime wait_threshold_unscaled; // can't cache real time bcz it's creating bugs when timescale changes
    // WaitForSecondsRealtime wait_repeat_unscaled;
    private WaitForSeconds wait_threshold;
    private WaitForSeconds wait_repeat;

    // timers help
    float threshold_time = 0f;
    float repeat_time = 0f;


    public EndlessInput(string name, InputAction action, float threshold = 0.5f, float repeat = 0.5f, bool unscaled_time = false)
    {
        this.name = name;
        // action.started += OnInput;
        if (typeof(T) == typeof(float))
        {
            action.canceled += OnCanceled;
        }
        else if (typeof(T) == typeof(Vector2))
        {
            action.performed += cancel_if_below_threshold;
        }

        // setup timers
        this.unscaled_time = unscaled_time;
        wait_threshold = new WaitForSeconds(threshold);
        wait_repeat = new WaitForSeconds(repeat);

        // setup times
        threshold_time = threshold;
        repeat_time = repeat;
    }

    // INPUTS TRIGGERS
    public void OnInput(InputAction.CallbackContext context)
    {
        if (holdCoroutine != null) { return; } // already started
        holdCoroutine = InputManager.Instance.StartInputCoroutine(HoldInputCoroutine(context));
        OnStarted?.Invoke(context);
    }
    private void OnCanceled(InputAction.CallbackContext context)
    {
        if (holdCoroutine == null) { return; } // not started

        InputManager.Instance.StopInputCoroutine(holdCoroutine);
        waiting = false;
        holding = false;
        holdCoroutine = null;

        OnResetted?.Invoke(context);
    }
    private void cancel_if_below_threshold(InputAction.CallbackContext context)
    {
        if (context.ReadValue<Vector2>().magnitude > 0.5f) { return; }
        OnCanceled(context);
    }

    // HOLDING COROUTINE
    private IEnumerator HoldInputCoroutine(InputAction.CallbackContext context)
    {
        // wait for threshold
        waiting = true;
        yield return unscaled_time ? new WaitForSecondsRealtime(threshold_time) : wait_threshold;
        if (!waiting) { yield break; }

        // we start the holding
        OnHold?.Invoke(context.ReadValue<T>());
        waiting = false;
        holding = true;
        while (holding)
        {
            OnEndless?.Invoke(context.ReadValue<T>());
            yield return unscaled_time ? new WaitForSecondsRealtime(repeat_time) : wait_repeat;
        }

        // we stop the routine
        OnCanceled(context);
    }

}