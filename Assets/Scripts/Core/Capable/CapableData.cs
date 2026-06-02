using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;


/// <summary>
/// this attribute "RuntimeOnly" will never be serialized, and
/// will be ignored when duplicating data
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class RuntimeOnlyAttribute : Attribute { }


/// <summary>
/// this attribute "InstanceSpecific" mark fields that need to be
/// saved dynamically for all instances of a same kind. the other fiels
/// are shared between all instances of a same kind
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class InstanceSpecificAttribute : Attribute { }



public interface ICapableData : IData
{
    ICapableData Duplicate();
}

[Serializable] public class CapableData : ICapableData
{
    [field: SerializeField, InstanceSpecific] public string id { get; set; }
    [InstanceSpecific] public string kind; // used to determine which kind of capable it is. i.e. chest, IA, spawner or else
    // marked as InstanceSpecific bcz we need to store it so we can
    // load back the correct kind, and so to retrieve the KindSpecific fields when loading
    

    // GENERAL
    [InstanceSpecific] public Vector2 position;
    [InstanceSpecific] public Vector2 orientation;
    public int layer;
    public string tag;

    // CAPACITIES
    [InstanceSpecific] public List<string> capacities_ids;
    public int TotalCapacitiesCount() { return capacities_ids.Count; }
    
    // ANIM PLAYER
    public AnimPlayerData anim_data;

    // INVENTORY
    public InventoryData inventory;

    // BODY
    public FeetData feet_data;


    // EFFECTS
    [InstanceSpecific] public List<Effect> effects;
    [InstanceSpecific] public List<float> effects_ttl; // time to live for each effect, in seconds


    // EVENTS
    public event Action<CapableData> OnPositionChanged;
    public event Action<Capable, CapableData> OnCapableLoaded;
    public event Action<Capable, CapableData> OnCapableUnloaded;

    // RUNTIME ONLY
    [RuntimeOnly, NonSerialized] private Capable loaded_assigned_capable;
    [RuntimeOnly] public Capable Capable { get { return loaded_assigned_capable; } }
    [RuntimeOnly] public Vector2 Position
    {
        get
        {
            if (loaded_assigned_capable is not null && loaded_assigned_capable.Loaded) { return loaded_assigned_capable.transform.position; }
            return this.position;
        }
    }
    public virtual void OnLoaded(Capable capable)
    {
        loaded_assigned_capable = capable;
        OnCapableLoaded?.Invoke(capable, this);
    }
    public virtual void OnUnloaded(Capable capable)
    {
        OnCapableUnloaded?.Invoke(capable, this);
        loaded_assigned_capable = null;
    }


    // CONSTRUCTORS
    public CapableData() { }
    protected CapableData(CapableData parent) { copy_from_parent(parent); }
    private void copy_from_parent(CapableData parent)
    {
        if (parent == null) { return; }

        var type = parent.GetType();

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || !property.CanWrite) { continue; }
            if (Attribute.IsDefined(property, typeof(RuntimeOnlyAttribute))) { continue; }

            property.SetValue(this, property.GetValue(parent));
        }

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (Attribute.IsDefined(field, typeof(RuntimeOnlyAttribute))) { continue; }

            field.SetValue(this, field.GetValue(parent));
        }
    }


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

    // SET POSITION
    public void SetPosition(Vector2 new_position)
    {
        position = new_position;
        OnPositionChanged?.Invoke(this);
    }
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
