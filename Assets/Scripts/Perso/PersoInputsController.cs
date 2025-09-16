using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class PersoInputsController : Singleton<PersoInputsController>
{
    public Capable Capable
    {
        get
        {
            if (_capable == null)
                _capable = transform.parent.GetComponent<Capable>();
            return _capable;
        }
    }
    [SerializeField] private Capable _capable;

    [Header("INPUTS")]
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

    private void Start()
    {
        // on récupère les inputs
        initInputs();
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
        /* Laptop laptop = Laptop;
        if (laptop == null) { return; } // if the laptop is not set, we return */

        // UseItem("hardware:laptop");
        // if (UI_LaptopItemSlot.Instance == null || !UI_LaptopItemSlot.Instance.HasLaptop) { return; } // if the laptop is not set, we return
        // UI_LaptopItemSlot.Instance.Laptop.Use(this);

        // on récupère le laptop
        Laptop laptop = null;
        if (Capable is Perso perso) { laptop = perso.ItemManager.GetLaptop(); }
        else { laptop = Capable.Inventory.GetItem<Laptop>(); }
        if (laptop == null) { return; }

        // on utilise le laptop
        laptop.Use(Capable);
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


    // CHANGE CAPABLE TARGET
    public void ChangeCapableTarget(Capable new_target, float duration = -888f)
    {


        // reset les inputs de l'ancien capable
        Capable.ClearInputs();
        if (Capable.GetCapacity<WalkCapacity>() != null)
        {
            Capable.GetCapacity<WalkCapacity>().walk_percentage_target = 0f;
        }

        // reset le behaviour
        if (Capable is IA old_ia)
        {
            // on réactive l'ancien Brain si le capable actuel est une ia
            old_ia.Brain.gameObject.SetActive(true);

            // on remet le tag
            old_ia.gameObject.tag = old_ia.BaseTag;
        }



        // on déplace le script sur le gameobject capable
        transform.parent = new_target.transform;
        transform.localPosition = Vector3.zero;

        // on change le capable
        _capable = new_target;

        // on refresh la cam
        CameraFollow.Instance.RefreshTarget(new_target);

        // si on a une durée, on reviens au perso après la durée
        CancelInvoke("ResetCapableTarget");
        if (duration != -888f) { Invoke("ResetCapableTarget", duration); }

        // on désactive le Brain si le nouveau capable est un IA
        if (new_target is IA ia)
        {
            ia.Brain.gameObject.SetActive(false);

            // on remet le tag
            ia.gameObject.tag = "Controlled";
        }
    }
    public void ResetCapableTarget()
    {
        CancelInvoke("ResetCapableTarget");
        ChangeCapableTarget(Perso.Instance);
    }


    // ENABLE / DISABLE INPUTS
    public bool Disabled;
    public void DisableInputs()
    {
        perso_inputs.dodge.performed -= dodgeCallback;
        perso_inputs.attack.performed -= attackCallback;
        perso_inputs.randomTalk.performed -= talkCallback;
        perso_inputs.conso1.performed -= useConso1Callback;
        perso_inputs.conso2.performed -= useConso2Callback;
        perso_inputs.conso3.performed -= useConso3Callback;
        perso_inputs.conso4.performed -= useConso4Callback;

        Disabled = true;
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

        Disabled = false;
    }

}