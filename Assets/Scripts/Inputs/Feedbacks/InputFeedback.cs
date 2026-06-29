using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// This class is used to give feedback to the player when they are inputting a command.
/// Update the given Sprite to the correct one when the player is inputting a command.
/// </summary>
public class InputFeedback : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] protected InputActionReference input;
    protected InputManager input_manager;
    protected InputAction action;
    protected System.Action<InputAction.CallbackContext> input_callback;
    protected System.Action<InputAction.CallbackContext> reset_callback;
    protected System.Action<InputAction.CallbackContext> press_and_release_callback;
    public bool use_press_and_release = false; // whether to use the press and release callback instead of the simple input & reset
    protected bool is_pressed = false;

    [Header("Logs")]
    public bool log = false;

    // START
    protected void Awake()
    {
        // we get the input manager & input
        input_manager = InputManager.LazyInstance;
        if (input == null) { return; }
        this.action = input_manager.GetAction(input);
        defineCallbacks();
    }
    public virtual void InitializeWithAction(InputAction action)
    {
        this.action = action;
        defineCallbacks();
        register_callbacks(action);
        OnReset();
    }

    // DEFINE CALLBACKS
    protected virtual void defineCallbacks()
    {
        // we define the callback
        input_callback = ctx => OnInput();
        reset_callback = ctx => OnReset();

        // and the complexe callback for press & release
        press_and_release_callback = ctx => HandlePressAndReleaseInput(ctx);
    }

    // REGISTER / UNREGISTER CALLBACKS
    protected bool callbacks_registered = false;
    protected virtual void register_callbacks(InputAction action)
    {
        if (callbacks_registered) { return; }

        // we add the listeners
        if (use_press_and_release)
        {
            action.performed += press_and_release_callback;
        }
        else
        {
            action.performed += input_callback;
            action.canceled += reset_callback;
        }
        callbacks_registered = true;
    }
    protected virtual void unregister_callbacks(InputAction action)
    {
        if (!callbacks_registered) { return; }

        // we remove the listeners
        if (use_press_and_release)
        {
            action.performed -= press_and_release_callback;
        }
        else
        {
            action.performed -= input_callback;
            action.canceled -= reset_callback;
        }
        callbacks_registered = false;
    }
    
    // ON ENABLE / DISABLE
    protected virtual void OnEnable()
    {
        if (action == null) { return; }
        register_callbacks(action);
        OnReset();
    }
    protected virtual void OnDisable()
    {
        if (action == null) { return; }
        unregister_callbacks(this.action);
    }


    // INPUT / RESET
    public virtual void OnInput()
    {
        is_pressed = true;
    }
    public virtual void OnReset()
    {
        is_pressed = false;
    }
    public virtual void HandlePressAndReleaseInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnInput();
        }
        else if (context.canceled)
        {
            OnReset();
        }
    }

}