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
    public CapableTarget food_target; // the current food target of the being, can be null if no target
    // public Food found_food;
    public float hunger = 0f; // Hunger level of the being, the less the better
    public float range_food_detection = 15f;
    public float bite_duration = 1f; // duration of one bite
    public int bites_per_eating = 1; // number of bites per eating action
    [SerializeField] private string food_rule = "food"; // rule to determine what food the being can eat
    public string FoodRule => food_rule;

    // current coroutine
    private Coroutine current_coroutine = null;

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
            if (log) { Debug.LogWarning("(EatCapacity) " + capable.name + " is already eating"); }
            return;
        }

        // we get the runtime Food
        if (food_target == null)
        {
            if (log) { Debug.LogWarning("(EatCapacity) " + capable.name + " has no food target"); }
            return;
        }
        Food food = food_target.LoadedTarget as Food;
        if (food == null)
        {
            food_target = null; // we reset the food target if it's not valid anymore
            if (log) { Debug.LogWarning("(EatCapacity) " + capable.name + " food target is not a loaded Food"); }
            return;
        }

        // check if the food target is still valid
        // todo check if the food still has portions, etc

        // we check if the capable has a health capacity
        if (!Capable.TryGetCapacity(out HealthCapacity health))
        {
            if (log) { Debug.LogWarning("(EatCapacity) " + capable.name + " has no health capacity"); }
            return;
        }

        // we launch the eating action for the food to take effect
        current_coroutine = StartCoroutine(eat_coroutine(health, food));
    }
    private IEnumerator eat_coroutine(HealthCapacity health, Food food)
    {
        // launch the animation
        Anim anim = health.Capable.AnimPlayer.Play("eat", duration_override: bite_duration);
        if (anim == null) { current_coroutine = null; yield break; }

        if (log) { Debug.Log("(EatCapacity) " + health.name + " is trying to eat " + food.name); }
        yield return new WaitForSeconds(bite_duration * bites_per_eating); // wait for the eating duration

        // we stop playing the anim
        health.Capable.AnimPlayer.StopPlaying("eat");

        // we check if the food target is still valid
        if (food == null || !food.Loaded)
        {
            if (log) { Debug.LogWarning("(EatCapacity) " + health.name + " has no food target anymore"); }
            current_coroutine = null;
            yield break;
        }

        // we eat the food
        if (log) { Debug.Log("(EatCapacity) " + health.name + " is eating " + food.name); }
        health.AddLife(food.life_regen);
        this.hunger -= food.life_regen;
        food.BeEaten(health); // we remove one bite from the food target

        // we reset the current coroutine
        current_coroutine = null;
    }
    public void Cancel(IA ia)
    {
        // we cancel the eating action
        if (log) { Debug.Log("(EatCapacity) Canceling eating action on " + ia.name); }
        food_target = null; // we reset the food target
        StopAllCoroutines(); // stop all coroutines related to eating
        current_coroutine = null; // we reset the current coroutine

        // we stop the anim_player from playing
        ia.AnimPlayer.StopPlaying("eat");
    }

    // SET FOOD
    public void SetFoodTarget(Food food) { food_target = new CapableTarget(food); }




    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        if (data is not EatData edata) { return; }

        // load eating parameters
        this.food_rule = edata.food_rule;
        this.range_food_detection = edata.range_food_detection;
        this.bite_duration = edata.bite_duration;
        this.bites_per_eating = edata.bites_per_eating;

        // load entity data
        this.food_target = edata.food_target;
        this.hunger = edata.hunger;

        base.LoadData(data);
    }

    // SAVE DYNAMIC DATA
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();

        if (this.data == null) { return; }
        if (this.data is not EatData edata) { return; }

        // save entity data
        edata.food_target = this.food_target;
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
            bites_per_eating = this.bites_per_eating,
            hunger = this.hunger,
            food_target = null // no food target when no game loaded
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
    public int bites_per_eating = 1; // number of bites per eating action


    // ENTITY DATA (dynamic at runtime, one per entity)
    public CapableTarget food_target;
    public float hunger = 0f; // Hunger level of the being, the less the better


    // CONSTRUCTOR
    public EatData(CapacityData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new EatData(base.Duplicate() as CapacityData)
        {
            food_rule = this.food_rule,
            range_food_detection = this.range_food_detection,
            bite_duration = this.bite_duration,
            bites_per_eating = this.bites_per_eating,
            food_target = this.food_target,
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
        details += $"  - bites_per_eating : {bites_per_eating}\n";
        if (food_target != null) { details += $"  - food_target : {food_target.capable_id}\n"; }
        else { details += $"  - food_target : null\n"; }
        details += $"  - hunger : {hunger}\n";
        return base.GetDetails() + details;
    }
}