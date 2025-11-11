
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
    protected void Start()
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

        /* // et pour l'activation
        add_endless_input(new EndlessInput<float>("ui_activate", ui_inputs.activate,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD,
                repeat: -1, // no repeat, only holding
                unscaled_time: true)).OnHold += _ => OnUI_ActivateHeld();
        add_endless_input(new EndlessInput<float>("ui_activate_ingame", ui_inputs.ui_drop_ingame,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD,
                repeat: -1, // no repeat, only holding
                unscaled_time: false)).OnHold += _ => OnUI_ActivateHeld(); */
    }

    // INPUTS
    private void initInputs()
    {

        // on assigne les callbacks
        ui_inputs.navigate.performed += handle_navigate_input;
        ui_inputs.mouse_navigation.performed += handle_navigate_input;
        ui_inputs.activate.performed += handle_activate_input;
        ui_inputs.navigate_exploits.performed += handle_character_orientation;

        // ui_inputs.roll_panel.performed += ctx => { handle_roll_panel_input(ctx.ReadValue<float>()); };
        // ui_inputs.cancel.performed += ctx => { handle_cancel_pool_input(ctx.ReadValue<float>()); };
    }
    public void RemoveCallbacks()
    {
        // on enlève les callbacks
        ui_inputs.navigate.performed -= handle_navigate_input;
        ui_inputs.mouse_navigation.performed -= handle_navigate_input;
        ui_inputs.activate.performed -= handle_activate_input;
        ui_inputs.navigate_exploits.performed -= handle_character_orientation;
    }
    private void OnDestroy()
    {
        RemoveCallbacks();
    }

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
    }
}