using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class PersoInputsController : InputController
{
    public bool log = false;

    // Controller thing
    private Controller controller;
    private Capable Capable => controller.Capable;

    [Header("INPUTS")]
    public bool InputsDisabled = false;
    protected InputManager input_manager;
    private PersoActions perso_inputs;
    private event Action<InputAction.CallbackContext> dodgeCallback;
    private event Action<InputAction.CallbackContext> attackCallback;
    private event Action<InputAction.CallbackContext> hackCallback;
    private event Action<InputAction.CallbackContext> select_hackableCallback;
    private event Action<InputAction.CallbackContext> hackMouseCallback;
    private event Action<InputAction.CallbackContext> exploitMouseCallback;
    private event Action<InputAction.CallbackContext> talkCallback;
    private event Action<InputAction.CallbackContext> useConso1Callback;
    private event Action<InputAction.CallbackContext> useConso2Callback;
    private event Action<InputAction.CallbackContext> useConso3Callback;
    private event Action<InputAction.CallbackContext> useConso4Callback;
    private event Action<InputAction.CallbackContext> interactCallback;

    [Header("Components")]
    public HackableNavigator HackableNavigator { get; private set; }
    public ExploitNavigator ExploitNavigator { get; private set; }


    private void Start()
    {
        // on récupère les inputs
        initInputs();

        controller = GetComponent<Controller>();

        HackableNavigator = transform.Find("hacking").GetComponent<HackableNavigator>();
        ExploitNavigator = transform.Find("hacking").GetComponent<ExploitNavigator>();

        // on crée les hold inputs
        add_endless_input(new EndlessInput<float>("interact", perso_inputs.interact,
                threshold: InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD,
                repeat: InputManager.Instance.BUTTON_ENDLESSLY_LONG_DELAY,
                unscaled_time: false))
                .OnEndless += ctx => OnInteract(true); // ajoute le callback directement
    }

    // INPUTS
    private void initInputs()
    {
        // on récupère les inputs
        input_manager = InputManager.Instance;
        perso_inputs = input_manager.inputs.perso;

        // on crée les callbacks de base
        dodgeCallback = ctx => OnDodge();
        attackCallback = ctx => OnAttack();
        talkCallback = ctx => OnRandomTalk();

        // et les callbacks de conso
        useConso1Callback = ctx => OnUseConso(1);
        useConso2Callback = ctx => OnUseConso(2);
        useConso3Callback = ctx => OnUseConso(3);
        useConso4Callback = ctx => OnUseConso(4);


        // ici c'est les inputs qui prennent en charge hold input
        interactCallback = ctx => HandleInteractInput(ctx);

        // ici c les inputs qui ont pas besoin d'hold input
        hackCallback = ctx => HandleRunHackInput(ctx);
        hackMouseCallback = ctx => handle_mouse_hack_selection(ctx);
        exploitMouseCallback = ctx => handle_exploit_wheel_mouse(ctx);

        // ensuite les callbacks statiques (ne se désactivent pas quand )
        // perso_inputs.select_hackable.performed += ctx => { handle_select_hack_target_input(ctx.ReadValue<Vector2>()); };
        select_hackableCallback = ctx => { handle_select_hack_target_input(ctx.ReadValue<Vector2>()); };

        EnableInputs();
    }
    public void EnableInputs()
    {
        perso_inputs.dodge.performed += dodgeCallback;
        perso_inputs.attack.performed += attackCallback;
        perso_inputs.hack.performed += hackCallback;
        perso_inputs.randomTalk.performed += talkCallback;
        perso_inputs.conso1.performed += useConso1Callback;
        perso_inputs.conso2.performed += useConso2Callback;
        perso_inputs.conso3.performed += useConso3Callback;
        perso_inputs.conso4.performed += useConso4Callback;
        perso_inputs.interact.performed += interactCallback;
        perso_inputs.select_hackable.performed += select_hackableCallback;
        perso_inputs.mouse_hack.performed += hackMouseCallback;
        perso_inputs.exploit_wheel_mouse.performed += exploitMouseCallback;

        InputsDisabled = false;
    }
    public void DisableInputs()
    {
        perso_inputs.dodge.performed -= dodgeCallback;
        perso_inputs.attack.performed -= attackCallback;
        perso_inputs.hack.performed -= hackCallback;
        perso_inputs.randomTalk.performed -= talkCallback;
        perso_inputs.conso1.performed -= useConso1Callback;
        perso_inputs.conso2.performed -= useConso2Callback;
        perso_inputs.conso3.performed -= useConso3Callback;
        perso_inputs.conso4.performed -= useConso4Callback;
        perso_inputs.interact.performed -= interactCallback;
        perso_inputs.select_hackable.performed -= select_hackableCallback;
        perso_inputs.mouse_hack.performed -= hackMouseCallback;
        perso_inputs.exploit_wheel_mouse.performed -= exploitMouseCallback;

        InputsDisabled = true;
    }
    private void OnDestroy()
    {
        DisableInputs();
    }


    // UPDATE
    private void Update()
    {

        // si les perso_inputs sont desactivés on return (comme ça on garde la même vitesse)
        // if (!perso_inputs.enabled) { return; }

        // walk
        if (Capable.HasCapacity<WalkCapacity>())
        {
            Vector2 raw_inputs = InputManager.Instance.MovementRawInputs;

            // we check if the raw inputs are below the deadzone
            raw_inputs.x = Mathf.Abs(raw_inputs.x) < input_manager.JOYSTICK_MIN_THRESHOLD ? 0f : raw_inputs.x;
            raw_inputs.y = Mathf.Abs(raw_inputs.y) < input_manager.JOYSTICK_MIN_THRESHOLD ? 0f : raw_inputs.y;

            // we normalize the inputs
            Capable.Orientation = raw_inputs.normalized;

            // we set the walk_capacity.walk_percentage_target
            Capable.GetCapacity<WalkCapacity>().walk_percentage_target = raw_inputs.magnitude;

            // Debug.Log("inputs : " + inputs + " / raw_inputs : " + raw_inputs + " / inputs_magnitude : " + raw_inputs.magnitude);
        }

        // run
        if (Capable.TryGetCapacity(out WalkCapacity walker))
        {
            if (perso_inputs.run.ReadValue<float>() >= input_manager.BUTTON_MAX_THRESHOLD)
            {
                walker.EnableRun();
            }
            else if (perso_inputs.run.ReadValue<float>() < input_manager.BUTTON_MIN_THRESHOLD)
            {
                walker.DisableRun();
            }
        }
    }










    // HANDLE INPUTS
    public void OnAttack()
    {
        // on cherche si on a un usable dans la weapon_stack
        // (on l'appelle weapon mais ça peut etre n'importe quel usable en vrai)
        Usable weapon = null;
        if (Capable.Inventory != null) { weapon = Capable.Inventory.GetWeapon(); }
        if (weapon != null) { weapon.Use(Capable); return; }

        // on essaie d'attaquer à la main
        AttackCapacity hitter = Capable.GetCapacity<AttackCapacity>();
        if (hitter != null) { hitter.Use(Capable); return; }

        // on a pas d'armes ni rien, on return juste
        return;
    }
    private void OnDodge()
    {
        // on récupère l'usable qui est dans le shoes_stack de l'inventaire
        Usable shoes = null; // on l'appelle shoes pour simplifier la nomenclature mais ça peut etre completement autre chose
        if (Capable.Inventory != null) { shoes = Capable.Inventory.GetShoes(); }
        if (shoes == null) { return; } // if the shoes are not set, we return

        // on utilise les shoes
        shoes.Use(Capable);
    }
    public void OnUseConso(int index)
    {
        // on trouve la conso
        Usable conso = null;
        if (Capable.Inventory != null) { conso = Capable.Inventory.GetConso(index); }
        if (conso == null) { return; }

        // on utilise la conso
        conso.Use(Capable);
    }
    public void OnRandomTalk()
    {
        TalkCapacity voice = Capable.GetCapacity<TalkCapacity>();
        if (voice == null) { return; } // if we don't have a talk capacity
        voice.Use(Capable);
    }
    
    // INTERACT
    public void HandleInteractInput(InputAction.CallbackContext context)
    {
        // if we press the button we launch the endless input
        if (context.ReadValue<float>() >= 0.5f)
        {
            get_endless_input<float>("interact").OnInput(context);
            return;
        }

        // else we release the button so we direclty interact with it
        OnInteract();
    }
    private void OnInteract(bool endless = false)
    {
        InteractCapacity interactor = Capable.GetCapacity<InteractCapacity>();
        if (interactor == null) { return; } // if we don't have an interact capacity
        // if (!interactor.Able) { return; } // if we don't have an interact capacity
        interactor.Interact(endless: endless);
    }

    // RUN HACK
    public void HandleRunHackInput(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();

        // 1 - if we are hacking we run the hack
        if (Controller.Instance.HackableNavigator.IsSelecting)
        {
            // we check if the input is > 0.5 (we down the trigger -> we run hack), or not
            if (input > 0.5f) { OnHack(); }
            return;
        }

        // 2 - if we are in the hud / device we show the exploit wheel to select the exploit
        if (UI_Manager.Instance.IsOnHUD() || UI_Manager.Instance.IsStacked("exploit_wheel"))
        {
            ExploitNavigator.HandleExploitWheelInput(input);
            return;
        }
    }
    private void OnHack()
    {
        // On récupère la hack capacity du hackable navigator
        HackCapacity hacker = HackableNavigator.hacker;
        if (hacker == null) { if (log) { Debug.Log("(PersoInputsController) " + name + " tried to hack " + HackableNavigator.name + " but it has no HackCapacity"); } return; } // if the hacker is not set, we return

        ConnectCapacity connector = Controller.Instance.Capable.Connector;
        if (connector == null) { if (log) { Debug.Log("(PersoInputsController) " + name + " tried to hack " + HackableNavigator.name + " but it has no Connector"); } return; } // if the connector is not set,

        // on hack
        if (log) { Debug.Log("(PersoInputsController) " + name + " launches hack on " + HackableNavigator.name); }
        hacker.SetConnector(connector);
        hacker.Use(Capable);
    }

    // SELECT HACK TARGET
    private void handle_select_hack_target_input(Vector2 input)
    {
        // we activate the hackable navigator when input is pressed > 0.5
        // and disable it when released < 0.5
        if (input.magnitude < InputManager.Instance.JOYSTICK_MIN_THRESHOLD || Perso.Instance.Device == null)
        {
            HackableNavigator.Disable();
            return;
        }

        select_hack(input);
    }
    private void select_hack(Vector2 direction)
    {
        // if we are not on the hud we don't hack
        if (!UI_Manager.Instance.IsOnHUD()) { HackableNavigator.Disable(); return; }

        // show the input
        Debug.Log("(PersoInputsController) selecting hack target : magnitude is " + direction.magnitude + " / direction is " + direction.normalized);

        HackableNavigator.Enable();
        HackableNavigator.HandleHackNavigationInput(direction);
    }


    // MOUSE HACKING INPUTS
    public void handle_mouse_hack_selection(InputAction.CallbackContext context)
    {
        if (Perso.Instance.Device == null) { HackableNavigator.Disable(); return; }
        
        // checks if we are releasing the right button while connected to a target -> we run the hack
        if (Input.GetMouseButtonUp(1) && Controller.Instance.HackableNavigator.IsConnected) { OnHack(); return; }

        // else if we are not downing the right button we are not selecting a hack target anymore
        if (!Input.GetMouseButton(1))
        {
            HackableNavigator.Disable();
            return;
        }

        // get the direction
        Vector2 direction = calculate_mouse_direction();

        // finally we select the target
        select_hack(direction);
    }
    private void handle_exploit_wheel_mouse(InputAction.CallbackContext context)
    {
        // checks if we are pressing the middle button & have a device
        if (!Input.GetMouseButton(2) || Perso.Instance.Device == null)
        {
            ExploitNavigator.HandleExploitWheelInput(0f);
            Cursor.visible = true;
            return;
        }

        // check if we just clicked mid button
        if (Input.GetMouseButtonDown(2))
        {
            Cursor.visible = false;
            ExploitNavigator.HandleExploitWheelInput(1f);
            return;
        }

        // else we are holding & moving the mid button -> we check the threshold
        if (context.valueType != typeof(Vector2)) { return; }
        if (context.ReadValue<Vector2>().magnitude < InputManager.Instance.MOUSE_DELTA_BIG_THRESHOLD) { return; }

        // we transmit the delta position to UI_ExploitSelector
        UI_ExploitSelector exploit_selector = UI_Manager.Instance.GetPool("exploit_wheel").GetComponent<UI_ExploitSelector>();
        exploit_selector.HandleSelectionInput(context.ReadValue<Vector2>().normalized);
    }
    private Vector2 calculate_mouse_direction()
    {
        Vector2 mouse_position = Mouse.current.position.ReadValue();
        Vector2 distance = Camera.main.ScreenToWorldPoint(mouse_position) - Controller.Instance.Capable.transform.position;
        return distance.normalized;
    }
}