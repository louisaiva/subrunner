using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System;
using System.Collections;

public class InputManager : Singleton<InputManager>
{
    [Header("INPUT MANAGER")]
    [SerializeField] private string current_input_type = "keyboard"; // keyboard or gamepad
    public string CurrentInputType => current_input_type;
    public event Action<string> OnInputTypeChanged = delegate { };
    public PlayerInputActions inputs;


    [Header("Inputs thresholds")]
    [SerializeField] public float JOYSTICK_MIN_THRESHOLD = 0.3f;
    [SerializeField] public float JOYSTICK_MAX_THRESHOLD = 0.95f;
    [SerializeField] public float BUTTON_MIN_THRESHOLD = 0.2f;
    [SerializeField] public float BUTTON_MAX_THRESHOLD = 0.8f;
    // [SerializeField] public float MOUSE_DELTA_MIN_THRESHOLD = 5f;

    [Header("Inputing endlessly")]
    [SerializeField] public float BUTTON_ENDLESSLY_SHORT_THRESHOLD = 0.3f; // time threshold input need to be maintain before inputing endlessly
    [SerializeField] public float BUTTON_ENDLESSLY_SHORT_DELAY = 0.06f; // when inputing endlessly, delay btwn each input
    [SerializeField] public float BUTTON_ENDLESSLY_LONG_THRESHOLD = 0.6f; // time threshold input need to be maintain before inputing endlessly
    [SerializeField] public float BUTTON_ENDLESSLY_LONG_DELAY = 0.1f; // when inputing endlessly, delay btwn each input


    [Header("Components")]
    [SerializeField] private InputSystemUIInputModule input_system_ui_input_module;

    [Header("Logs")]
    public bool log = false;
    public bool log_input_maps_enabled = false;


    // unity functions
    protected override void Awake()
    {
        base.Awake();

        // on crée les inputs
        inputs = new PlayerInputActions();

        // on active les inputs
        inputs.perso.Enable();
        inputs.UI.Enable();
        inputs.any.Enable();
        inputs.menus.Enable();
        inputs.feedbacks.Enable();

        // on ajoute les listeners
        inputs.any.keyboard.performed += ctx => setInputType("keyboard");
        inputs.any.gamepad.performed += ctx => setInputType("gamepad");

        // inputs.perso.move.performed += ctx => MovementRawInputs = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        if (log_input_maps_enabled)
        {
            string s = "inputs maps: \n\t";
            s += "- perso : " + inputs.perso.enabled + "\n\t";
            s += "- ui : " + inputs.UI.enabled + "\n\t";
            s += "- any : " + inputs.any.enabled + "\n\t";
            // s += "- enhanced_perso : " + inputs.enhanced_perso.enabled + "\n\t";
            s += "- menus : " + inputs.menus.enabled + "\n\t";
            Debug.Log(s);
        }
    }

    // SWITCH INPUTS TYPE
    private void setInputType(string input_type)
    {
        if (current_input_type == input_type) { return; }

        // on met à jour le type d'input
        current_input_type = input_type;
        // input_system_ui_input_module.enabled = input_type == "keyboard";
        Cursor.visible = input_type == "keyboard";
        if (log) { Debug.Log("(InputManager) switching to " + input_type); }
        OnInputTypeChanged?.Invoke(current_input_type);
    }

    // getters
    public InputAction GetAction(InputActionReference reference)
    {
        if (reference == null)
        {
            if (log) { Debug.LogWarning("(InputManager) GetAction called with null reference"); }
            return null;
        }

        // on récupère le nom de l'action
        string action_name = reference.ToString();

        // on découpe le nom de l'action avec : 
        string[] action_name_parts = action_name.Split(':');
        if (action_name_parts.Length > 1) { action_name = action_name_parts[1]; }

        // on retourne l'action
        return getActionFromString(action_name);
    }
    private InputAction getActionFromString(string action_name)
    {
        // on découpe via / pour avoir l'inputMap
        string[] action_name_parts = action_name.Split('/');
        string inputMap = action_name_parts[0];
        action_name = action_name_parts[1];

        // on instancie l'action
        InputAction action = null;

        // on récupère l'action
        if (inputMap == "perso") { action = inputs.perso.Get()[action_name]; }
        else if (inputMap == "UI") { action = inputs.UI.Get()[action_name]; }
        else if (inputMap == "any") { action = inputs.any.Get()[action_name]; }
        else if (inputMap == "menus") { action = inputs.menus.Get()[action_name]; }
        else if (inputMap == "feedbacks") { action = inputs.feedbacks.Get()[action_name]; }
        // else if (inputMap == "enhanced_perso") { action = inputs.enhanced_perso.Get()[action_name]; }

        // on log
        if (log) { Debug.Log("(InputManager) getting action : " + action_name + " from " + inputMap + " returned " + action); }

        // on retourne l'action
        return action;
    }
    public bool isUsingGamepad()
    {
        return current_input_type == "gamepad";
    }
    public string getCurrentInputType()
    {
        return current_input_type;
    }
    public Vector2 MovementRawInputs { get => inputs.perso.move.ReadValue<Vector2>(); }

    // INPUTS MAP TOGGLING
    public event Action<bool> OnPersoInputsToggled = delegate { };
    public void EnablePersoInputs()
    {
        if (log) { Debug.Log("(InputManager) enabling perso inputs"); }
        inputs.perso.Enable();

        OnPersoInputsToggled?.Invoke(true);
    }
    public void DisablePersoInputs()
    {
        if (log) { Debug.Log("(InputManager) disabling perso inputs"); }
        inputs.perso.Disable();

        OnPersoInputsToggled?.Invoke(false);
    }


    // INPUTS COROUTINES
    public Coroutine StartInputCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
    public void StopInputCoroutine(Coroutine coroutine)
    {
        StopCoroutine(coroutine);
    }

    // ON DESTROY
    private void OnDestroy()
    {
        OnInputTypeChanged = null;
    }

}