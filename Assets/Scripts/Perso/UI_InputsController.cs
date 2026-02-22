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

        // et pour le slide continu dans les options
        add_endless_input(new EndlessInput<Vector2>("slide", ui_inputs.navigate_exploits,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_SHORT_DELAY,
                unscaled_time: true)).OnEndless += slide_if_needed;
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
        // mouse_navigationCallback = ctx => OnUI_Navigate(ctx.ReadValue<Vector2>());
        mouse_navigationCallback = handle_mouse_navigation;


        // on crée les always active callbacks
        ui_exploit_selectionCallback = handle_right_navigation_input;
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
        }
        ui_inputs.activate.performed += ui_activateCallback;
        ui_inputs.navigate.performed += ui_navigateCallback;

        // ui_inputs
        ui_inputs.mouse_navigation.performed += mouse_navigationCallback;
        in_game = ingame_navigation; // on met à jour la variable


        if (log_states) { Debug.Log($"(UI_InputsController) ON ({(ingame_navigation ? "ingame" : "menu")})"); }
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
        return IsEndlessInputDown<float>("ui_activate");
    }
    public bool IsDropInputDown()
    {
        if (in_game) { return IsEndlessInputDown<float>("ui_drop_ingame"); }
        return IsEndlessInputDown<float>("ui_drop");
    }

    // MOUSE NAVIGATION
    public void handle_mouse_navigation(InputAction.CallbackContext context)
    {
        // we check if the mouse is pressing a button or not
        if (Input.GetMouseButton(0)) { handle_drag_input(context); }
        OnUI_Navigate(context.ReadValue<Vector2>());
    }
    public void handle_drag_input(InputAction.CallbackContext context)
    {
        // depends on the slot type
        if (navigator.CurrentSlot == null) { return; }
        
        // if ui_slider we down so it will update the slider
        if (navigator.CurrentSlot is UI_Slider) { navigator.OnDown(); }
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

        // si on est in-game et qu'on utilise la souris on veut pas les activer (pcq ça drop en même temps)
        if (in_game)
        {
            // si on est à la souris on active jamais
            if (!InputManager.Instance.isUsingGamepad()) { return; }

            // si on est au gamepad & qu'on ne bouge pas d'items on up et return
            if (!navigator.Mover.IsMovingItem && navigator.IsCurrentSlotTypeOf(typeof(UI_ItemStack)))
            {
                navigator.OnUp();
                return;
            }
        }
        
        // sinon on active
        navigator.OnActivate();
    }
    private void OnUI_ActivateHeld()
    {
        // si on est in-game et qu'on utilise la souris on veut pas start moving item non plus
        // pcq on veut au contraire endless drop et ça va l'arreter
        if (in_game && !InputManager.Instance.isUsingGamepad()) { return; }

        // on start moving item
        navigator.StartMovingItemIfInputDown();
    }

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


    // RIGHT JOYSTICK NAVIGATION
    private UI_ExploitSelector exploit_selector;
    private void handle_right_navigation_input(InputAction.CallbackContext context)
    {
        // on selectionne l'exploit seulement quand on est dans l'ui pool exploit_wheel
        if (UI_Manager.Instance.IsStacked("exploit_wheel"))
        {
            handle_exploit_selection_input(context.ReadValue<Vector2>());
            return;
        }

        // sinon on essaie de slide
        if (UI_Manager.Instance.IsStacked("settings"))
        {
            handle_sliding_input(context);
            return;
        }
    }
    private void handle_exploit_selection_input(Vector2 direction)
    {
        // transfère l'event seulement quand on est dans l'ui pool exploit_wheel
        if (!UI_Manager.Instance.IsStacked("exploit_wheel")) { return; }

        // récupère l'UI_Exploit Selector
        if (exploit_selector == null) { exploit_selector = UI_Manager.Instance.GetPool("exploit_wheel").GetComponent<UI_ExploitSelector>(); }

        // on transfère l'input
        exploit_selector.HandleSelectionInput(direction);
    }
    private void handle_sliding_input(InputAction.CallbackContext context)
    {
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

    // UI_EXIT
    private void handle_UI_exit_input(InputAction.CallbackContext context)
    {
        // Debug.Log("(UI_InputsController) handling UI exit input : " + context.ReadValue<float>());
        if (context.ReadValue<float>() > 0.5f)
        {
            navigator.OnExitDown();
            return;
        } // only on release
        navigator.OnExit();
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