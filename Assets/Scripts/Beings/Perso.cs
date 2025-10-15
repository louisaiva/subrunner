using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class Perso : Being, LaptopHacker
{
    public static int deaths = 0; // nombre de morts du perso
    public static Perso Instance { get; private set; }
    public override ConnectCapacity Connector => Device?.Connector;

    [Header("PERSO")]
    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 100;

    // private GameObject floating_text_prefab;
    private GameObject cam;

    [Header("SKILLS")]
    public SkillManager skillManager;
    

    [Header("METAMORPH")]
    [SerializeField] private List<string> metamorph_skins = new List<string>() { "perso", "cat", "zombo", "robot", "apple", "fridge", "small_laptop" };


    [Header("Items")]
    public ItemManager ItemManager
    {
        get
        {
            if (_itemManager == null)
            {
                _itemManager = GameObject.Find("/utils/item_manager").GetComponent<ItemManager>();
            }
            return _itemManager;
        }
    }
    private ItemManager _itemManager;


    [Header("Devices")]
    public Laptop _laptop = null; // laptop item in our inventory on the "laptop" slot
    public Laptop Laptop
    {
        get { return _laptop; }
        set
        {
            if (_laptop == value) { return; }
            _laptop = value;
            OnDeviceChanged?.Invoke(Device);
        }
    }
    private Computer _computer = null; // computer we are currently interacting with (null if none)
    public Computer Computer
    {
        get { return _computer; }
        set
        {
            if (_computer == value) { return; }
            
            _computer = value;
            OnDeviceChanged?.Invoke(Device);
        }
    }
    public Device Device
    {
        get
        {
            // if we are interacting with a computer we go with the computer
            if (_computer != null) { return _computer; }

            // if we have a laptop we return the laptop
            return Laptop;
        }
    }
    public Action<Device> OnDeviceChanged; // callback pour quand le device du perso change



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
        // initInputs();

        // on start de d'habitude
        base.Start();

        // todo move this in awake ?

        // ON RECUP DES TRUCS
        cam = GameObject.Find("/cam_follow/cam");
        skillManager = GetComponentInChildren<SkillManager>();

        // ItemManager = GameObject.Find("/utils/ItemManager").GetComponent<ItemManager>();

        //
        // floating_text_prefab = Resources.Load("prefabs/ui/floating_text") as GameObject;

        // on s'enregistre en tant que trigger dans l'XPProvider particle system
        var trigger_particle_module = XPProvider.Instance.GetComponent<ParticleSystem>().trigger;
        trigger_particle_module.SetCollider(0, body_collider);

        // met le callback de device pour le laptop
        UI_LaptopItemSlot.Instance.OnItemChanged += (List<Item> items) => Laptop = items.Count > 0 ? items[0] as Laptop : null;
    }


    private string quest_text = "mission 1 :\nfind the\nELEVATOR";
    void showQuest()
    {
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText(quest_text, transform.position + new Vector3(0, 0.5f, 0), "yellow");
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
        UI_Manager.Instance.SwitchTo("level_up", force: true);

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

        // on désactive le PersoInputsController
        Controller.Instance.ResetCapableTarget();
        Controller.Instance.PIC.DisableInputs();

        // on switch au game_over panel
        UI_Manager.Instance.SwitchTo("game_over",force:true);

        // on désactive plein de choses
        Destroy(GetComponent<SeeThroughHandler>());
        Destroy(transform.Find("body").GetComponent<ParticleSystemForceField>());

        deaths += 1; // on incrémente le nombre de morts du perso
    }
}