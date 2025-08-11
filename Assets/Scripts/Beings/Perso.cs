using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class Perso : Being, Hacker
{
    public static int deaths = 0; // nombre de morts du perso
    public static Perso Instance { get; private set; }
    
    [Header("PERSO")]
    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 100;

    private GameObject floating_text_prefab;
    private GameObject cam;

    public Room current_room { get; set; }

    public HackableNavigator HackableNavigator { get; private set; }


    [Header("SKILLS")]
    public SkillManager skillManager;
    // public UI_Fullmap big_map;

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



    [Header("METAMORPH")]
    [SerializeField] private List<string> metamorph_skins = new List<string>() { "perso", "cat", "zombo", "robot", "apple", "fridge", "small_laptop" };


    [Header("Items")]
    private ItemManager item_manager;
    public Laptop Laptop
    {
        get
        {
            if (item_manager == null) { return null; }
            Laptop laptop = item_manager.GetLaptop();
            if (laptop == null) { return null; }
            return laptop;
        }
    }



    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        // Singleton logic
        if (Instance != null) { Destroy(Instance.gameObject); }
        Instance = this;
    }

    // START
    protected override void Start()
    {
        // on récupère les inputs
        initInputs();

        // on start de d'habitude
        base.Start();

        // ON RECUP DES TRUCS
        cam = GameObject.Find("/cam_follow/cam");
        skillManager = GetComponentInChildren<SkillManager>();

        item_manager = GameObject.Find("/utils/item_manager").GetComponent<ItemManager>();

        //
        floating_text_prefab = Resources.Load("prefabs/ui/floating_text") as GameObject;

        // on s'enregistre en tant que trigger dans l'XPProvider particle system
        var trigger_particle_module = XPProvider.Instance.GetComponent<ParticleSystem>().trigger;
        trigger_particle_module.SetCollider(0, body_collider);

        HackableNavigator = transform.Find("processor").GetComponent<HackableNavigator>();
    }


    private string quest_text = "mission 1 :\nfind the\nELEVATOR";
    void showQuest()
    {
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText(quest_text, transform.position + new Vector3(0, 0.5f, 0), "yellow");
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

    // CAPACITES
    protected override void Update()
    {
        base.Update();

        // si les perso_inputs sont desactivés on return (comme ça on garde la même vitesse)
        // if (!perso_inputs.enabled) { return; }

        // walk
        if (HasCapacity<WalkCapacity>())
        {
            Vector2 raw_inputs = InputManager.Instance.MovementRawInputs;
            
            // we check if the raw inputs are below the deadzone
            raw_inputs.x = Mathf.Abs(raw_inputs.x) < input_manager.JOYSTICK_MIN_THRESHOLD ? 0f : raw_inputs.x;
            raw_inputs.y = Mathf.Abs(raw_inputs.y) < input_manager.JOYSTICK_MIN_THRESHOLD ? 0f : raw_inputs.y;

            // we normalize the inputs
            Orientation = raw_inputs.normalized;

            // we set the walk_capacity.walk_percentage_target
            GetCapacity<WalkCapacity>().walk_percentage_target = raw_inputs.magnitude;

            // Debug.Log("inputs : " + inputs + " / raw_inputs : " + raw_inputs + " / inputs_magnitude : " + raw_inputs.magnitude);
        }

        // run
        if (HasCapacity<RunCapacity>())
        {
            if (perso_inputs.run.ReadValue<float>() >= input_manager.BUTTON_MAX_THRESHOLD)
            {
                GetCapacity<RunCapacity>().EnableRun();
            }
            else if (perso_inputs.run.ReadValue<float>() < input_manager.BUTTON_MIN_THRESHOLD)
            {
                GetCapacity<RunCapacity>().DisableRun();
            }
        }
    }


    // METAMORPH
    public void Metamorph()
    {
        // checks which skins we have
        string skin = anim_player.Skin;

        // checks if we are a ghost
        if (skin == "ghost") { ToggleGhost(); }

        // we roll through the list
        int index = metamorph_skins.IndexOf(skin);
        if (index == metamorph_skins.Count - 1)
        {
            index = 0; // if we are at the end, we go back to the start
        }
        else
        {
            index += 1; // otherwise we go to the next skin
        }

        // we set the new skin
        anim_player.Skin = metamorph_skins[index];
    }
    public void ToggleGhost()
    {
        if (anim_player.Skin != "ghost")
        {
            // on change le skin
            anim_player.Skin = "ghost";

            // on applique l'Effect Ghost & Invisible
            AddEffect(Effect.Ghost, -888f);
            AddEffect(Effect.Invisible, -888f);
        }
        else
        {
            // on remet le skin de base
            anim_player.Skin = "perso";

            // on enleve l'Effect Ghost & Invisible
            RemoveEffect(Effect.Ghost);
            RemoveEffect(Effect.Invisible);
        }
    }

    // XP
    public void addXP(int count)
    {
        xp += count;
        total_xp += count;
        if (xp >= xp_to_next_level)
        {
            levelUp();
        }
    }
    private void levelUp()
    {
        level += 1;
        xp = 0;
        xp_to_next_level = (int)(xp_to_next_level * 1.5f);

        Debug.Log("LEVEL UP ! level " + level);

        // on ouvre le level up menu
        GameObject.Find("/ui").GetComponent<UI_Manager>().SwitchTo("level_up");

        // on augmente x1.5 l'attaque
        if (HasCapacity("attack"))
        {
            transform.Find("attack").GetComponent<AttackCapacity>().damage *= 1.5f;
        }

        // on affiche un texte de level up
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("LEVEL " + level.ToString(), transform.position + new Vector3(0, 0.5f, 0), "yellow");

    }

    // DAMAGE
    public override bool take_damage(float damage, Force knockback = null)
    {
        bool dmg_status = base.take_damage(damage, knockback);
        if (!dmg_status) { return false; }

        // we make a little screenshake if perso
        float shake_magnitude = damage / life;
        cam.GetComponent<CameraShaker>().Shake(shake_magnitude);

        return true;
    }
    public override void Die()
    {
        Debug.Log("YOU DIED");

        // on affiche un floating text
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("YOU DIED", transform.position + new Vector3(0, 0.5f, 0), "red");

        // on enlève les callbacks
        perso_inputs.dodge.performed -= dodgeCallback;
        perso_inputs.attack.performed -= attackCallback;
        perso_inputs.randomTalk.performed -= talkCallback;
        perso_inputs.conso1.performed -= useConso1Callback;
        perso_inputs.conso2.performed -= useConso2Callback;
        perso_inputs.conso3.performed -= useConso3Callback;
        perso_inputs.conso4.performed -= useConso4Callback;

        // on switch au game_over panel
        UI_Manager.Instance.SwitchTo("game_over", override_duration: 3f);

        // on désactive plein de choses
        Destroy(GetComponent<SeeThroughHandler>());
        Destroy(transform.Find("body").GetComponent<ParticleSystemForceField>());

        deaths += 1; // on incrémente le nombre de morts du perso
    }

    // INPUTS
    public void OnAttack()
    {
        // if (HasEffect(Effect.Stunned)) { return; }
        // if (anim_player.current_anim.capacity == "attack") { return; }

        // on met à jour la valeur de damage
        /* if (HasItem("weapon:katana", out Item katana))
        {
            katana.GetCapacity<AttackCapacity>().damage = skillManager.GetSkillValue("stat:damage");
        }
        else { return; }

        // on utilise l'item weapon:katana
        UseItem("weapon:katana"); */

        // on récupère le current weapon
        Weapon current_weapon = item_manager.GetWeapon();
        if (current_weapon == null) { return; } // if the weapon is not set, we return

        // on met à jour la valeur de damage
        current_weapon.GetCapacity<AttackCapacity>().damage = skillManager.GetSkillValue("stat:damage");

        // on utilise l'attaque
        current_weapon.Use(this);
    }
    public void OnRandomTalk()
    {
        if (Can("talk") && !HasEffect(Effect.Stunned))
        {
            Do("talk");
        }
    }
    private void OnDodge()
    {
        // on vérifie que le perso peut dodge
        if (!Can("dodge")) { return; }
        Do("dodge");
    }
    public void OnHack()
    {
        /* Laptop laptop = Laptop;
        if (laptop == null) { return; } // if the laptop is not set, we return */

        // UseItem("hardware:laptop");
        // if (UI_LaptopItemSlot.Instance == null || !UI_LaptopItemSlot.Instance.HasLaptop) { return; } // if the laptop is not set, we return
        // UI_LaptopItemSlot.Instance.Laptop.Use(this);
        Laptop laptop = item_manager.GetLaptop();
        if (laptop == null) { return; }
        laptop.Use(this);
    }

    // CONSOMMABLES INPUTS
    public void OnUseConso(int index)
    {
        Usable conso = item_manager.GetConsumable(index);
        if (conso == null) { return; }
        conso.Use(this);
    }


    // ! deprecated HACK

    [Header("HACKIN")]
    public float bits = 8f; // bits = mana (lance des sorts de hacks)
    public int max_bits = 8;
    public void addBits(int count)
    {
        bits += count;
        if (bits > max_bits) { bits = max_bits; }
    }
    
}