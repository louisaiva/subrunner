using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class UI_InputsController : InputController
{
    [Header("UI Inputs Parameters")]
    // [SerializeField] private UI_XboxNavigator navigator;
    [SerializeField] private UI_Navigator navigator;
    [SerializeField] private UIActions ui_inputs;
    [SerializeField] private bool navigate_in_game = false; // if true we navigate in game, else in UI

    [Header("Actions")]
    // private InputAction ui_drop_ingameAction;
    private PersoActions perso_inputs;
    private event Action<InputAction.CallbackContext> ui_dropCallback;
    private event Action<InputAction.CallbackContext> ui_navigateCallback;
    private event Action<InputAction.CallbackContext> ui_activateCallback;

    // START
    protected void Start()
    {
        // on récupère les inputs
        perso_inputs = InputManager.Instance.inputs.perso;
        initInputs();

        // on récupère le navigator
        navigator = UI_Navigator.Instance;

        // on crée les endless inputs pour la navigation continue
        add_endless_input(new EndlessInput<Vector2>("ui_navigate", ui_inputs.navigate,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_DELAY,
                unscaled_time: true)).OnEndless += (direction) => OnUI_Navigate(direction);
        add_endless_input(new EndlessInput<Vector2>("ui_navigate_ingame", ui_inputs.navigate_in_game,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_DELAY,
                unscaled_time: false)).OnEndless += (direction) => OnUI_Navigate(direction);

        // et pour le drop continu
        add_endless_input(new EndlessInput<float>("ui_drop", ui_inputs.x,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_LONG_DELAY,
                unscaled_time: true)).OnEndless += _ => OnUI_Drop();
        add_endless_input(new EndlessInput<float>("ui_drop_ingame", perso_inputs.interact,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_LONG_DELAY,
                unscaled_time: false)).OnEndless += _ => OnUI_Drop();
    }

    // INPUTS
    private void initInputs()
    {
        // on récupère les inputs
        ui_inputs = InputManager.Instance.inputs.UI;

        // on crée les callbacks
        ui_dropCallback = ctx => handle_UI_drop_input(ctx);
        ui_navigateCallback = ctx => handle_UI_navigate_input(ctx);
        ui_activateCallback = ctx => handle_UI_activate_input(ctx);

        // on met en place certains callbacks qu'on veut tout le temps actifs
        ui_inputs.navigate_exploits.performed += ctx => handle_exploit_selection_input(ctx.ReadValue<Vector2>());
        ui_inputs.cancel.performed += ctx => { handle_cancel_pool_input(ctx.ReadValue<float>()); };
        // notamment les inputs de menus
        MenusActions ui_menus = InputManager.Instance.inputs.menus;
        ui_menus.inventory.performed += ctx => { UI_Manager.Instance.TogglePool("inventory"); };
        ui_menus.pause.performed += ctx => { UI_Manager.Instance.TogglePool("pause"); };
    }
    public void EnableInputs(bool ingame_navigation = false)
    {
        // moveItemAction.performed += moveItemCallback;

        // on active les bons callbacks
        if (ingame_navigation)
        {
            ui_inputs.navigate_in_game.performed += ui_navigateCallback;
            perso_inputs.interact.performed += ui_dropCallback;
        }
        else
        {
            ui_inputs.activate.performed += ui_activateCallback;
            ui_inputs.navigate.performed += ui_navigateCallback;
            ui_inputs.x.performed += ui_dropCallback;
        }

        navigate_in_game = ingame_navigation; // on met à jour la variable
    }
    public void DisableInputs()
    {
        // on récupère les inputs
        ui_inputs.navigate.performed -= ui_navigateCallback;
        ui_inputs.navigate_in_game.performed -= ui_navigateCallback;
        ui_inputs.x.performed -= ui_dropCallback;
        perso_inputs.interact.performed -= ui_dropCallback;
        ui_inputs.activate.performed -= ui_activateCallback;
        // moveItemAction.performed -= moveItemCallback;

        navigate_in_game = false; // on met à jour la variable
    }
    public void ToggleInput(string input_name, bool enable = true)
    {
        if (input_name == "drop")
        {
            if (enable) { ui_inputs.x.performed += ui_dropCallback; }
            else { ui_inputs.x.performed -= ui_dropCallback; }
        }
        /* else if (input_name == "activate")
        {
            if (enable) { activateAction.performed += activateCallback; }
            else { activateAction.performed -= activateCallback; }
        }
        else if (input_name == "move")
        {
            if (enable) { moveItemAction.performed += moveItemCallback; }
            else { moveItemAction.performed -= moveItemCallback; }
        } */
    }


    // UI_NAVIGATE
    public void handle_UI_navigate_input(InputAction.CallbackContext context)
    {
        // if we press have a big joystick magnitude we launch the endless threshold
        Vector2 navigate_value = context.ReadValue<Vector2>();
        if (navigate_value.magnitude >= 0.5f)
        {
            OnUI_Navigate(navigate_value);
            get_endless_input<Vector2>("ui_navigate" + (navigate_in_game ? "_ingame" : "")).OnInput(context);
            return;
        }

        // OnUI_Drop();
    }
    private void OnUI_Navigate(Vector2 direction)
    {
        navigator.OnNavigate(direction);
    }

    // UI_ACTIVATE
    private void handle_UI_activate_input(InputAction.CallbackContext context)
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

    // UI_DROP
    public void handle_UI_drop_input(InputAction.CallbackContext context)
    {
        // if we press the button we launch the endless threshold
        if (context.ReadValue<float>() >= 0.5f)
        {
            navigator.OnDown();
            get_endless_input<float>("ui_drop" + (navigate_in_game ? "_ingame" : "")).OnInput(context);
            return;
        }

        // else we release the button so we direclty drop with it
        OnUI_Drop();
    }
    private void OnUI_Drop() { navigator.OnDrop(); }


    // RIGHT JOYSTICK EXPLOIT SELECTION
    private UI_ExploitSelector exploit_selector;
    private void handle_exploit_selection_input(Vector2 direction)
    {
        // transfère l'event seulement quand on est dans l'ui pool exploit_wheel
        if (!UI_Manager.Instance.IsStacked("exploit_wheel")) { return; }

        // récupère l'UI_Exploit Selector
        if (exploit_selector == null) { exploit_selector = UI_Manager.Instance.GetPool("exploit_wheel").GetComponent<UI_ExploitSelector>(); }

        // on transfère l'input
        exploit_selector.HandleSelectionInput(direction);
    }


    // UI_MANAGER CANCEL POOL
    private void handle_cancel_pool_input(float input)
    {
        if (input > 0.5f) { return; } // we only handle the release of the input

        UI_Manager.Instance.CancelCurrentPool();
    }

}