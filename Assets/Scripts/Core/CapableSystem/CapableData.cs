using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable] public class CapableData
{
    public string id;
    public string kind; // used to determine which kind of capable it is. i.e. chest, IA, spawner or else
    public Vector2 position;
    public Vector2 orientation;
    // public Vector2 inputs; ????

    // ANIM PLAYER
    // todo maybe we can have a AnimPlayerData class ?
    // - skin
    // - layers
    // - List<AnimCapacityPriority>
    public string skin;

    // INVENTORY
    public List<string> inventory;

    // CAPACITIES
    public List<string> capacities_ids;

    // EFFECTS
    public List<Effect> effects;
    public List<float> effects_ttl; // time to live for each effect, in seconds


    // DUPLICATE
    public CapableData Duplicate()
    {
        CapableData new_data = new CapableData();
        new_data.id = this.id + "_copy"; // we add _copy to the id to avoid conflicts, it will be changed later in GenerateUniqueId
        new_data.kind = this.kind;
        new_data.position = this.position;
        new_data.orientation = this.orientation;
        new_data.skin = this.skin;
        new_data.inventory = new List<string>(this.inventory);
        new_data.capacities_ids = new List<string>(this.capacities_ids);
        new_data.effects = new List<Effect>(this.effects);
        new_data.effects_ttl = new List<float>(this.effects_ttl);
        return new_data;
    }

    // GET DETAILS
    public string GetDetails()
    {
        string details = $"Capable {id} :\n";
        details += $"  - kind : {kind}\n";
        details += $"  - position : {position}\n";
        details += $"  - orientation : {orientation}\n";
        details += $"  - skin : {skin}\n";
        details += $"  - inventory : {inventory.Count} items\n";
        details += $"  - capacities : {capacities_ids.Count} capacities\n";
        details += $"  - effects : {effects.Count} effects\n";
        return details;
    }
}