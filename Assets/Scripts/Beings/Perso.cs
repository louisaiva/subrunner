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
    public override ConnectCapacity Connector => Device?.Connector;

    [Header("PERSO")]
    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 100;

    // private GameObject floating_text_prefab;
    private GameObject cam;

    [Header("SETTINGS")]
    private StringSetting skin_setting;
    private Setting ghost_setting;

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

            Device old_device = Device;
            Laptop old_laptop = _laptop;
            _laptop = value;

            if (value == null && _computer == null)
            {
                // if we are here we successfully dropped item
                // we check if we dropped a laptop that was using trojan / cyborg_puppet since we don't want them to
                // continue if we are not here to stop them !!!! (if perso is not controlled he can't grab back the laptop)
                // and if he can't grab the laptop he can't cancel the hack
                // so it is stuck in the trojan / cyborg
                old_laptop.Hacker.CancelControlHacks();
                OnDeviceRemoved?.Invoke(old_laptop);
            }
            else if (value != null)
            {
                if (old_device != null)
                {
                    old_device.Hacker.CancelControlHacks();
                    OnDeviceRemoved?.Invoke(old_device);
                }
                OnDeviceGranted?.Invoke(value);
            }

        }
    }
    private Computer _computer = null; // computer we are currently interacting with (null if none)
    public Computer Computer
    {
        get { return _computer; }
        set
        {
            if (_computer == value) { return; }

            Device old_device = Device;
            Computer old_computer = _computer;
            _computer = value;

            if (value == null)
            {
                old_computer.Hacker.CancelControlHacks();
                OnDeviceRemoved?.Invoke(old_computer);
                if (_laptop != null) { OnDeviceGranted?.Invoke(_laptop); }
            }
            else if (value != null)
            {
                if (old_device != null)
                {
                    old_device.Hacker.CancelControlHacks();
                    OnDeviceRemoved?.Invoke(old_device);
                }
                OnDeviceGranted?.Invoke(value);
            }
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
    public Action<Device> OnDeviceRemoved { get; set; } = delegate { };
    public Action<Device> OnDeviceGranted { get; set; } = delegate { };



    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        // Singleton logic
        if (Instance != null) { Destroy(Instance.gameObject); }
        Instance = this;

        // set des logs
        OnDeviceGranted += (Device new_device) =>
        {
            /* if (debug) {  */Debug.Log($"(Perso) new device set : {(new_device is Laptop laptop ? laptop.Reference : new_device.name)}"); /* } */
        };
        OnDeviceRemoved += (Device old_device) =>
        {
            /* if (debug) {  */
            Debug.Log($"(Perso) Device removed: {(old_device is Laptop laptop ? laptop.Reference : old_device.name)}"); /* } */
        };

        // ON RECUP DES TRUCS
        cam = GameObject.Find("/cam_follow/cam");
        skillManager = GetComponentInChildren<SkillManager>();
    }

    // START & CALLBACKS
    protected override void Start()
    {
        // on start de d'habitude
        base.Start();

        // on s'enregistre en tant que trigger dans l'XPProvider particle system
        var trigger_particle_module = XPProvider.Instance.GetComponent<ParticleSystem>().trigger;
        trigger_particle_module.SetCollider(0, body_collider);

        // mets les callbacks
        set_callbacks();

        // on met le skin en fonction du settings skin
        skin_setting = SettingsManager.Instance.GetSetting("skin") as StringSetting;
        if (skin_setting != null)
        {
            SetSkin(skin_setting.ToString());
            skin_setting.OnStringChanged += SetSkin;
        }

        // on met le ghost en fonction du settings ghost
        ghost_setting = SettingsManager.Instance.GetSetting("ghost_mode");
        if (ghost_setting != null)
        {
            set_ghost(ghost_setting.value >= 0.5f);
            ghost_setting.OnValueChanged += set_ghost;
        }
    }
    private void set_callbacks()
    {
        // todo plutot bouger ça dans le controller si on veut pouvoir afficher l'inventaire des bots ?

        // met les callbacks de notif
        UI_Manager.Instance.GetPool("hud").GetComponent<UI_HUD>().Notifier.SetCallbacks();

        // on met le callback de pour afficher ui_hacking
        UI_Hacking hacking_pool = UI_Manager.Instance.GetPool("hacking").GetComponent<UI_Hacking>();
        Instance.OnDeviceGranted += hacking_pool.HandleDeviceGranted;
        Instance.OnDeviceRemoved += hacking_pool.HandleDeviceRemoved;

        // callbacks de ui_running hacks viewer
        UI_RunningHacksViewer running_hacks_viewer = hacking_pool.transform.GetComponentInChildren<UI_RunningHacksViewer>(includeInactive: true);
        Instance.OnDeviceGranted += running_hacks_viewer.HandleDeviceGranted;
        Instance.OnDeviceRemoved += running_hacks_viewer.HandleDeviceRemoved;

        // et du cores viewer
        UI_CoresViewer cores_viewer = hacking_pool.transform.GetComponentInChildren<UI_CoresViewer>(includeInactive: true);
        Instance.OnDeviceGranted += cores_viewer.HandleDeviceGranted;
        Instance.OnDeviceRemoved += cores_viewer.HandleDeviceRemoved;
    }
    private void remove_callbacks()
    {
        // enleve les callbacks de notif
        UI_Manager.Instance.GetPool("hud").GetComponent<UI_HUD>().Notifier.RemoveCallbacks();

        // on enleve les callbacks de pour afficher ui_hacking
        UI_Hacking hacking_pool = UI_Manager.Instance.GetPool("hacking").GetComponent<UI_Hacking>();
        Instance.OnDeviceGranted -= hacking_pool.HandleDeviceGranted;
        Instance.OnDeviceRemoved -= hacking_pool.HandleDeviceRemoved;

        // callbacks de ui_running hacks viewer
        UI_RunningHacksViewer running_hacks_viewer = hacking_pool.transform.GetComponentInChildren<UI_RunningHacksViewer>(includeInactive: true);
        Instance.OnDeviceGranted -= running_hacks_viewer.HandleDeviceGranted;
        Instance.OnDeviceRemoved -= running_hacks_viewer.HandleDeviceRemoved;
        
        // et du cores viewer
        UI_CoresViewer cores_viewer = hacking_pool.transform.GetComponentInChildren<UI_CoresViewer>(includeInactive: true);
        Instance.OnDeviceGranted -= cores_viewer.HandleDeviceGranted;
        Instance.OnDeviceRemoved -= cores_viewer.HandleDeviceRemoved;

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
    public void SetSkin(string skin_name)
    {
        // checks which skins we have
        string skin = anim_player.Skin;

        // checks if we are a ghost
        set_ghost(false);

        // we set the new skin
        anim_player.Skin = skin_name;
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
            if (skin_setting != null) { anim_player.Skin = skin_setting.ToString(); }
            else { anim_player.Skin = "perso"; }

            // on enleve l'Effect Ghost & Invisible
            RemoveEffect(Effect.Ghost);
            RemoveEffect(Effect.Invisible);
        }

        // sets the SettingsManager ghost setting
        if (ghost_setting == null) { return; }
        ghost_setting.value = (anim_player.Skin == "ghost") ? 1f : 0f;
    }
    private void set_ghost(bool activate=false) { set_ghost(activate ? 1f : 0f); }
    private void set_ghost(float value)
    {
        bool is_ghost = value >= 0.5f;  
        if (is_ghost && anim_player.Skin != "ghost")
        {
            ToggleGhost();
        }
        else if (!is_ghost && anim_player.Skin == "ghost")
        {
            ToggleGhost();
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

        // on désactive le Controller & PersoInputsController
        Controller.Instance.ResetCapableTarget(control_nothing: true);
        Controller.Instance.PIC.DisableInputs();


        // on switch au game_over panel
        UI_Manager.Instance.SwitchTo("game_over",force:true,override_transition:true);

        // on désactive plein de choses
        Destroy(GetComponent<SeeThroughHandler>());
        Destroy(transform.Find("body").GetComponent<ParticleSystemForceField>());

        remove_callbacks();

        deaths += 1; // on incrémente le nombre de morts du perso
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();

        // enleve les callbacks des settings
        if (skin_setting != null) { skin_setting.OnStringChanged -= SetSkin; }
        if (ghost_setting != null) { ghost_setting.OnValueChanged -= set_ghost; }
    }
}