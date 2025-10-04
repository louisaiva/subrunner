using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class PersoInputsController : MonoBehaviour
{
    private Controller controller;
    private Capable Capable => controller.Capable;

    [Header("INPUTS")]
    public bool InputsDisabled = false;
    [SerializeField] private InputManager input_manager;
    private PersoActions perso_inputs;
    // private event Action<InputAction.CallbackContext> reviveCallback;
    private event Action<InputAction.CallbackContext> dodgeCallback;
    private event Action<InputAction.CallbackContext> attackCallback;
    private event Action<InputAction.CallbackContext> hackCallback;
    private event Action<InputAction.CallbackContext> talkCallback;
    private event Action<InputAction.CallbackContext> useConso1Callback;
    private event Action<InputAction.CallbackContext> useConso2Callback;
    private event Action<InputAction.CallbackContext> useConso3Callback;
    private event Action<InputAction.CallbackContext> useConso4Callback;
    private event Action<InputAction.CallbackContext> interactCallback;

    [Header("Components")]
    public HackableNavigator HackableNavigator { get; private set; }
    public ExploitNavigator ExploitNavigator { get; private set; }

    [Header("Log")]
    public bool log = false;

    private void Start()
    {
        // on récupère les inputs
        initInputs();

        controller = GetComponent<Controller>();

        HackableNavigator = transform.Find("hacking").GetComponent<HackableNavigator>();
        ExploitNavigator = transform.Find("hacking").GetComponent<ExploitNavigator>();

        // mets les callbacks pour stopper correctement les endless inputs
        InputManager.Instance.OnPersoInputsToggled += perso_inputs_true =>
        {
            if (!perso_inputs_true)
            {
                cancel_endless_interact();
                // cancel_endless_hack();
            }
        };

    }

    // INPUTS
    private void initInputs()
    {
        // on récupère les inputs
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        perso_inputs = input_manager.inputs.perso;

        // on crée les callbacks de base
        dodgeCallback = ctx => OnDodge();
        attackCallback = ctx => OnAttack();
        hackCallback = ctx => HandleRunHackInput(ctx);
        talkCallback = ctx => OnRandomTalk();

        // et les callbacks de conso
        useConso1Callback = ctx => OnUseConso(1);
        useConso2Callback = ctx => OnUseConso(2);
        useConso3Callback = ctx => OnUseConso(3);
        useConso4Callback = ctx => OnUseConso(4);

        // et les callbacks d'interaction
        interactCallback = ctx => OnInteract(ctx);

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

        InputsDisabled = true;
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
        if (Capable.HasCapacity<RunCapacity>())
        {
            if (perso_inputs.run.ReadValue<float>() >= input_manager.BUTTON_MAX_THRESHOLD)
            {
                Capable.GetCapacity<RunCapacity>().EnableRun();
            }
            else if (perso_inputs.run.ReadValue<float>() < input_manager.BUTTON_MIN_THRESHOLD)
            {
                Capable.GetCapacity<RunCapacity>().DisableRun();
            }
        }
    }

    // INPUTS
    public void OnAttack()
    {
        // on cherche si on a des armes
        Weapon weapon = null;
        if (Capable is Perso perso)
        {
            weapon = perso.ItemManager.GetWeapon();
            if (weapon != null)
            {
                weapon.GetCapacity<AttackCapacity>().damage = perso.skillManager.GetSkillValue("stat:damage");
            }
        }
        else if (Capable.Inventory != null) { weapon = Capable.Inventory.GetItem<Weapon>(); }
        if (weapon != null) { weapon.Use(Capable); return; }

        // on essaie d'attaquer à la main
        AttackCapacity hitter = Capable.GetCapacity<AttackCapacity>();
        if (hitter != null) { hitter.Use(Capable); return; }

        // on a pas d'armes ni rien, on return juste
        return;
    }
    public void OnRandomTalk()
    {
        TalkCapacity voice = Capable.GetCapacity<TalkCapacity>();
        if (voice == null) { return; } // if we don't have a talk capacity
        if (!voice.Able) { return; } // if we don't have a talk capacity
        voice.Use(Capable);
    }
    private void OnDodge()
    {
        // on vérifie que le perso peut dodge
        // if (!Can("dodge")) { return; }
        // Do("dodge");

        // on récupère les shoes
        Shoes shoes = null;
        if (Capable is Perso perso) { shoes = perso.ItemManager.GetShoes(); }
        else if (Capable.Inventory != null) { shoes = Capable.Inventory.GetItem<Shoes>(); }
        if (shoes == null) { return; } // if the shoes are not set, we return

        // on utilise les shoes
        shoes.Use(Capable);
    }
    public void OnUseConso(int index)
    {

        // on trouve la conso
        Usable conso = null;
        if (Capable is Perso perso) { conso = perso.ItemManager.GetConsumable(index); }
        if (conso == null) { return; }

        // on utilise la conso
        conso.Use(Capable);
    }


    [Header("Interaction input parameters")]
    [SerializeField] private bool waiting_interacting = false; // waiting for the threshold delay before endless_interacting
    [SerializeField] private bool endless_interacting = false; // we interact endlessly
    public void OnInteract(InputAction.CallbackContext context)
    {
        InteractCapacity interactor = Capable.GetCapacity<InteractCapacity>();
        if (interactor == null) { return; } // if we don't have an interact capacity
        if (!interactor.Able) { return; } // if we don't have an interact capacity

        // if we press the button we launch the endless threshold
        if (context.ReadValue<float>() >= 0.5f)
        {
            StopCoroutine(OnEndlessInteract(interactor));
            if (!waiting_interacting && !endless_interacting) { StartCoroutine(OnEndlessInteract(interactor)); }
            return;
        }

        // else we release the button so we direclty interact with it
        if (waiting_interacting || endless_interacting) { cancel_endless_interact(); }
        interactor.Interact();
    }
    public IEnumerator OnEndlessInteract(InteractCapacity interactor)
    {
        // reset parameters
        cancel_endless_interact();

        // wait for threshold
        waiting_interacting = true;
        yield return new WaitForSeconds(InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD);
        if (!waiting_interacting) { yield break; }

        // we start the endless interaction
        endless_interacting = true;
        waiting_interacting = false;
        while (endless_interacting)
        {
            interactor.Interact(endless: true);
            yield return new WaitForSeconds(InputManager.Instance.BUTTON_ENDLESSLY_LONG_DELAY);
        }

        // we stop the endless interaction
        cancel_endless_interact();
    }
    private void cancel_endless_interact()
    {
        waiting_interacting = false;
        endless_interacting = false;
        StopCoroutine(OnEndlessInteract(Capable.GetCapacity<InteractCapacity>()));
    }



    /* [Header("Hack input parameters")]
    [SerializeField] private bool waiting_hacking = false; // waiting for the threshold delay before endless_hacking
    [SerializeField] private bool endless_hacking = false; // we are pressing hack input for a long time
    public void HandleHackInput(InputAction.CallbackContext context)
    {
        if (log) { Debug.Log("(PersoInputsController) hack input received : " + context.ReadValue<float>()); }


        // if we press the button we launch the endless threshold
        if (context.ReadValue<float>() < 0.5f)
        {
            cancel_endless_hack();
            return;
        }

        // if we are in the hud we launch the endless cancel hack routine
        if (UI_Manager.Instance.CurrentPool == "hud" || UI_Manager.Instance.CurrentPool == "device")
        {
            if (!waiting_hacking && !endless_hacking) { StartCoroutine(OnEndlessHack()); }
            return;
        }

        // else if we are in the hacking we launch the hack directly
        if (UI_Manager.Instance.CurrentPool == "hacking") { OnHack(); }
        // if (waiting_hacking || endless_hacking) { cancel_endless_hack(); }
    }
    public IEnumerator OnEndlessHack()
    {
        // reset parameters
        cancel_endless_hack();

        // wait for threshold
        waiting_hacking = true;
        yield return new WaitForSeconds(InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD);
        if (!waiting_hacking) { yield break; }

        // we start the endless hack
        endless_hacking = true;
        waiting_hacking = false;
        int hack_inputs_done = 0;
        while (endless_hacking)
        {
            OnHackCancel(); // we cancel last hack

            // we calculate next duration
            hack_inputs_done++;
            float hack_delay = Mathf.Max(InputManager.Instance.BUTTON_ENDLESSLY_SHORT_DELAY, InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD / hack_inputs_done);
            yield return new WaitForSeconds(hack_delay);
        }

        // we cancel the hack
        cancel_endless_hack();
    }
    private void cancel_endless_hack()
    {
        waiting_hacking = false;
        endless_hacking = false;
        StopCoroutine(OnEndlessHack());
    } */
    public void OnHack()
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
    public void OnHackCancel()
    {
        // On récupère la hack capacity du hackable navigator
        HackCapacity hacker = HackableNavigator.hacker;
        if (hacker == null)
        {
            if (log) { Debug.Log("(PersoInputsController) " + name + " tried to cancel hack but it has no HackCapacity"); }
            // cancel_endless_hack();
            return;
        }

        // on hack
        if (log) { Debug.Log("(PersoInputsController) " + name + " cancels hack on " + HackableNavigator.name); }
        hacker.CancelLastHack();
    }


    // [Header("Run Hack input")]
    public void HandleRunHackInput(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();

        // 1 - if we are hacking we run the hack
        if (UI_Manager.Instance.CurrentPool == "hacking")
        {
            // we check if the input is > 0.5 (we down the trigger -> we run hack), or not
            if (input > 0.5f) { OnHack(); }
            return;
        }

        // 2 - if we are in the hud / device we show the exploit wheel to select the exploit
        if (!new List<string> { "hud", "device", "exploit_wheel" }.Contains(UI_Manager.Instance.CurrentPool)) { return; }
        ExploitNavigator.HandleExploitWheelInput(input);
    }


}