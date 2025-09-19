using System;
using System.Collections;
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

        // mets les callbacks pour stopper correctement les endless inputs
        InputManager.Instance.OnPersoInputsToggled += perso_inputs_true => { if (!perso_inputs_true) { cancel_endless_interact(); } };

    }

    // INPUTS
    private void initInputs()
    {
        // on récupère les inputs
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        perso_inputs = input_manager.inputs.perso;

        // on set les callbacks
        dodgeCallback = ctx => OnDodge();
        attackCallback = ctx => OnAttack();
        talkCallback = ctx => OnRandomTalk();
        perso_inputs.dodge.performed += dodgeCallback;
        perso_inputs.attack.performed += attackCallback;
        perso_inputs.randomTalk.performed += talkCallback;

        // et les callbacks de conso
        useConso1Callback = ctx => OnUseConso(1);
        useConso2Callback = ctx => OnUseConso(2);
        useConso3Callback = ctx => OnUseConso(3);
        useConso4Callback = ctx => OnUseConso(4);
        perso_inputs.conso1.performed += useConso1Callback;
        perso_inputs.conso2.performed += useConso2Callback;
        perso_inputs.conso3.performed += useConso3Callback;
        perso_inputs.conso4.performed += useConso4Callback;

        // et les callbacks d'interaction
        interactCallback = ctx => OnInteract(ctx);
        perso_inputs.interact.performed += interactCallback;

        EnableInputs();
    }
    public void EnableInputs()
    {
        perso_inputs.dodge.performed += dodgeCallback;
        perso_inputs.attack.performed += attackCallback;
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
    public void OnHack()
    {


        // On récupère la hack capacity du hackable navigator
        HackCapacity hacker = HackableNavigator.hacker;
        if (hacker == null) { return; } // if the hacker is not set, we return

        ConnectCapacity connector = Controller.Instance.Capable.Connector;
        if (connector == null) { return; } // if the connector is not set,

        hacker.SetConnector(connector);
        hacker.Use(Capable);
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
            StartCoroutine(OnEndlessInteract(interactor));
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
            interactor.Interact(endless:true);
            yield return new WaitForSeconds(InputManager.Instance.BUTTON_ENDLESSLY_LONG_DELAY);
        }

        // we stop the endless interaction
        cancel_endless_interact();
    }
    private void cancel_endless_interact()
    {
        waiting_interacting = false;
        endless_interacting = false;
    }


}