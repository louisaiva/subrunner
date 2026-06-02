using System;
using System.Collections.Generic;
using UnityEngine;

public class Perso : Movable, Hacker
{
    public static int Deaths = 0; // nombre de morts du perso
    public override ConnectCapacity Connector => Device?.Connector;

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


    // CONTROL
    private bool callbacks_set = false;
    public override void OnControlled()
    {
        // met les callbacks de notif
        UI_Manager.Instance.GetPool("hud").GetComponent<UI_HUD>().Notifier.SetCallbacks(this);

        // mets les callbacks de settings
        SettingsManager.Instance.RegisterCallback("skin", set_skin);
        SettingsManager.Instance.RegisterCallback("ghost", set_ghost);

        // callbacks de health capacity
        HealthCapacity health_capacity = GetCapacity<HealthCapacity>();
        health_capacity.OnTakeDamage += OnDamageTaken;
        health_capacity.OnHeal += OnLifeAdded;
        health_capacity.OnDie += OnDie;

        callbacks_set = true;
    }
    public override void OnUncontrolled()
    {
        // enleve les callbacks de notif
        UI_Manager.Instance?.GetPool<UI_HUD>()?.Notifier.RemoveCallbacks(this);

        // remove callbacks
        SettingsManager.Instance.UnregisterCallback("skin", set_skin);
        SettingsManager.Instance.UnregisterCallback("ghost", set_ghost);

        // callbacks de health capacity
        HealthCapacity health_capacity = GetCapacity<HealthCapacity>();
        health_capacity.OnTakeDamage -= OnDamageTaken;
        health_capacity.OnHeal -= OnLifeAdded;
        health_capacity.OnDie -= OnDie;

        callbacks_set = false;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (callbacks_set) { OnUncontrolled(); }
    }

    ///
    //
    /// PERSO CALLBACKS
    //
    ///

    private AnimPlayer AnimPlayer => Visual as AnimPlayer;

    // METAMORPH & GHOST
    private void set_skin(Setting skin_setting)
    {
        set_skin(skin_setting.ToString());
    }
    private void set_skin(string skin_name)
    {
        disable_ghost();


        // we set the new skin
        AnimPlayer.Skin = skin_name;
    }
    private void set_ghost(Setting ghost_setting)
    {
        bool is_ghost = ghost_setting.Value >= 0.5f;  
        if (is_ghost && AnimPlayer.Skin != "ghost") { enable_ghost(); }
        else if (!is_ghost && AnimPlayer.Skin == "ghost") { disable_ghost(); }
    }
    public void enable_ghost()
    {
        // on change le skin
        AnimPlayer.Skin = "ghost";

        // on applique l'Effect Ghost & Invisible
        AddEffect(Effect.Ghost, -888f);
        AddEffect(Effect.Invisible, -888f);

        SettingsManager.Instance.SetSettingWithoutNotifying("ghost", 1f);
    }
    public void disable_ghost()
    {
        // on change le skin
        if (SettingsManager.Instance.GetSetting("skin") is StringSetting skin_setting)
        {
            AnimPlayer.Skin = skin_setting.ToString();
        }
        else { AnimPlayer.Skin = "bob"; }

        // on enleve l'Effect Ghost & Invisible
        RemoveEffect(Effect.Ghost);
        RemoveEffect(Effect.Invisible);

        SettingsManager.Instance.SetSettingWithoutNotifying("ghost", 0f);
    }

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

        // on switch au game_over panel
        UI_Manager.Instance.SwitchTo("game_over",force:true,override_transition:true);
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
