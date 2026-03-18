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
    public bool log_actions = false;

    [Header("Eating parameters")]
    public Food food_target;
    public float hunger = 0f; // Hunger level of the being, the less the better
    public float range_food_detection = 15f;
    public float bite_duration = 1f; // duration of one bite
    public int bites_per_eating = 1; // number of bites per eating action
    [SerializeField] private string food_rule = "food"; // rule to determine what food the being can eat
    public string FoodRule => food_rule;


    // 1 - FOOD DETECTION // todo change this to work with colliders
    public List<Capable> DetectPotentialFoods(IA ia)
    {
        // we do an overlap to detect foods
        Collider2D[] results = Physics2D.OverlapCircleAll(ia.transform.position,
            range_food_detection,
            LayerMask.GetMask("Interactives"));
        if (results.Length == 0) { return new List<Capable>(); }

        if (logs_detection)
        {
            Debug.Log("(EatCapacity) " + ia.name + " detected " + results.Length + " potential foods in range of " + range_food_detection);
        }

        // we convert those into foods & check few things
        List<Capable> potential_foods = new List<Capable>();
        foreach (Collider2D collider in results)
        {
            // we check if the parent capable has a Capable component
            Capable capable = collider.transform.parent.GetComponent<Capable>();
            if (capable == null) { continue; }
            if (capable is not Food && capable is not Corpse) { continue; }

            if (capable is Food food && food.ValidateRule(FoodRule))
            {
                // we add the food to the list of potential foods
                potential_foods.Add(food);
            }
            else if (capable is Corpse corpse && corpse.EatableBy(FoodRule))
            {
                // we add the corpse to the list of potential foods
                potential_foods.Add(corpse);
            }
        }
        return potential_foods;
    }
    public Capable GetClosestFoodTarget(IA ia)
    {
        // get the potential foods
        List<Capable> potential_foods = DetectPotentialFoods(ia);
        if (potential_foods.Count == 0) { return null; }

        // we find the closest food
        Capable closest_food = null;
        float closest_distance = float.MaxValue;
        foreach (Capable food in potential_foods)
        {
            float distance = Vector3.Distance(food.gameObject.transform.position, ia.transform.position);

            if (!(distance < closest_distance))
                continue;

            closest_food = food;
            closest_distance = distance;
        }
        return closest_food;
    }

    // UPDATE
    protected void Update()
    {
        if (hunger > 500f) { return; }
        hunger += Time.deltaTime * 1f;
    }

    // USE
    public override void Use(Capable capable)
    {
        // we check if we can Use()
        if (food_target == null)
        {
            if (log) { Debug.LogWarning("(EatCapacity) " + capable.name + " has no food target"); }
            return;
        }
        if (capable is not Being being)
        {
            if (log) { Debug.LogWarning("(EatCapacity) " + capable.name + " is not a being"); }
            return;
        }

        // we launch the eating action for the food to take effect
        StartCoroutine(eat_coroutine(being));
    }
    private IEnumerator eat_coroutine(Being being)
    {
        // launch the animation
        Anim anim = being.AnimPlayer.Play("eat", duration_override: bite_duration);
        if (anim == null) { yield break; }

        if (log) { Debug.Log("(EatCapacity) " + being.name + " is trying to eat " + food_target.name); }
        yield return new WaitForSeconds(bite_duration * bites_per_eating); // wait for the eating duration

        // we stop playing the anim
        being.AnimPlayer.StopPlaying("eat");

        // we check if the food target is still valid
        if (food_target == null)
        {
            if (log) { Debug.LogWarning("(EatCapacity) " + being.name + " has no food target anymore"); }
            yield break;
        }

        // we eat the food
        if (log) { Debug.Log("(EatCapacity) " + being.name + " is eating " + food_target.name); }
        being.AddLife(food_target.life_regen);
        this.hunger -= food_target.life_regen;
        food_target.BeEaten(being); // we remove one bite from the food target
        food_target = null;
    }
    public void Cancel(IA ia)
    {
        // we cancel the eating action
        if (log) { Debug.Log("(EatCapacity) Canceling eating action on " + ia.name); }
        food_target = null; // we reset the food target
        StopAllCoroutines(); // stop all coroutines related to eating

        // we stop the anim_player from playing
        ia.AnimPlayer.StopPlaying("eat");
    }

    // SET FOOD
    public void SetFoodTarget(Food food) { food_target = food; }
}