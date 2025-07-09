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
    public float hunger = 0f; // Hunger level of the being, the less the better
    [SerializeField] private string[] items_eatable = new string[] { "apple" }; // List of items that the IA can eat
    public string FoodRule
    {
        get
        {
            // return the total item_rule to see if the ia can eat a precise item
            if (items_eatable.Length == 0) { return "food"; }
            string rule = "";
            for (int i = 0; i < items_eatable.Length; i++)
            {
                rule += "food:" + items_eatable[i] + ",";
            }
            rule = rule.TrimEnd(','); // we remove the last comma
            return rule;
        }
    }


    // UPDATE
    protected override void Update()
    {
        base.Update();

        hunger += Time.deltaTime * 1f;
    }
    
    // USE
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
        food_target.BeingBitten(being, anim_duration);
    }

    public void SetFoodTarget(Food food) { food_target = food; }
}