


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IColliderData
{
    IColliderData Duplicate();
}




[Serializable] public class ColliderData : IColliderData
{
    // collider data
    public Vector2 local_position;
    public int layerID;
    public bool used_for_pathfinding;
    public ShadowCasterData shadow_caster_data;
    public bool is_trigger;
    public Vector2 offset;


    // DUPLICATE
    public virtual IColliderData Duplicate()
    {
        ColliderData new_data = new ColliderData()
        {
            local_position = this.local_position,
            layerID = this.layerID,
            used_for_pathfinding = this.used_for_pathfinding,
            is_trigger = this.is_trigger,
            offset = this.offset,
            shadow_caster_data = this.shadow_caster_data.Duplicate()
        };

        return new_data;
    }


    // GET DETAILS
    public virtual string GetDetails()
    {
        string details = $"collider data :\n";
        details += $"     - local_position : {local_position}\n";
        details += $"     - layerID : {layerID} ({LayerMask.LayerToName(layerID)})\n";
        details += $"     - used_for_pathfinding : {used_for_pathfinding}\n";
        details += $"     - is_trigger : {is_trigger}\n";
        details += $"     - offset : {offset}\n";
        details += $"     - shadow_caster_data : {(shadow_caster_data != null ? "\n" + shadow_caster_data.GetDetails() : "NONE")}\n";
        return details;
    }
}
[Serializable] public class CircleData : ColliderData
{
    public float radius;


    // CONSTRUCTOR
    public CircleData() { }
    public CircleData(ColliderData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override IColliderData Duplicate()
    {
        return new CircleData(base.Duplicate() as ColliderData)
        {
            radius = this.radius
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = $"     - radius : {radius}\n";
        return base.GetDetails() + details;
    }
}

[Serializable] public class BoxData : ColliderData
{
    public Vector2 size;

    // CONSTRUCTOR
    public BoxData() {}
    public BoxData(ColliderData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override IColliderData Duplicate()
    {
        return new BoxData(base.Duplicate() as ColliderData)
        {
            size = this.size
        };
    }
    // GET DETAILS
    public override string GetDetails()
    {
        string details = $"     - size : {size}\n";
        return base.GetDetails() + details;
    }
}


[Serializable] public class ShadowCasterData
{
    public bool cast_and_self; // if true it means we cast + self, otherwise we only cast
    public List<string> used_layers = new List<string>();
    
    // DUPLICATE
    public ShadowCasterData Duplicate()
    {
        ShadowCasterData new_data = new ShadowCasterData()
        {
            cast_and_self = this.cast_and_self,
            used_layers = new List<string>(this.used_layers)
        };

        return new_data;
    }

    // GET DETAILS
    public string GetDetails()
    {
        string details = $"         - cast_and_self : {cast_and_self}\n";
        if (used_layers != null && used_layers.Count > 0) { details += $"         - used_layers : {string.Join(", ", used_layers)}\n"; }
        else { details += $"         - used_layers : None\n"; }
        return details;
    }
}