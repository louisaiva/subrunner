using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using Unity.Cinemachine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterOrientationController : MonoBehaviour
{

    [Header("Input & Callbacks")]
    private InputManager input_manager;
    [SerializeField] private InputActionReference rotateInput;
    private InputAction rotateAction;
    private event Action<InputAction.CallbackContext> rotateCallback;
    InputControl[] rotate_controls;
    string[] rotate_controls_paths;
    private event Action<InputAction.CallbackContext> playCallback;

    [Header("Components")]
    [SerializeField] private UI_AnimPlayer animPlayer;
    [SerializeField] private JoystickFeedback joystickFeedback;

    // START
    protected void Start()
    {
        if (rotateAction != null) { return; } // on ne fait rien si on a déjà Start()

        // we get the actions
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        rotateAction = input_manager.GetAction(rotateInput);
        rotateCallback = ctx => handleOrientation(ctx.ReadValue<Vector2>());
        rotateAction.performed += rotateCallback;

        // we get the anim player
        animPlayer = GetComponent<UI_AnimPlayer>();


        // we set some callbacks for launching the game
        playCallback = ctx => press_button(ctx);
        input_manager.inputs.any.keyboard.performed += playCallback;
        input_manager.inputs.any.gamepad.performed += playCallback;


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

    private void handleOrientation(Vector2 input)
    {
        // we check if the input is not zero
        if (input == Vector2.zero) { return; }

        // we set the orientation of the animplayer
        animPlayer.SetOrientation(input);
    }

    private void press_button(InputAction.CallbackContext ctx)
    {
        // we check if the binding of the input action is a different binding from the rotate action input action

        // we get the input control
        InputControl control = ctx.action.activeControl;
        // Debug.Log("INPUT CONTROL: " + control.path + " - " + control.device + " - " + control.device.displayName);

        // we check if the binding is not null and if it is not the rotate action input action
        if (control != null && rotate_controls_paths.Contains(control.path)) { return; }

        StopAllCoroutines(); // we stop all coroutines to avoid multiple calls
        StartCoroutine(load_game()); // we start the coroutine to load the game
    }

    private IEnumerator load_game()
    {
        // we disable the input action
        rotateAction.performed -= rotateCallback;
        input_manager.inputs.any.keyboard.performed -= playCallback;
        input_manager.inputs.any.gamepad.performed -= playCallback;

        // we disable the input feedback
        Destroy(joystickFeedback.gameObject);

        // we disable the anim_player
        animPlayer.enabled = false;

        // we wait one frame
        yield return null;

        Debug.Log("LAUNCH THE GAME");

        // load the main clean scene
        SceneManager.LoadSceneAsync(1);
    }

}