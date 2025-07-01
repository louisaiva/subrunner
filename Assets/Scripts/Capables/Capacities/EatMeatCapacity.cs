using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EatMeatCapacity : Capacity
{

    [Header("Eat Meat Capacity")]
    public Meat meat_target; // the meat to eat
    [SerializeField] private float hp_per_meat = 1f; // amount of hp gained per meat eaten

    public override void Use(Capable capable)
    {
        // checks if we have a meat_target
        if (!meat_target)
        {
            if (debug) { Debug.LogWarning("(EatMeatCapacity) No meat target set for " + capable.name); }
            return;
        }

        // we play the animation
        capable.anim_player.Play("eat");

        StopAllCoroutines();
        StartCoroutine(wait_for_eating());
    }

    private IEnumerator wait_for_eating()
    {
        
        while (capable.anim_player.current_capacity == "eat")
        {
            // we wait for the animation to finish
            yield return null;
        }

        // we absorb the meat
        absorb_meat();
    }

    private void absorb_meat()
    {
        Being being = capable as Being;
        if (being == null) { return; }

        // we check if the being has a meat target
        if (meat_target == null) { return; }

        // we try to eat one meat
        if (!meat_target.BeingEaten(being)) { return; }

        // we gain 1 hp
        being.AddLife(hp_per_meat);
    }
}