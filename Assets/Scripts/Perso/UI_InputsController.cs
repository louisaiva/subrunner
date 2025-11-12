using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class UI_InputsController : InputController
{
    [SerializeField] private bool log_states = false;

    [Header("UI Inputs Parameters")]
    [SerializeField] private UI_Navigator navigator;
    [SerializeField] private UIActions ui_inputs;
    private MenusActions ui_menus;
    [SerializeField] private bool in_game = false; // if true we navigate in game, else in UI
    public bool InGame { get => in_game; }

    // CALLBACKS
    private Action<InputAction.CallbackContext> ui_dropCallback = delegate { };
    private Action<InputAction.CallbackContext> ui_navigateCallback = delegate { };
    private Action<InputAction.CallbackContext> ui_activateCallback = delegate { };
    private Action<InputAction.CallbackContext> mouse_navigationCallback = delegate { };
    private Action<InputAction.CallbackContext> ui_exitCallback = delegate { };

    // ALWAYS ACTIVATED CALLBACKS
    private Action<InputAction.CallbackContext> ui_exploit_selectionCallback = delegate { };
    private Action<InputAction.CallbackContext> ui_cancelPoolCallback = delegate { };
    private Action<InputAction.CallbackContext> ui_rollPanelCallback = delegate { };
    private Action<InputAction.CallbackContext> ui_inventoryCallback = delegate { };
    private Action<InputAction.CallbackContext> ui_pauseCallback = delegate { };


    // START
    protected void Start()
    {
        // on récupère les inputs
        // perso_inputs = InputManager.Instance.inputs.perso;
        initInputs();

        // on récupère le navigator
        navigator = UI_Navigator.Instance;

        // on crée les endless inputs pour la navigation continue
        add_endless_input(new EndlessInput<Vector2>("ui_navigate", ui_inputs.navigate,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_DELAY,
                unscaled_time: true)).OnEndless += (direction) => OnUI_Navigate(direction);
        /* add_endless_input(new EndlessInput<Vector2>("ui_navigate_ingame", ui_inputs.navigate_in_game,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_DELAY,
                unscaled_time: false)).OnEndless += (direction) => OnUI_Navigate(direction); */

        // et pour le drop continu
        add_endless_input(new EndlessInput<float>("ui_drop", ui_inputs.ui_drop,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_LONG_DELAY,
                unscaled_time: true)).OnEndless += _ => OnUI_Drop();
        add_endless_input(new EndlessInput<float>("ui_drop_ingame", ui_inputs.ui_drop_ingame,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_LONG_DELAY,
                unscaled_time: false)).OnEndless += _ => OnUI_Drop();

        // et pour l'activation
        add_endless_input(new EndlessInput<float>("ui_activate", ui_inputs.activate,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD,
                repeat: -1, // no repeat, only holding
                unscaled_time: true)).OnHold += _ => OnUI_ActivateHeld();
        add_endless_input(new EndlessInput<float>("ui_activate_ingame", ui_inputs.ui_drop_ingame,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD,
                repeat: -1, // no repeat, only holding
                unscaled_time: false)).OnHold += _ => OnUI_ActivateHeld();
    }

    // INPUTS
    private void initInputs()
    {
        // on récupère les inputs
        ui_inputs = InputManager.Instance.inputs.UI;
        ui_menus = InputManager.Instance.inputs.menus;


        // on crée les callbacks
        ui_dropCallback = ctx => handle_UI_drop_input(ctx);
        ui_navigateCallback = ctx => handle_UI_navigate_input(ctx);
        ui_activateCallback = ctx => handle_UI_activate_input(ctx);
        ui_exitCallback = ctx => handle_UI_exit_input(ctx);
        mouse_navigationCallback = ctx => OnUI_Navigate(ctx.ReadValue<Vector2>());


        // on crée les always active callbacks
        ui_exploit_selectionCallback = ctx => handle_exploit_selection_input(ctx.ReadValue<Vector2>());
        ui_cancelPoolCallback = ctx => { handle_cancel_pool_input(ctx.ReadValue<float>()); };
        ui_rollPanelCallback = ctx => { handle_roll_panel_input(ctx.ReadValue<float>()); };
        ui_inventoryCallback = ctx => { UI_Manager.Instance.TogglePool("inventory"); };
        ui_pauseCallback = ctx => { UI_Manager.Instance.TogglePool("pause"); };


        // on met en place certains callbacks qu'on veut tout le temps actifs
        ui_inputs.navigate_exploits.performed += ui_exploit_selectionCallback;
        ui_inputs.cancel.performed += ui_cancelPoolCallback;
        ui_inputs.roll_panel.performed += ui_rollPanelCallback;
        // notamment les inputs de menus
        ui_menus.inventory.performed += ui_inventoryCallback;
        ui_menus.pause.performed += ui_pauseCallback;

        if (log_states) { Debug.Log("(UI_InputsController) UIC INIT"); }
    }
    public void EnableInputs(bool ingame_navigation = false)
    {
        // on active les bons callbacks
        if (ingame_navigation)
        {
            // ui_inputs.navigate_in_game.performed += ui_navigateCallback;
            ui_inputs.ui_drop_ingame.performed += ui_dropCallback;
            ui_inputs.ui_exit_ingame.performed += ui_exitCallback;
        }
        else
        {
            ui_inputs.ui_drop.performed += ui_dropCallback;
            ui_inputs.activate.performed += ui_activateCallback;
        }
        ui_inputs.navigate.performed += ui_navigateCallback;

        // ui_inputs
        ui_inputs.mouse_navigation.performed += mouse_navigationCallback;
        in_game = ingame_navigation; // on met à jour la variable


        if (log_states) { Debug.Log($"(UI_InputsController) ON"); }
    }
    public void DisableInputs()
    {
        // on désactive les inputs
        ui_inputs.navigate.performed -= ui_navigateCallback;
        ui_inputs.navigate_in_game.performed -= ui_navigateCallback;
        ui_inputs.ui_drop.performed -= ui_dropCallback;
        ui_inputs.ui_drop_ingame.performed -= ui_dropCallback;
        ui_inputs.ui_exit_ingame.performed -= ui_exitCallback;
        ui_inputs.activate.performed -= ui_activateCallback;
        ui_inputs.mouse_navigation.performed -= mouse_navigationCallback;
        // ui_inputs.ui_move_item.performed -= ui_item_moveCallback;

        in_game = false; // on met à jour la variable

        if (log_states) { Debug.Log($"(UI_InputsController) OFF"); }
    }
    public void ToggleInput(string input_name, bool enable = true)
    {
        /* if (input_name == "drop")
        {
            if (enable)
            {
                ui_inputs.ui_drop.performed += ui_dropCallback;
                ui_inputs.ui_drop_ingame.performed += ui_dropCallback;
            }
            else
            {
                ui_inputs.ui_drop.performed -= ui_dropCallback;
                ui_inputs.ui_drop_ingame.performed -= ui_dropCallback;
            }
        } */
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
    public bool IsEndlessInputDown<T>(string input_name) where T : struct
    {
        EndlessInput<T> endless_input = get_endless_input<T>(input_name);
        if (log_endless_inputs) { Debug.Log($"(UI_InputsController) checking if endless input {input_name} is down : " + (endless_input != null ? endless_input.IsInputDown().ToString() : "endless input not found")); }
        if (endless_input == null) { return false; }
        return endless_input.IsInputDown();
    }

    // GETTERS
    public bool IsMovingInputDown()
    {
        return IsEndlessInputDown<float>("ui_activate") || IsEndlessInputDown<float>("ui_drop_ingame");
    }
    public bool IsDropInputDown()
    {
        if (in_game) { return IsEndlessInputDown<float>("ui_drop_ingame"); }
        return IsEndlessInputDown<float>("ui_drop");
    }

    // UI_NAVIGATE
    public void handle_UI_navigate_input(InputAction.CallbackContext context)
    {
        // if we press have a big joystick magnitude we launch the endless threshold
        Vector2 navigate_value = context.ReadValue<Vector2>();
        if (navigate_value.magnitude >= 0.5f)
        {
            OnUI_Navigate(navigate_value);
            get_endless_input<Vector2>("ui_navigate").OnInput(context);
            return;
        }
    }
    private void OnUI_Navigate(Vector2 direction) { navigator.OnNavigate(direction); }

    // UI_ACTIVATE
    private void handle_UI_activate_input(InputAction.CallbackContext context)
    {
        // checks magnitue to know if we downed or released
        float activate_value = context.ReadValue<float>();
        if (activate_value >= 0.5f)
        {
            navigator.OnDown();
            get_endless_input<float>("ui_activate" /* + (navigate_in_game ? "_ingame" : "" )*/).OnInput(context);
            return;
        }
        navigator.OnActivate();
    }
    private void OnUI_ActivateHeld() { navigator.StartMovingItemIfInputDown(); }

    // UI_DROP
    public void handle_UI_drop_input(InputAction.CallbackContext context)
    {
        // if we press the button we launch the endless threshold
        if (context.ReadValue<float>() >= 0.5f)
        {
            navigator.OnDown(for_drop: true);

            // si on est pas in game on lance l'endless drop
            if (!in_game) { get_endless_input<float>("ui_drop").OnInput(context); }

            // si on est in game on lance l'activation du ui_drop_ingame
            else { get_endless_input<float>("ui_drop_ingame").OnInput(context); }

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


    // UI_EXIT
    private void handle_UI_exit_input(InputAction.CallbackContext context)
    {
        // Debug.Log("(UI_InputsController) handling UI exit input : " + context.ReadValue<float>());
        if (context.ReadValue<float>() > 0.5f) { return; } // only on release
        UI_Navigator.Instance.OnExit();
    }

    // UI_MANAGER CANCEL POOL
    private void handle_cancel_pool_input(float input)
    {
        if (input > 0.5f) { return; } // we only handle the release of the input

        UI_Manager.Instance.CancelCurrentPool();
    }

    // ROLL PANEL INPUT
    private void handle_roll_panel_input(float input)
    {
        if (input == 0f) { return; } // no input

        // on check si le current pool est un panelable (si non, ça sert a r de scroll)
        UI_Pool current_pool = UI_Manager.Instance.GetCurrentPool();
        if (current_pool == null || current_pool is not Panelable panelable) { return; }

        Debug.Log($"(UI_InputsController) rolling panel with input {input}");
        panelable.PanelManager.RollPanel((int)input);
    }


    // OnDestroy
    private void OnDestroy()
    {
        DisableInputs();

        // on désactive les callbacks tout le temps actifs
        ui_inputs.navigate_exploits.performed -= ui_exploit_selectionCallback;
        ui_inputs.cancel.performed -= ui_cancelPoolCallback;
        ui_inputs.roll_panel.performed -= ui_rollPanelCallback;
        // notamment les inputs de menus
        ui_menus.inventory.performed -= ui_inventoryCallback;
        ui_menus.pause.performed -= ui_pauseCallback;

        if (log_states) { Debug.Log("(UI_InputsController) UIC ON DESTROY"); }

    }
}