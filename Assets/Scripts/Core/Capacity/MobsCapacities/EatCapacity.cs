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
    // public bool logs_detection = false;
    // public bool log_actions = false;

    [Header("Eating parameters")]
    public float hunger = 0f; // Hunger level of the being, the less the better
    public float bite_duration = 1f; // duration of one bite
    public int bites_per_portion = 1; // number of bites per eating action


    [Header("Static data parameters")] // used for saving static data in the template, not used at runtime
    [SerializeField] private float range_food_detection = 15f;
    [SerializeField] private string food_rule = "food"; // rule to determine what food the being can eat


    [Header("Dynamic variables")] // not saved, only used at runtime
    public Food food_target; // the current food target of the being, can be null if no target

    // current coroutine
    private Coroutine current_coroutine = null;
    public bool IsEating => current_coroutine != null;

    // UPDATE
    protected void Update()
    {
        if (hunger > 500f) { return; }
        hunger += Time.deltaTime * 1f;
    }

    // USE
    public override void Use(Capable capable)
    {
        // verify that we are not already eating
        if (current_coroutine != null)
        {
            if (log) { Debug.LogWarning("(EatCapacity) " + capable.ID + " is already eating"); }
            return;
        }

        // we get the runtime Food
        if (food_target == null || !food_target.Loaded)
        {
            if (log) { Debug.LogWarning("(EatCapacity) " + capable.ID + " has no food target"); }
            return;
        }

        // check if the food target is still valid
        // todo check if the food still has portions, etc

        // we check if the capable has a health capacity
        if (!Capable.TryGetCapacity(out HealthCapacity health))
        {
            if (log) { Debug.LogWarning("(EatCapacity) " + capable.ID + " has no health capacity"); }
            return;
        }

        // we launch the eating action for the food to take effect
        current_coroutine = StartCoroutine(eat_coroutine(health, food_target));
    }
    private IEnumerator eat_coroutine(HealthCapacity health, Food food)
    {
        // launch the animation
        for (int i = 0; i < bites_per_portion; i++)
        {
            Anim anim = AnimPlayer.Play("eat", duration_override: bite_duration);
            if (anim == null)
            {
                current_coroutine = null;
                yield break;
            }

            if (log) { Debug.Log("(EatCapacity) " + health.ID + " is biting " + food.ID); }
            yield return new WaitForSeconds(bite_duration); // wait for the eating duration

            // we stop playing the anim
            AnimPlayer.StopPlaying("eat");

            // we check if the food target is still valid
            if (food == null || !food.Loaded)
            {
                if (log) { Debug.LogWarning("(EatCapacity) " + health.ID + " has no food target anymore"); }
                current_coroutine = null;
                food_target = null; // we reset the food target
                yield break;
            }
        }


        // we eat the food
        if (log) { Debug.Log("(EatCapacity) " + health.ID + " is eating " + food.ID); }
        health.AddLife(food.life_regen);
        this.hunger -= food.life_regen;
        food.BeEaten(health); // we remove one bite from the food target
        food_target = null; // we reset the food target

        // we reset the current coroutine
        current_coroutine = null;
    }
    public void Cancel()
    {
        // we cancel the eating action
        if (log) { Debug.Log("(EatCapacity) Canceling eating action on " + Capable.ID); }
        food_target = null; // we reset the food target
        StopAllCoroutines(); // stop all coroutines related to eating
        current_coroutine = null; // we reset the current coroutine

        // we stop the anim_player from playing
        if (AnimPlayer != null) { AnimPlayer.StopPlaying("eat"); }
    }

    // SET FOOD
    public void SetFoodTarget(Food food)
    {
        if (food == null || !food.Loaded)
        {
            if (log) { Debug.LogWarning("(EatCapacity) Trying to set a null or unloaded food target"); }
            return;
        }
        food_target = food;
    }




    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        if (data is not EatData edata) { return; }

        // load eating parameters
        this.bite_duration = edata.bite_duration;
        this.bites_per_portion = edata.bites_per_portion;

        // load entity data
        this.hunger = edata.hunger;

        // ? is there a reason we load the data at the end ????
        base.LoadData(data, capable_data);
    }

    // SAVE DYNAMIC DATA
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();

        if (this.data == null) { return; }
        if (this.data is not EatData edata) { return; }

        // save entity data
        edata.hunger = this.hunger;
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        EatData static_data = new EatData(base.GetStaticData())
        {
            food_rule = this.food_rule,
            range_food_detection = this.range_food_detection,
            bite_duration = this.bite_duration,
            bites_per_portion = this.bites_per_portion,
            hunger = this.hunger
        };

        return static_data;
    }
}

[Serializable] public class EatData : CapacityData
{
    // TYPE DATA (static at runtime, one per template)
    public string food_rule = "food";
    public float range_food_detection = 15f;
    public float bite_duration = 1f; // duration of one bite
    public int bites_per_portion = 1; // number of bites per eating action


    // ENTITY DATA (dynamic at runtime, one per entity)
    public float hunger = 0f; // Hunger level of the being, the less the better


    // CONSTRUCTOR
    public EatData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new EatData(base.Duplicate() as CapacityData)
        {
            food_rule = this.food_rule,
            range_food_detection = this.range_food_detection,
            bite_duration = this.bite_duration,
            bites_per_portion = this.bites_per_portion,
            hunger = this.hunger
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - food_rule : {food_rule}\n";
        details += $"  - range_food_detection : {range_food_detection}\n";
        details += $"  - bite_duration : {bite_duration}\n";
        details += $"  - bites_per_portion : {bites_per_portion}\n";
        details += $"  - hunger : {hunger}\n";
        return base.GetDetails() + details;
    }
}