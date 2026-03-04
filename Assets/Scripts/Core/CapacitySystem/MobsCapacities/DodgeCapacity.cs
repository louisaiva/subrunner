using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// DodgeCapacity is a capacity that allows a being to dodge
/// gives Immobile & Invincible for a short time
/// </summary>

public class DodgeCapacity : Capacity
{
    [Header("Dodge parameters")]
    [SerializeField] private float dodge_magnitude = 25f;
    [SerializeField] private Force dodge_force;
    [SerializeField] private float dodge_duration = default;

    // USE
    public override void Use(Capable capable)
    {
        // we check if we are already dodging
        if (capable.anim_player.current_capacity == "dodge") { return; }

        // we play the animation
        Anim anim = capable.anim_player.Play("dodge", duration_override: dodge_duration);
        if (debug) { Debug.Log("(DodgeCapacity) dodge launched for " + capable.name + ", anim found is " + (anim != null ? anim.name : "null")); }
        if (anim == null) { return; }

        // we play the sound
        AudioEngine.Instance.Play("dodge", capable);

        // we start the cooldown for the time of the animation
        float duration = anim.GetDuration();
        startCooldown(duration);

        // we gives the invincible & immobile effects if Being
        if (capable is Being)
        {
            // we can't take damage for the animation duration
            capable.AddEffect(Effect.Invincible, duration);
            // we can't move for a short time
            capable.AddEffect(Effect.Immobile, duration / 2f);
            // we can't move for a short time
            capable.AddEffect(Effect.SemiGhost, duration);

            if (debug) { Debug.Log("(DodgeCapacity) dodge added invincible & immobile effects to " + capable.name); }
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

            if (debug) { Debug.Log("(DodgeCapacity) dodge added dodge force to " + capable.name); }
        }

        // log
        // Debug.Log(transform.parent.name + " just dodged");
        if (debug) { Debug.Log($"(DodgeCapacity) {capable.name} used dodge for {duration} seconds"); }
    }
}