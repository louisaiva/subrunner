using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
/// <summary>
/// this class is used to invoke an event when an input is pressed
/// works with the InputManager
/// mainly for debugging purposes
/// </summary>
public class InputButton : MonoBehaviour
{

    [Header("Input & Callbacks")]
    [SerializeField] private InputActionReference buttonInput;
    private InputAction buttonAction;

    [Header("Event")]
    [SerializeField] private UnityEvent onButtonPressed;
    [SerializeField] private UnityEvent onButtonReleased;

    // START
    private void Start()
    {
        // we get the button action
        buttonAction = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().GetAction(buttonInput);

        // we set the callbacks
        buttonAction.performed += ctx => OnButton(ctx);
        buttonAction.canceled += ctx => OnButton(ctx);
    }

    // BUTTON ACTION CALLBACK
    private void OnButton(InputAction.CallbackContext ctx)
    {
        // we check if the button is pressed
        if (ctx.performed)
        {
            onButtonPressed?.Invoke();
        }
        else if (ctx.canceled)
        {
            onButtonReleased?.Invoke();
        }
    }

}