using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEgg : Item, TurnableIntoSomething
{
    [SerializeField] private string entity_to_spawn;
    [SerializeField] private float xp_to_spawn = 0f;
    private static Dictionary<string, float> default_entities_spawn_chances = new Dictionary<string, float>()
    {
        { "rat", 0.5f },
        { "cat", 0.3f },
        { "zombo", 0.2f },
    };
    private static string RandomDefaultEntity
    {
        get
        {
            float total_chance = 0f;
            foreach (var kvp in default_entities_spawn_chances)
            {
                total_chance += kvp.Value;
            }

            float random_value = UnityEngine.Random.Range(0f, total_chance);
            float cumulative_chance = 0f;
            foreach (var kvp in default_entities_spawn_chances)
            {
                cumulative_chance += kvp.Value;
                if (random_value <= cumulative_chance)
                {
                    return kvp.Key;
                }
            }

            // Fallback (should not happen if chances are set correctly)
            return "rat";
        }
    }
    public DropParameters DropParameters => DropParameters.Default;


    // BEING DROPPED
    public override void BeDropped(Capable dropper)
    {
        _grabbed = false;
        idata.is_grabbed = false; // we set this to false before calling on_dropped so the events are triggered with the correct value
        
        if (!string.IsNullOrEmpty(entity_to_spawn))
        {
            if (log) { Debug.Log($"(SpawnEgg) --- EGG IS SPAWNING --- '{entity_to_spawn}' (from spawn egg '{ID}')"); }
            CapableEngine.Instance.TurnToSomething(this, entity_to_spawn);
        }
        else
        {
            int xp = Mathf.CeilToInt(xp_to_spawn);
            if (log) { Debug.Log($"(SpawnEgg) --- EGG IS GIVING --- '{xp}' XP (from spawn egg '{ID}')"); }
            XPProvider.Instance.EmitXP(xp, this.transform.position + Vector3.up * 0.5f);
            CapableEngine.Instance.DespawnCapable(data);
        }
    }


    // DATA MANAGEMENT
    public override void LoadData(CapableData data)
    {
        if (data is not SpawnEggData sedata)
        {
            if (log) { Debug.LogError($"(SpawnEgg - LoadData) The data provided is not of type SpawnEggData for item '{ID}'"); }
            return;
        }

        // if no current spawn egg data, and nothing to do, we add a random default entity or xp to spawn
        if (string.IsNullOrEmpty(sedata.entity_to_spawn) && sedata.xp_to_spawn == 0f)
        {
            if (UnityEngine.Random.value < 0.5f) // 50% to give xp, 50% to give an entity
            {
                entity_to_spawn = RandomDefaultEntity;
                xp_to_spawn = 0f;
            }
            else
            {
                entity_to_spawn = "";
                xp_to_spawn = UnityEngine.Random.Range(0f, 10f); // random xp between 0 and 10
            }
        }
        else
        {
            entity_to_spawn = sedata.entity_to_spawn;
            xp_to_spawn = sedata.xp_to_spawn;
        }

        base.LoadData(data);
    }
    public override ICapableData GetStaticData()
    {

        SpawnEggData static_data = new SpawnEggData((CapableData)base.GetStaticData())
        {
            xp_to_spawn = this.xp_to_spawn,
            entity_to_spawn = this.entity_to_spawn,
        };

        // returns current spawn egg data if it has a loaded one
        if (this.data != null && this.data is SpawnEggData sedata &&
         (!string.IsNullOrEmpty(sedata.entity_to_spawn) || sedata.xp_to_spawn > 0f))
        {
            static_data.xp_to_spawn = sedata.xp_to_spawn;
            static_data.entity_to_spawn = sedata.entity_to_spawn;
            return static_data;
        }

        return static_data;
    }

}

[Serializable] public class SpawnEggData : ItemData
{
    public string entity_to_spawn;
    public float xp_to_spawn = 0f;


    // CONSTRUCTOR
    public SpawnEggData() : base() { }
    public SpawnEggData(CapableData parent) : base(parent) { }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        return new SpawnEggData(base.Duplicate() as CapableData)
        {
            entity_to_spawn = this.entity_to_spawn,
            xp_to_spawn = this.xp_to_spawn,
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = base.GetDetails();
        details += $"  - Entity to spawn : {entity_to_spawn}\n";
        details += $"  - XP to spawn : {xp_to_spawn}\n";
        return details;
    }
}