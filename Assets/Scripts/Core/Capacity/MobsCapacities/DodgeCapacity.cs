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
    public override void Use(Capable user)
    {
        // we check if we are already dodging
        if (user.AnimPlayer.IsShowing("dodge")) { return; }

        // we play the animation
        Anim anim = user.AnimPlayer.Play("dodge", duration_override: dodge_duration);
        if (log) { Debug.Log("(DodgeCapacity) dodge launched for " + user.ID + ", anim found is " + (anim != null ? anim.name : "null")); }
        if (anim == null) { return; }

        // we play the sound
        AudioEngine.Instance.Play("dodge", user);

        // we start the cooldown for the time of the animation
        float duration = anim.GetDuration();
        startCooldown(duration);

        // we gives the invincible & immobile effects if Being
        if (user.HasCapacity<HealthCapacity>())
        {
            // we can't take damage for the animation duration
            user.AddEffect(Effect.Invincible, duration);
            // we can't move for a short time
            user.AddEffect(Effect.Immobile, duration / 2f);
            // we can't move for a short time
            user.AddEffect(Effect.SemiGhost, duration);

            if (log) { Debug.Log("(DodgeCapacity) dodge added invincible & immobile effects to " + user.ID); }
        }

        // check if the capable is a Movable_ to add them a force
        if (user is Movable movable)
        {
            // add a dodge Force to the movable
            dodge_force.direction = movable.Orientation;
            dodge_force.magnitude = dodge_magnitude;
            // dodge_force.CalculateMagnitudeMax(dodge_distance, dodge_duration);
            movable.AddForce(dodge_force);

            if (log) { Debug.Log("(DodgeCapacity) dodge added dodge force to " + user.ID); }
        }

        // log
        // Debug.Log(transform.parent.name + " just dodged");
        if (log) { Debug.Log($"(DodgeCapacity) {user.ID} used dodge for {duration} seconds"); }
    }


    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        if (data is not DodgeData ddata) { return; }

        // we load the static data
        dodge_magnitude = ddata.dodge_magnitude;
        dodge_duration = ddata.dodge_duration;

        // ? is there a reason we load the data at the end ????
        base.LoadData(data, capable_data);
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
    public DodgeData(CapacityData parent) : base(parent) { }

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