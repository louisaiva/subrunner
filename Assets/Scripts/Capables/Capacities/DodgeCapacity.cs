using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// DodgeCapacity is a capacity that allows a being to dodge
/// gives Immobile & Invincible for a short time
/// todo : can we always dodge ? or only when we are being attacked ?
/// todo : to test
/// </summary>

public class DodgeCapacity : Capacity
{
    [Header("Dodge parameters")]
    // [SerializeField] private float dodge_distance = 2f;
    // [SerializeField] private float dodge_duration = 0.5f;
    [SerializeField] private float dodge_magnitude = 25f;
    [SerializeField] private Force dodge_force;

    // trigger the dodge
    public override void Use(Capable capable)
    {
        // we play the animation
        Anim anim = play_anim(capable.anim_player, name);
        if (anim == null) { return; }
        
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
            capable.AddEffect(Effect.Ghost, duration);
        }

        // check if the capable is a Movable_ to add them a force
        if (capable is Movable) 
        {
            // add a dodge Force to the movable
            Movable movable = (Movable) capable;
            dodge_force.direction = movable.Orientation;
            dodge_force.magnitude = dodge_magnitude;
            // dodge_force.CalculateMagnitudeMax(dodge_distance, dodge_duration);
            movable.AddForce(dodge_force);
        }

        // log
        // Debug.Log(transform.parent.name + " just dodged");
    }
}