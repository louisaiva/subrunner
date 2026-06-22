using System;
using System.Collections.Generic;
using UnityEngine;

public class ExpCapacity : Capacity
{
    /* // on s'enregistre en tant que trigger dans l'XPProvider particle system
    var trigger_particle_module = XPProvider.Instance.GetComponent<ParticleSystem>().trigger;
    trigger_particle_module.SetCollider(0, GetCapacity<HealthCapacity>().HealthCollider); */

    public ExpData edata { get { return (ExpData)data;} }
    public float XPPourcent { get { return (float)edata.xp / edata.xp_to_next_level; } }
    [SerializeField] private ExpData data_to_save;

    // [Header("Logs")]
    // [SerializeField] private Loggable<ExpCapacity> log_exp = new Loggable<ExpCapacity>();

    // MAIN ENTRY POINTS
    public void AddXP(int count)
    {
        edata.xp += count;
        edata.total_xp += count;
        // log_exp.LogSpecific("Added " + count + " XP !!!!!! nex level in   " + edata.xp + " / " + edata.xp_to_next_level);
        if (Controller.Capable != null && Controller.Capable == Capable && UI_Manager.Instance.IsOnHUD()) { UI_Manager.Instance.GetPool<UI_HUD>().PersoGrabbedXP(); }
        if (edata.xp >= edata.xp_to_next_level)
        {
            LevelUP();
        }
    }
    private void LevelUP()
    {
        edata.level += 1;
        edata.xp = 0;
        edata.xp_to_next_level = (int)(edata.xp_to_next_level * 1.5f);
        edata.upgrade_points += 1;


        // we check the type of the entity that leveled up
        if (Controller.Capable != null && Controller.Capable == Capable)
        {
            Debug.Log("LEVEL UP ! level " + edata.level);

            // on affiche un texte de level up
            FloatingDmgProvider.Instance.TextManager.addFloatingText("LEVEL " + edata.level.ToString(), transform.position + new Vector3(0, 0.5f, 0), "yellow");

            // we run the hud "xp" continuous color sweep
            UI_Manager.Instance.GetPool<UI_HUD>().PersoLeveledUP(edata.level, edata.upgrade_points);

            return;
        }

        // else it is a mob,
        // we update a random capacity stat
        // todo : do a better upgrade system than that, for now we either upgrade the attack or the health or speed by 1.25
        int random_capacity = UnityEngine.Random.Range(0, 3);
        if (random_capacity == 0 && TryGetSiblingCapacity(out AttackCapacity attack_capacity))
        {
            attack_capacity.damage = (int)(attack_capacity.damage * 1.25f);
        }
        else if (random_capacity == 1 && TryGetSiblingCapacity(out HealthCapacity health_capacity))
        {
            health_capacity.MaxHealth = (int)(health_capacity.MaxHealth * 1.25f);
            health_capacity.HealMax();
        }
        else if (random_capacity == 2 && TryGetSiblingCapacity(out WalkCapacity walk_capacity))
        {
            walk_capacity.max_walk_speed *= 1.25f;
            walk_capacity.max_run_speed *= 1.25f;
        }
    }

    public void ReleaseXP()
    {
        // release all xp lol
        if (edata.xp <= 0) { edata.xp = UnityEngine.Random.Range(1, 15); }
        XPProvider.Instance.EmitXP(edata.xp, transform.position);
        // todo also release the exp upgrade points, but we need another particle system for that, with bigger particles !!
        edata.total_xp -= edata.xp;
        edata.xp = 0;
        edata.xp_to_next_level = 100;
        edata.level = 0;
        edata.upgrade_points = 0;

        // we run the hud "xp" continuous color sweep
        if (Controller.Capable != null && Controller.Capable == Capable) { UI_Manager.Instance.GetPool<UI_HUD>().PersoReleasedUP(up_waiting : 0); }
    }


    // DATA MANAGEMENT
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        base.LoadData(data, capable_data);
        if (data is not ExpData expd) { return; }

        // we load the collider data if we have it
        CircleCollider2D collider = GetComponentInChildren<CircleCollider2D>(includeInactive: true);
        if (collider != null)
        {
            // load the collider data
            ColliderBank.LoadColliderData(collider, expd.circle_data);
            XPProvider.Instance.RegisterTrigger(collider);
        }

        // we load the particle system data if we have it
        ParticleSystemForceField ps = GetComponentInChildren<ParticleSystemForceField>(includeInactive: true);
        if (ps != null)
        {
            ps.endRange = expd.end_range;
            var gravity = ps.gravity;
            gravity.constant = expd.gravity_strengh;
            ps.gravity = gravity;
        }
    }
    public override void UnloadData()
    {
        base.UnloadData();

        // we unregister our collider as trigger for the XP provider particle system
        XPProvider.Instance.UnregisterTrigger(GetComponent<Collider2D>());
    }


    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        ExpData static_data = new ExpData(base.GetStaticData())
        {
            xp = data_to_save.xp,
            total_xp = data_to_save.total_xp,
            xp_to_next_level = data_to_save.xp_to_next_level,
            level = data_to_save.level,
            upgrade_points = data_to_save.upgrade_points
        };

        // check if we have a collider and save its data
        Collider2D collider = GetComponentInChildren<Collider2D>(includeInactive: true);
        if (collider != null) { static_data.circle_data = (CircleData)ColliderBank.GetColliderData(collider); }

        // we save the particle system data
        ParticleSystemForceField ps = GetComponentInChildren<ParticleSystemForceField>(includeInactive: true);
        if (ps != null)
        {
            static_data.end_range = ps.endRange;
            static_data.gravity_strengh = ps.gravity.constant;
        }

        return static_data;
    }
}

[Serializable] public class ExpData : CapacityData
{
    public int xp;
    public int total_xp;
    public int xp_to_next_level;
    public int level;
    public int upgrade_points;

    // particle system field data
    public float end_range;
    public float gravity_strengh;

    // collider data
    public CircleData circle_data;

    // CONSTRUCTOR
    public ExpData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        ExpData new_data = new ExpData(base.Duplicate() as CapacityData)
        {
            xp = this.xp,
            total_xp = this.total_xp,
            level = this.level,
            xp_to_next_level = this.xp_to_next_level,
            upgrade_points = this.upgrade_points,
            end_range = this.end_range,
            gravity_strengh = this.gravity_strengh,
            circle_data = (CircleData)this.circle_data.Duplicate()
        };
        return new_data;
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - Level: {level}\n";
        details += $"  - XP / XP to Next Level: {xp} / {xp_to_next_level}\n";
        details += $"  - Total XP: {total_xp}\n";
        details += $"  - Availables Upgrades: {upgrade_points}\n";
        details += $"  - Particle System:\n";
        details += $"    - End Range: {end_range}\n";
        details += $"    - Gravity Strengh: {gravity_strengh}\n";
        details += $"  - Collider:\n" + circle_data.GetDetails();
        return base.GetDetails() + details;
    }
}

[Serializable] public class CapacityUpgrade<T, U> where T : Capacity where U : struct
{
    public T capacity_type; // todo : très peu serializable ça mdr
    public string variable_name;
    public List<U> levels; // todo ça aussi
    public int current_level = 0; 
}