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
    protected InputManager input_manager;
    protected InputAction action;
    protected System.Action<InputAction.CallbackContext> input_callback;
    protected System.Action<InputAction.CallbackContext> reset_callback;
    protected System.Action<InputAction.CallbackContext> press_and_release_callback;
    public bool use_press_and_release = false; // whether to use the press and release callback instead of the simple input & reset
    private bool is_pressed = false;

    [Header("Image")]
    [SerializeField] protected Image image;
    [SerializeField] protected SpriteBank bank { get
        {
            if (_bank != null) { return _bank; }
            
            _bank = AnimBank.Instance.GetComponent<SpriteBank>();
            return _bank;
        } }
    private SpriteBank _bank;

    [Header("Colors & Label")]
    [SerializeField] protected Color base_color = new Color(1f, 1f, 1f, 1f);
    [SerializeField] protected Color clicked_color = new Color(1f, 1f, 0f, 1f);
    [SerializeField] protected List<UI_Colorer> colorers = new List<UI_Colorer>();
    [SerializeField] private TextMeshProUGUI label;

    [Header("Logs")]
    public bool log = false;

    // START
    protected virtual void Start()
    {
        // we verify the image & the input
        if (log)
        {
            if (image == null) { Debug.LogWarning("(InputFeedback : " + name + " ) image is not set ! you should assign it in the inspector"); }
            if (input == null) { Debug.LogWarning("(InputFeedback : " + name + " ) input is not set ! you should assign it in the inspector"); }
        }

        // we get the input manager & input
        input_manager = InputManager.Instance;
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

        // and the complexe callback for press & release
        press_and_release_callback = ctx => HandlePressAndReleaseInput(ctx);
    }

    // ONENABLE/DISABLE
    protected virtual void OnEnable()
    {
        if (action == null) { return; }

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

        // we reset the IF
        OnReset();
    }
    protected virtual void OnDisable()
    {
        if (action == null) { return; }

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
    }


    // INPUT / RESET
    public virtual void OnInput()
    {
        is_pressed = true;
        // we set the color
        image.color = clicked_color;

        // we apply the colorers color if we have any
        foreach (UI_Colorer colorer in colorers)
        {
            colorer.ApplyColor(clicked_color);
        }
    }
    public virtual void OnReset()
    {
        is_pressed = false;
        // we set the color
        image.color = base_color;

        // we revert the colorers color if we have any
        foreach (UI_Colorer colorer in colorers)
        {
            colorer.RevertColor();
        }
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

    // SETTERS
    public void SetLabel(string text)
    {
        if (label == null)
        {
            if (log) { Debug.LogWarning("(InputFeedback : " + name + " ) label is not set ! you should assign it in the inspector"); }
            return;
        }
        label.text = text;
    }
    public void SetColors(Color base_color, Color clicked_color)
    {
        this.base_color = base_color;
        this.clicked_color = clicked_color;

        // we apply the base color immediately
        image.color = is_pressed ? clicked_color : base_color;
    }
}