using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Linq;

public class CharacterOrientationController : MonoBehaviour
{

    // [Header("Input & Callbacks")]
    // private InputManager input_manager;
    // [SerializeField] private InputActionReference rotateInput;
    // private InputAction rotateAction;
    // private event Action<InputAction.CallbackContext> rotateCallback;
    // private InputControl[] rotate_controls;
    // private string[] rotate_controls_paths;
    // private event Action<InputAction.CallbackContext> playCallback;

    [Header("Components")]
    [SerializeField] private UI_AnimPlayer animPlayer;

    // START
    protected void Start()
    {
        // we get the anim player
        animPlayer = GetComponent<UI_AnimPlayer>();
    }

    // ORIENTATION UPDATING
    public void HandleOrientation(Vector2 input)
    {
        // we check if the input is not zero
        if (input == Vector2.zero) { return; }

        // we set the orientation of the animplayer
        animPlayer.SetOrientation(input);
    }

    // LOAD GAME BUTTON PRESSING
    /* private void press_button(InputAction.CallbackContext ctx)
    {
        // we check if the binding of the input action is a different binding from the rotate action input action

        // we get the input control
        InputControl control = ctx.action.activeControl;
        // Debug.Log("INPUT CONTROL: " + control.path + " - " + control.device + " - " + control.device.displayName);

        // we check if the binding is not null and if it is not the rotate action input action
        if (control != null && rotate_controls_paths.Contains(control.path)) { return; }

        SceneLoader.Instance.LoadGame();
    } */


    // CALLBACKS
    /* private void setCallbacks()
    {
        rotateAction.performed += rotateCallback;
        input_manager.inputs.any.keyboard.performed += playCallback;
        input_manager.inputs.any.gamepad.performed += playCallback;
    }

    public void RemoveCallbacks()
    {
        rotateAction.performed -= rotateCallback;
        input_manager.inputs.any.keyboard.performed -= playCallback;
        input_manager.inputs.any.gamepad.performed -= playCallback;
    } */

}