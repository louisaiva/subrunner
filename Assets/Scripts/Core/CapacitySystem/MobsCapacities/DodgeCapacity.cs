using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// DodgeCapacity is a capacity that allows a being to dodge
/// gives Immobile & Invincible for a short time
/// </summary>

public class DodgeCapacity : CooldownCapacity
{
    [Header("Dodge parameters")]
    [SerializeField] private float dodge_magnitude = 25f;
    [SerializeField] private Force dodge_force; // RTO
    [SerializeField] private float dodge_duration = default;

    // USE
    public override void Use(Capable capable)
    {
        // we check if we are already dodging
        if (capable.AnimPlayer.current_capacity == "dodge") { return; }

        // we play the animation
        Anim anim = capable.AnimPlayer.Play("dodge", duration_override: dodge_duration);
        if (log) { Debug.Log("(DodgeCapacity) dodge launched for " + capable.name + ", anim found is " + (anim != null ? anim.name : "null")); }
        if (anim == null) { return; }

        // we play the sound
        AudioEngine.Instance.Play("dodge", capable);

        // we start the cooldown for the time of the animation
        float duration = anim.GetDuration();
        startCooldown(duration);

        // we gives the invincible & immobile effects if Being
        if (capable.HasCapacity<HealthCapacity>())
        {
            // we can't take damage for the animation duration
            capable.AddEffect(Effect.Invincible, duration);
            // we can't move for a short time
            capable.AddEffect(Effect.Immobile, duration / 2f);
            // we can't move for a short time
            capable.AddEffect(Effect.SemiGhost, duration);

            if (log) { Debug.Log("(DodgeCapacity) dodge added invincible & immobile effects to " + capable.name); }
        }

        // check if the capable is a Movable_ to add them a force
        if (capable is Movable)
        {
            // add a dodge Force to the movable
            Movable movable = (Movable)capable;
            dodge_force.direction = movable.Orientation;
            dodge_force.magnitude = dodge_magnitude;
            // dodge_force.CalculateMagnitudeMax(dodge_distance, dodge_duration);
            movable.AddForce(dodge_force);

            if (log) { Debug.Log("(DodgeCapacity) dodge added dodge force to " + capable.name); }
        }

        // log
        // Debug.Log(transform.parent.name + " just dodged");
        if (log) { Debug.Log($"(DodgeCapacity) {capable.name} used dodge for {duration} seconds"); }
    }


    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        if (data is not DodgeData ddata) { return; }

        // we load the static data
        dodge_magnitude = ddata.dodge_magnitude;
        dodge_duration = ddata.dodge_duration;

        base.LoadData(data);
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        DodgeData static_data = new DodgeData(base.GetStaticData())
        {
            dodge_magnitude = this.dodge_magnitude,
            dodge_duration = this.dodge_duration
        };

        return static_data;
    }
}

public class DodgeData : CapacityData
{

    // instance parameters
    public float dodge_magnitude;
    public float dodge_duration;


    // CONSTRUCTOR
    public DodgeData(CapacityData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new DodgeData(base.Duplicate() as CapacityData)
        {
            dodge_magnitude = this.dodge_magnitude,
            dodge_duration = this.dodge_duration
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - dodge magnitude : {dodge_magnitude}\n";
        details += $"  - dodge duration : {dodge_duration}\n";
        return base.GetDetails() + details;
    }

}