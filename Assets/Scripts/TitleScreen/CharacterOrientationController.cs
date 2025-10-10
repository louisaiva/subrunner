using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Linq;

public class CharacterOrientationController : MonoBehaviour
{

    [Header("Input & Callbacks")]
    private InputManager input_manager;
    [SerializeField] private InputActionReference rotateInput;
    private InputAction rotateAction;
    private event Action<InputAction.CallbackContext> rotateCallback;
    private InputControl[] rotate_controls;
    private string[] rotate_controls_paths;
    private event Action<InputAction.CallbackContext> playCallback;

    [Header("Components")]
    [SerializeField] private UI_AnimPlayer animPlayer;

    // START
    protected void Start()
    {
        if (rotateAction != null) { return; } // on ne fait rien si on a déjà Start()

        // we get the actions
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        rotateAction = input_manager.GetAction(rotateInput);

        // we create the callbacks & set them
        playCallback = ctx => press_button(ctx);
        rotateCallback = ctx => handleOrientation(ctx.ReadValue<Vector2>());
        setCallbacks();

        // we get the anim player
        animPlayer = GetComponent<UI_AnimPlayer>();

        // we get the controls of the rotate action
        rotate_controls = rotateAction.controls.ToArray();
        rotate_controls_paths = new string[rotate_controls.Length];
        string s = "ROTATE CONTROLS: ";
        foreach (InputControl c in rotate_controls)
        {
            s += "\n - " + c.path;
            rotate_controls_paths[Array.IndexOf(rotate_controls, c)] = c.path;
        }
        // Debug.Log(s);
    }

    // ORIENTATION UPDATING
    private void handleOrientation(Vector2 input)
    {
        // we check if the input is not zero
        if (input == Vector2.zero) { return; }

        // we set the orientation of the animplayer
        animPlayer.SetOrientation(input);
    }

    // LOAD GAME BUTTON PRESSING
    private void press_button(InputAction.CallbackContext ctx)
    {
        // we check if the binding of the input action is a different binding from the rotate action input action

        // we get the input control
        InputControl control = ctx.action.activeControl;
        // Debug.Log("INPUT CONTROL: " + control.path + " - " + control.device + " - " + control.device.displayName);

        // we check if the binding is not null and if it is not the rotate action input action
        if (control != null && rotate_controls_paths.Contains(control.path)) { return; }

        SceneLoader.Instance.LoadGame();
    }


    // CALLBACKS
    private void setCallbacks()
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
    }

}