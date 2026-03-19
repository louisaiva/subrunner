using System.Collections.Generic;
using UnityEngine;
using System;

public interface ICapableData : IData
{
    string id { get; set; }
    ICapableData Duplicate();
}

[Serializable] public class CapableData : ICapableData
{
    [field: SerializeField] public string id { get; set; }
    public string kind; // used to determine which kind of capable it is. i.e. chest, IA, spawner or else
    
    // GENERAL
    public Vector2 position;
    public Vector2 orientation;
    public int layer;
    public string tag;

    // ANIM PLAYER
    public AnimData anim_data;

    // INVENTORY
    public InventoryData inventory;

    // BODY
    public FeetData feet_data;

    // CAPACITIES
    public List<string> capacities_ids;

    // EFFECTS
    public List<Effect> effects;
    public List<float> effects_ttl; // time to live for each effect, in seconds


    // DUPLICATE
    public virtual ICapableData Duplicate()
    {
        return new CapableData
        {
            // general
            id = this.id + "_copy", // we add _copy to the id to avoid conflicts, it will be changed later in GenerateUniqueId
            kind = this.kind,
            position = this.position,
            orientation = this.orientation,
            layer = this.layer,
            tag = this.tag,

            // anim
            anim_data = anim_data.Duplicate(),

            // inventory
            inventory = this.inventory != null ? this.inventory.Duplicate() : null,

            // body
            feet_data = this.feet_data != null ? this.feet_data.Duplicate() : null,

            // capacities & effects
            capacities_ids = new List<string>(this.capacities_ids),
            effects = new List<Effect>(this.effects),
            effects_ttl = new List<float>(this.effects_ttl),
        };
    }

    // GET DETAILS
    public virtual string GetDetails()
    {
        string details = $"Capable '{id}' :\n";
        details += $"  - kind : {kind}\n";
        details += $"  - position : {position}\n";
        details += $"  - orientation : {orientation}\n";
        details += $"  - layer : {LayerMask.LayerToName(layer)} ({layer})\n";
        details += $"  - tag : {tag}\n";
        if (feet_data != null) { details += $"  - {feet_data.GetDetails()}\n"; }
        else { details += $"  - no feet\n"; }
        if (inventory != null) { details += $"  - {inventory.GetDetails()}"; }
        else { details += $"  - no inventory\n"; }
        if (capacities_ids != null) { details += $"  - capacities : {(capacities_ids.Count == 0 ? "none" : string.Join(", ", capacities_ids))}\n"; }
        else { details += $"  - no capacities\n"; }
        if (effects != null) { details += $"  - effects : {effects.Count} effects\n"; }
        else { details += $"  - no effects\n"; }
        if (anim_data != null) { details += $"  - {anim_data.GetDetails()}\n"; }
        else { details += $"  - no anim_data\n"; }
        return details;
    }
}


// ANIMATIONS
[Serializable] public class AnimData
{
    public string skin;
    public List<AnimCapacityPriority> anim_capacity_priorities;
    public string current_capacity; // runtime only

    // player sr data
    public string material_path;
    public int sorting_layer_id;
    public int order_in_layer;

    // layers
    public List<AnimLayerData> layers;


    // GET & DUPLICATE
    public AnimData Duplicate()
    {
        AnimData new_data = new AnimData
        {
            skin = this.skin,
            current_capacity = this.current_capacity,
            material_path = this.material_path,
            sorting_layer_id = this.sorting_layer_id,
            order_in_layer = this.order_in_layer,
            layers = new List<AnimLayerData>(this.layers)
        };

        // duplicate anim_capacity_priorities
        if (this.anim_capacity_priorities != null)
        {
            new_data.anim_capacity_priorities = new List<AnimCapacityPriority>();
            foreach (AnimCapacityPriority acp in this.anim_capacity_priorities)
            {
                new_data.anim_capacity_priorities.Add(acp.Duplicate());
            }
        }
        else { new_data.anim_capacity_priorities = null; }

        return new_data;
    }
    public string GetDetails()
    {
        string details = $"anim_data :\n";
        details += $"     - skin : {skin}\n";
        details += $"     - current_capacity : {current_capacity}\n";
        if (anim_capacity_priorities != null) { details += $"     - anim_capacity_priorities : {anim_capacity_priorities.Count} priorities\n"; }
        else { details += $"     - anim_capacity_priorities : null\n"; }
        if (layers != null) { details += $"     - layers : {layers.Count} layers"; }
        else { details += $"     - layers : null"; }
        return details;
    }
}
[Serializable] public class AnimLayerData
{
    public string skin;
    public Vector2 local_position;

    // layer sr data
    public string material_path;
    public int sorting_layer_id;
    public int order_in_layer;
}



// COLLIDERS
[Serializable] public class FeetData
{
    public List<BoxData> box_colliders;
    public List<CircleData> circle_colliders;

    // DUPLICATE
    public FeetData Duplicate()
    {
        return new FeetData() { 
            box_colliders = new List<BoxData>(this.box_colliders), 
            circle_colliders = new List<CircleData>(this.circle_colliders)
        };
    }

    // GET DETAILS
    public string GetDetails()
    {
        string details = $"feet_data :\n";
        if (box_colliders != null) { details += $"     - box_colliders : {box_colliders.Count} box colliders\n"; }
        else { details += $"     - box_colliders : null\n"; }
        if (circle_colliders != null) { details += $"     - circle_colliders : {circle_colliders.Count} circle colliders"; }
        else { details += $"     - circle_colliders : null"; }
        return details;
    }

}
