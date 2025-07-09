using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EatCapacity is a capacity that allows a being to eat
/// regen life
/// </summary>

public class EatCapacity : Capacity
{
    public bool logs_detection = false;

    [Header("Eating parameters")]
    public Food food_target;
    public float hunger = 0f; // Hunger level of the being, the less the better
    public float range_food_detection = 15f;
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


    // 1 - FOOD DETECTION // todo change this to work with colliders
    public List<Food> DetectPotentialFoods(IA ia)
    {
        // we do an overlap to detect foods
        Collider2D[] results = Physics2D.OverlapCircleAll(ia.transform.position,
            range_food_detection,
            LayerMask.GetMask("Interactives"));
        if (results.Length == 0) { return new List<Food>(); }

        if (logs_detection)
        {
            Debug.Log("(EatCapacity) " + ia.name + " detected " + results.Length + " potential foods in range of " + range_food_detection);
        }

        // we convert those into foods & check few things
        List<Food> potential_foods = new List<Food>();
        foreach (Collider2D collider in results)
        {
            // we check if the parent capable has a Food component
            Food food = collider.transform.parent.GetComponent<Food>();
            if (food == null) { continue; }

            // checks if they are on the ground & Eatable
            if (food.Grabbed || !food.Eatable) { continue; }

            // checks if they pass the rule
            if (!food.ValidateRule(FoodRule)) { continue; }

            // we add the food to the list of potential foods
            potential_foods.Add(food);
        }
        return potential_foods;
    }
    public Food GetClosestFood(IA ia)
    {
        // get the potential foods
        List<Food> potential_foods = DetectPotentialFoods(ia);
        if (potential_foods.Count == 0) { return null; }

        // we find the closest food
        Food closest_food = null;
        float closest_distance = float.MaxValue;
        foreach (Food food in potential_foods)
        {
            float distance = Vector3.Distance(food.gameObject.transform.position, ia.transform.position);

            if (!(distance < closest_distance))
                continue;

            closest_food = food;
            closest_distance = distance;
        }
        return closest_food;
    }


    // 2 - USING THE CAPACITY

    // UPDATE
    protected override void Update()
    {
        base.Update();
        if (hunger > 500f) { return; }
        hunger += Time.deltaTime * 1f;
    }

    // USE
    public override void Use(Capable capable)
    {
        // we check if we can Use()
        if (food_target == null)
        {
            if (debug) { Debug.LogWarning("(EatCapacity) " + capable.name + " has no food target"); }
            return;
        }
        if (capable is not Being being)
        {
            if (debug) { Debug.LogWarning("(EatCapacity) " + capable.name + " is not a being"); }
            return;
        }
        
        // launch the animation
        Anim anim = being.anim_player.Play("eat");
        if (anim != null)
        {
            // we start the cooldown for the time of the animation
            startCooldown(anim.GetDuration());
        }
        else { startCooldown(); }

        // we launch the eating action for the food to take effect
        StartCoroutine(Bite(being));
    }
    private IEnumerator Bite(Being being)
    {
        if (debug) { Debug.Log("(EatCapacity) " + being.name + " is trying to eat " + food_target.name); }
        while (being.anim_player.current_capacity == "eat") { yield return null; } // wait for the animation to finish

        // we check if the food target is still valid
        if (food_target == null || !food_target.Eatable)
        {
            if (debug) { Debug.LogWarning("(EatCapacity) " + being.name + " has no food target or the food is not eatable anymore"); }
            yield break;
        }

        // we eat one bite
        if (debug) { Debug.Log("(EatCapacity) " + being.name + " is eating one bite of " + food_target.name); }
        being.AddLife(food_target.life_regen_per_bite);
        this.hunger -= food_target.life_regen_per_bite;
        food_target.RemoveOneBite(); // we remove one bite from the food target
    }


    // SET FOOD
    public void SetFoodTarget(Food food) { food_target = food; }
}