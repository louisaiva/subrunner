using System;
using System.Collections.Generic;
using UnityEngine;

public class Perso : Movable, Hacker
{
    public static int Deaths = 0; // nombre de morts du perso
    public override ConnectCapacity Connector => Device?.Connector;

    [Header("PERSO")]
    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 100;

    // [Header("SKILLS")]
    // public SkillManager skillManager;
    

    [Header("METAMORPH")]
    [SerializeField] private List<string> metamorph_skins = new List<string>() { "bob", "cat", "zombo", "robot", "apple", "fridge", "small_laptop" };


    /* [Header("Items")]
    public ItemManager ItemManager
    {
        get
        {
            if (_itemManager == null)
            {
                _itemManager = GameObject.Find("/game/item_manager").GetComponent<ItemManager>();
            }
            return _itemManager;
        }
    }
    private ItemManager _itemManager; */


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
    /* protected override void Awake()
    {
        base.Awake();

        // set des logs
        OnDeviceGranted += (Device new_device) =>
        {
            /* if (debug) {  Debug.Log($"(Perso) new device set : {(new_device is Laptop laptop ? laptop.Reference : new_device.name)}"); /* } 
        };
        OnDeviceRemoved += (Device old_device) =>
        {
            /* if (debug) {  
            Debug.Log($"(Perso) Device removed: {(old_device is Laptop laptop ? laptop.Reference : old_device.name)}"); /* } 
        };
    } */



    ///
    //
    /// PERSO CALLBACKS
    //
    ///


    // CALLBACKS
    private void set_callbacks()
    {
        // met les callbacks de notif
        UI_Manager.Instance.GetPool("hud").GetComponent<UI_HUD>().Notifier.SetCallbacks(this);

        // on met le callback de pour afficher ui_hacking
        /* UI_Hacking hacking_pool = UI_Manager.Instance.GetPool("hacking").GetComponent<UI_Hacking>();
        OnDeviceGranted += hacking_pool.HandleDeviceGranted;
        OnDeviceRemoved += hacking_pool.HandleDeviceRemoved;

        // callbacks de ui_running hacks viewer
        UI_RunningHacksViewer running_hacks_viewer = hacking_pool.transform.GetComponentInChildren<UI_RunningHacksViewer>(includeInactive: true);
        OnDeviceGranted += running_hacks_viewer.HandleDeviceGranted;
        OnDeviceRemoved += running_hacks_viewer.HandleDeviceRemoved;

        // et du cores viewer
        UI_CoresViewer cores_viewer = hacking_pool.transform.GetComponentInChildren<UI_CoresViewer>(includeInactive: true);
        OnDeviceGranted += cores_viewer.HandleDeviceGranted;
        OnDeviceRemoved += cores_viewer.HandleDeviceRemoved; */

        // callbacks de health capacity
        HealthCapacity health_capacity = GetCapacity<HealthCapacity>();
        health_capacity.OnTakeDamage += OnDamageTaken;
        health_capacity.OnHeal += OnLifeAdded;
        health_capacity.OnDie += OnDie;
    }
    private void remove_callbacks()
    {
        // enleve les callbacks de notif
        UI_Manager.Instance?.GetPool<UI_HUD>()?.Notifier.RemoveCallbacks(this);

        // on enleve les callbacks de pour afficher ui_hacking
        /* UI_Hacking hacking_pool = UI_Manager.Instance.GetPool<UI_Hacking>();
        OnDeviceGranted -= hacking_pool.HandleDeviceGranted;
        OnDeviceRemoved -= hacking_pool.HandleDeviceRemoved;

        // callbacks de ui_running hacks viewer
        UI_RunningHacksViewer running_hacks_viewer = hacking_pool.transform.GetComponentInChildren<UI_RunningHacksViewer>(includeInactive: true);
        OnDeviceGranted -= running_hacks_viewer.HandleDeviceGranted;
        OnDeviceRemoved -= running_hacks_viewer.HandleDeviceRemoved;
        
        // et du cores viewer
        UI_CoresViewer cores_viewer = hacking_pool.transform.GetComponentInChildren<UI_CoresViewer>(includeInactive: true);
        OnDeviceGranted -= cores_viewer.HandleDeviceGranted;
        OnDeviceRemoved -= cores_viewer.HandleDeviceRemoved; */

        // callbacks de health capacity
        HealthCapacity health_capacity = GetCapacity<HealthCapacity>();
        health_capacity.OnTakeDamage -= OnDamageTaken;
        health_capacity.OnHeal -= OnLifeAdded;
        health_capacity.OnDie -= OnDie;
    }

    // METAMORPH & GHOST
    public void Metamorph()
    {
        // checks which skins we have
        string skin = AnimPlayer.Skin;

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
        AnimPlayer.Skin = metamorph_skins[index];
    }
    public void SetSkin(Setting skin_setting)
    {
        SetSkin(skin_setting.ToString());
    }
    public void SetSkin(string skin_name)
    {
        // checks which skins we have
        string skin = AnimPlayer.Skin;

        // checks if we are a ghost
        set_ghost(false);

        // we set the new skin
        AnimPlayer.Skin = skin_name;
    }
    public void ToggleGhost()
    {
        if (AnimPlayer.Skin != "ghost")
        {
            // on change le skin
            AnimPlayer.Skin = "ghost";

            // on applique l'Effect Ghost & Invisible
            AddEffect(Effect.Ghost, -888f);
            AddEffect(Effect.Invisible, -888f);
        }
        else
        {
            // on remet le skin de base
            if (SettingsManager.Instance.GetSetting("skin") is StringSetting skin_setting)
            {
                AnimPlayer.Skin = skin_setting.ToString();
            }
            else { AnimPlayer.Skin = "bob"; }

            // on enleve l'Effect Ghost & Invisible
            RemoveEffect(Effect.Ghost);
            RemoveEffect(Effect.Invisible);
        }

        // sets the SettingsManager ghost setting
        if (SettingsManager.Instance.GetSetting("ghost") != null)
        {
            SettingsManager.Instance.SetSettingWithoutNotifying("ghost", AnimPlayer.Skin == "ghost" ? 1f : 0f);
        }
    }
    private void set_ghost(Setting ghost_setting) { set_ghost(ghost_setting.Value >= 0.5f); }
    private void set_ghost(bool activate=false) { set_ghost(activate ? 1f : 0f); }
    private void set_ghost(float value)
    {
        bool is_ghost = value >= 0.5f;  
        if (is_ghost && AnimPlayer.Skin != "ghost")
        {
            ToggleGhost();
        }
        else if (!is_ghost && AnimPlayer.Skin == "ghost")
        {
            ToggleGhost();
        }
    }

    // XP
    /* public void addXP(int count)
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
        FloatingDmgProvider.Instance.TextManager.addFloatingText("LEVEL " + level.ToString(), transform.position + new Vector3(0, 0.5f, 0), "yellow");
    } */

    // HEAL & DAMAGE CALLBACKS
    public void OnLifeAdded(float life)
    {
        if (Controller.Perso == null || Controller.Perso != this) { return; } // if we are not the controlled perso, we do nothing

        // si on est sur le hud, on met à jour le chroma du PostProcessManager
        if (!UI_Manager.Instance.IsOnHUD()) { return; }
        PostProcessManager.Instance.UpdateChroma();
    }
    public void OnDamageTaken(float damage, Force knockback = null)
    {
        if (Controller.Perso == null || Controller.Perso != this) { return; } // if we are not the controlled perso, we do nothing

        // we make a little screenshake if perso
        float shake_magnitude = damage / GetCapacity<HealthCapacity>().Health;
        CameraShaker.Instance.Shake(shake_magnitude);

        // we shake the colors of the life bar
        if (!UI_Manager.Instance.IsOnHUD()) { return; }
        UI_Manager.Instance.GetPool<UI_HUD>().PersoTookDamage();
    }
    public void OnDie(CapableData capable_data)
    {
        if (Controller.Perso == null || Controller.Perso != this) { return; } // if we are not the controlled perso, we do nothing

        Deaths += 1; // on incrémente le nombre de morts du perso
        Debug.Log("YOU DIED");

        // on affiche un floating text
        FloatingDmgProvider.Instance.TextManager.addFloatingText("YOU DIED", transform.position + new Vector3(0, 0.5f, 0), "red");

        // on désactive le Controller & PersoInputsController
        // Controller.LazyInstance.Uncontrol(ID);
        // Controller.LazyInstance.PIC.DisableInputs();


        // on switch au game_over panel
        UI_Manager.Instance.SwitchTo("game_over",force:true,override_transition:true);

        // on désactive plein de choses
        // Destroy(GetComponent<SeeThroughHandler>());
        // Destroy(transform.Find("body").GetComponent<ParticleSystemForceField>());

        // remove_callbacks();

    }





    ///
    //
    /// DATA MANAGEMENT
    //
    ///

    public override void LoadData(CapableData data)
    {
        base.LoadData(data);

        // mets les callbacks
        set_callbacks();
        SettingsManager.Instance.RegisterCallback("skin", SetSkin);
        SettingsManager.Instance.RegisterCallback("ghost_mode", set_ghost);
    }
    public override void UnloadData()
    {
        // remove callbacks
        SettingsManager.Instance.UnregisterCallback("skin", SetSkin);
        SettingsManager.Instance.UnregisterCallback("ghost_mode", set_ghost);
        remove_callbacks();

        base.UnloadData();
    }

}

public class PersoData : CapableData
{
    // CONSTRUCTOR
    public PersoData(CapableData capable_data) : base(capable_data) { }

    // DUPLICATE
    public override ICapableData Duplicate() { return new PersoData(base.Duplicate() as CapableData);}

    // RUNTIME ONLY
    [RuntimeOnly, NonSerialized] private Perso loaded_assigned_perso;
    [RuntimeOnly] public Perso Perso { get { return loaded_assigned_perso; } }
    public override void OnLoaded(Capable capable)
    {
        base.OnLoaded(capable);
        if (capable is Perso perso) { loaded_assigned_perso = perso; }
    }
    public override void OnUnloaded(Capable capable)
    {
        base.OnUnloaded(capable);
        if (capable is Perso perso && loaded_assigned_perso == perso) { loaded_assigned_perso = null; }
    }
}
