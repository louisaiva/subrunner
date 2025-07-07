using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EatCapacity is a capacity that allows a being to eat
/// regen life
/// </summary>

public class EatCapacity : Capacity
{
    [Header("Eating parameters")]
    public Food food_target;

    // trigger the eating
    public override void Use(Capable capable)
    {
        // we check if we have a food target
        if (food_target == null)
        {
            if (debug) { Debug.LogWarning("(EatCapacity) " + capable.name + " has no food target"); }
            return;
        }

        // checks if the capable is a being
        if (capable is not Being)
        {
            if (debug) { Debug.LogWarning("(EatCapacity) " + capable.name + " is not a being"); }
            return;
        }
        Being being = capable as Being;

        // launch the animation
        Anim anim = being.anim_player.Play("eat");
        float anim_duration = cooldown;
        if (anim != null)
        {
            // we start the cooldown for the time of the animation
            anim_duration = anim.GetDuration();
            startCooldown(anim_duration);
        }
        else { startCooldown(); }

        // we launch the eating action for the food to take effect
        food_target.BeingBitten(being,anim_duration);
    }

    public void SetFoodTarget(Food food) { food_target = food; }
}