
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

/// <summary>
/// Controller des inputs pour l'UI (menus, inventaires, etc.)
/// </summary>
public class HomeInputsController : InputController
{
    [Header("UI Inputs Parameters")]
    [SerializeField] private UI_Navigator navigator;
    [SerializeField] private UIActions ui_inputs;

    // START
    protected async void Start()
    {
        // on récupère les inputs
        ui_inputs = InputManager.Instance.inputs.UI;
        initInputs();

        // on récupère le navigator
        navigator = UI_Navigator.Instance;

        // on crée les endless inputs pour la navigation continue
        add_endless_input(new EndlessInput<Vector2>("navigate", ui_inputs.navigate,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_DELAY,
                unscaled_time: true)).OnEndless += (direction) => navigator.OnNavigate(direction);
        add_endless_input(new EndlessInput<Vector2>("slide", ui_inputs.navigate_exploits,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_DELAY,
                unscaled_time: true)).OnEndless += slide_if_needed;
    }

    // INPUTS
    private void initInputs()
    {
        // on assigne les callbacks
        ui_inputs.navigate.performed += handle_navigate_input;
        ui_inputs.mouse_navigation.performed += handle_mouse_navigation;
        ui_inputs.activate.performed += handle_activate_input;
        ui_inputs.navigate_exploits.performed += handle_character_orientation;

        ui_inputs.roll_panel.performed += handle_roll_panel_input;
        ui_inputs.cancel.performed += handle_cancel_pool_input;

        // ui_rollPanelCallback = ctx => { handle_roll_panel_input(ctx.ReadValue<float>()); };

        InputManager.Instance.OnInputTypeChanged += reset_all_endless_inputs;
        // ui_inputs.roll_panel.performed += ctx => { handle_roll_panel_input(ctx.ReadValue<float>()); };
        // ui_inputs.cancel.performed += ctx => { handle_cancel_pool_input(ctx.ReadValue<float>()); };
    }
    public void RemoveCallbacks()
    {
        // on enlève les callbacks
        ui_inputs.navigate.performed -= handle_navigate_input;
        ui_inputs.mouse_navigation.performed -= handle_mouse_navigation;
        ui_inputs.activate.performed -= handle_activate_input;
        ui_inputs.navigate_exploits.performed -= handle_character_orientation;
        ui_inputs.roll_panel.performed -= handle_roll_panel_input;
        ui_inputs.cancel.performed -= handle_cancel_pool_input;
        if (InputManager.Instance != null) { InputManager.Instance.OnInputTypeChanged -= reset_all_endless_inputs; }
    }
    private void OnDestroy()
    {
        RemoveCallbacks();
    }
    private void reset_all_endless_inputs(string input_type)
    {
        // on reset tous les endless inputs
        for (int i = 0; i < endless_floats.Count; i++)
        {
            endless_floats[i].Cancel();
        }
        for (int i = 0; i < endless_vectors.Count; i++)
        {
            endless_vectors[i].Cancel();
        }
    }

    // MOUSE NAVIGATION
    public void handle_mouse_navigation(InputAction.CallbackContext context)
    {
        // we check if the mouse is pressing a button or not
        if (Input.GetMouseButton(0)) { handle_drag_input(context); return; }
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) { return; }
        navigator.OnNavigate(context.ReadValue<Vector2>());
    }
    public void handle_drag_input(InputAction.CallbackContext context) { navigator.OnDown(); }

    // UI_NAVIGATE
    public void handle_navigate_input(InputAction.CallbackContext context)
    {
        // if we press have a big joystick magnitude we launch the endless threshold
        Vector2 navigate_value = context.ReadValue<Vector2>();
        if (navigate_value.magnitude >= 0.5f)
        {
            navigator.OnNavigate(navigate_value);
            get_endless_input<Vector2>("navigate").OnInput(context);
            return;
        }
    }

    // UI_ACTIVATE
    private void handle_activate_input(InputAction.CallbackContext context)
    {
        // checks magnitue to know if we downed or released
        float activate_value = context.ReadValue<float>();
        if (activate_value >= 0.5f)
        {
            navigator.OnDown();
            return;
        }
        navigator.OnActivate();
    }
    
    // RIGHT JOYSTICK NAVIGATION
    private void handle_character_orientation(InputAction.CallbackContext context)
    {
        // on transfère l'input
        SceneLoader.Instance.CharacOrienter.HandleOrientation(context.ReadValue<Vector2>());

        // on regarde si on doit commencer l'endless sliding ou pas
        Vector2 slide_value = context.ReadValue<Vector2>();
        if (slide_value.magnitude < 0.5f) { return; }
        
        // if we are already holding no need to launch it
        if (get_endless_input<Vector2>("slide").IsInputDown()) { return; }

        // if we press have a big joystick magnitude we launch the endless threshold
        get_endless_input<Vector2>("slide").OnInput(context);

        // on fait bouger le slider si besoin
        slide_if_needed(slide_value);
    }
    private void slide_if_needed(Vector2 direction)
    {
        // on regarde si le navigator est sur un slider
        if (navigator.CurrentSlot == null) { return; }
        if (navigator.CurrentSlot is not UI_Slider slider) { return; }

        // on fait bouger le slider
        slider.OnSlide(direction.x);
    }



    // UI_MANAGER CANCEL POOL
    private void handle_cancel_pool_input(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();
        if (input > 0.5f) { return; } // we only handle the release of the input

        UI_Manager.Instance.CancelCurrentPool();
    }

    // ROLL PANEL INPUT
    private void handle_roll_panel_input(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();
        if (input == 0f) { return; } // no input

        // on check si le current pool est un panelable (si non, ça sert a r de scroll)
        UI_Pool current_pool = UI_Manager.Instance.GetCurrentPool();
        if (current_pool == null || current_pool is not Panelable panelable) { return; }

        Debug.Log($"(HomeInputsController) rolling panel with input {input}");
        panelable.PanelManager.RollPanel((int)input);
    }
}