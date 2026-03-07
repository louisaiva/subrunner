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
    public Vector2 position;
    // public Vector2 inputs; ????
    public Vector2 orientation;

    // ANIM PLAYER
    public AnimData anim_data;

    // BODY
    public BodyData body_data;

    // INVENTORY
    public InventoryData inventory;

    // CAPACITIES
    public List<string> capacities_ids;

    // EFFECTS
    public List<Effect> effects;
    public List<float> effects_ttl; // time to live for each effect, in seconds


    // DUPLICATE
    public virtual ICapableData Duplicate()
    {
        CapableData new_data = new CapableData();

        // general
        new_data.id = this.id + "_copy"; // we add _copy to the id to avoid conflicts, it will be changed later in GenerateUniqueId
        new_data.kind = this.kind;
        new_data.position = this.position;
        new_data.orientation = this.orientation;

        // anim
        new_data.anim_data = anim_data.Duplicate();

        // body
        if (this.body_data != null) { new_data.body_data = this.body_data.Duplicate(); }

        // capacities & effects
        new_data.capacities_ids = new List<string>(this.capacities_ids);
        new_data.effects = new List<Effect>(this.effects);
        new_data.effects_ttl = new List<float>(this.effects_ttl);
        return new_data;
    }

    // GET DETAILS
    public virtual string GetDetails()
    {
        string details = $"Capable '{id}' :\n";
        details += $"  - kind : {kind}\n";
        details += $"  - position : {position}\n";
        details += $"  - orientation : {orientation}\n";
        if (body_data != null) { details += $"  - {body_data.GetDetails()}\n"; }
        else { details += $"  - no body\n"; }
        if (inventory != null) { details += $"  - {inventory.GetDetails()}"; }
        else { details += $"  - no inventory\n"; }
        details += $"  - capacities : {capacities_ids.Count} capacities\n";
        details += $"  - effects : {effects.Count} effects\n";
        details += $"  - {anim_data.GetDetails()}\n";
        return details;
    }
}


// ANIMATIONS
[Serializable] public class AnimData
{
    public string skin;
    public List<AnimCapacityPriority> anim_capacity_priorities;

    // player sr data
    public string material_path;
    public int sorting_layer_id;
    public int order_in_layer;


    // layers
    public List<AnimLayerData> layers;


    // GET & DUPLICATE
    public AnimData Duplicate()
    {
        AnimData new_data = new AnimData();
        new_data.skin = this.skin;
        new_data.anim_capacity_priorities = new List<AnimCapacityPriority>(this.anim_capacity_priorities);
        new_data.material_path = this.material_path;
        new_data.sorting_layer_id = this.sorting_layer_id;
        new_data.order_in_layer = this.order_in_layer;
        new_data.layers = new List<AnimLayerData>(this.layers);
        return new_data;
    }
    public string GetDetails()
    {
        string details = $"anim_data :\n";
        details += $"     - skin : {skin}\n";
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
[Serializable] public class BodyData
{
    public List<BoxData> box_colliders;
    public List<CircleData> circle_colliders;

    // DUPLICATE
    public BodyData Duplicate()
    {
        return new BodyData() { 
            box_colliders = new List<BoxData>(this.box_colliders), 
            circle_colliders = new List<CircleData>(this.circle_colliders)
        };
    }

    // GET DETAILS
    public string GetDetails()
    {
        string details = $"body_data :\n";
        if (box_colliders != null) { details += $"     - box_colliders : {box_colliders.Count} box colliders\n"; }
        else { details += $"     - box_colliders : null\n"; }
        if (circle_colliders != null) { details += $"     - circle_colliders : {circle_colliders.Count} circle colliders"; }
        else { details += $"     - circle_colliders : null"; }
        return details;
    }

}
