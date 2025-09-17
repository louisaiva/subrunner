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
    [SerializeField] private InputActionReference input;
    [SerializeField] protected InputManager input_manager;
    protected InputAction action;
    protected System.Action<InputAction.CallbackContext> input_callback;
    protected System.Action<InputAction.CallbackContext> reset_callback;

    [Header("Image")]
    [SerializeField] protected Image image;
    [SerializeField] protected SpriteBank bank;

    [Header("Colors")]
    [SerializeField] protected Color base_color = new Color(1f, 1f, 1f, 1f);
    [SerializeField] protected Color clicked_color = new Color(1f, 1f, 0f, 1f);

    [Header("Colorers")]
    [SerializeField] protected List<UI_Colorer> colorers = new List<UI_Colorer>();

    [Header("Label")]
    [SerializeField] private TextMeshProUGUI label;

    [Header("Logs")]
    public bool debug = false;

    // START
    protected virtual void Start()
    {
        // we verify the image & the input
        if (debug)
        {
            if (image == null) { Debug.LogWarning("(InputFeedback : " + name + " ) image is not set ! you should assign it in the inspector"); }
            if (input == null) { Debug.LogWarning("(InputFeedback : " + name + " ) input is not set ! you should assign it in the inspector"); }
        }
        
        // we get the input manager
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();

        // we get the input
        action = input_manager.GetAction(input);

        defineCallbacks();

        OnEnable();
    }

    // DEFINE CALLBACKS
    protected virtual void defineCallbacks()
    {
        // we define the callback
        input_callback = ctx => OnInput();
        reset_callback = ctx => OnReset();
    }

    // ONENABLE/DISABLE
    private void OnEnable()
    {
        if (action == null) { return; }
        if (bank == null)
        {
            bank = GameObject.Find("/utils/bank").GetComponent<SpriteBank>();
            if (debug) { Debug.Log("(IF) SpriteBank loaded : SpriteBank == " + bank); }
        }

        // we add the listeners
        action.performed += input_callback;
        action.canceled += reset_callback;

        // we reset the IF
        OnReset();
    }
    private void OnDisable()
    {
        if (action == null) { return; }
        // we remove the listeners
        action.performed -= input_callback;
        action.canceled -= reset_callback;
    }


    // INPUT / RESET
    public virtual void OnInput()
    {
        // we set the color
        image.color = clicked_color;

        // we apply the colorers color if we have any
        foreach (UI_Colorer colorer in colorers)
        {
            colorer.ApplyColor();
        }
    }
    public virtual void OnReset()
    {
        // we set the color
        image.color = base_color;

        // we revert the colorers color if we have any
        foreach (UI_Colorer colorer in colorers)
        {
            colorer.RevertColor();
        }
    }

    // SETTERS
    public void SetLabel(string text)
    {
        if (label == null)
        {
            if (debug) { Debug.LogWarning("(InputFeedback : " + name + " ) label is not set ! you should assign it in the inspector"); }
            return;
        }
        label.text = text;
    }
}