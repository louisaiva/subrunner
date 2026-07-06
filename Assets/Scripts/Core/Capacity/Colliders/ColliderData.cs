


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public int pathfinding_area = -1; // -1 -> not used for pathfinding
    [RuntimeOnly] public bool UsedForPathfinding => pathfinding_area != -1;
    public ShadowCasterData shadow_caster_data;
    public bool is_trigger;
    public Vector2 offset;

    public ColliderData() { }
    protected ColliderData(ColliderData parent) { copy_from_parent(parent); }
    private void copy_from_parent(ColliderData parent)
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
    public virtual IColliderData Duplicate()
    {
        ColliderData new_data = new ColliderData()
        {
        };

        new_data.local_position = this.local_position;
        new_data.layerID = this.layerID;
        new_data.pathfinding_area = this.pathfinding_area;
        new_data.is_trigger = this.is_trigger;
        new_data.offset = this.offset;
        new_data.shadow_caster_data = this.shadow_caster_data == null ? null : this.shadow_caster_data.Duplicate();

        return new_data;
    }


    // GET DETAILS
    public virtual string GetDetails()
    {
        string details = $"collider data :\n";
        details += $"     - local_position : {local_position}\n";
        details += $"     - layerID : {layerID} ({LayerMask.LayerToName(layerID)})\n";
        details += $"     - pathfinding_area : {pathfinding_area}\n";
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
    public CircleData(ColliderData parent) : base(parent) { }

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
    public BoxData(ColliderData parent) : base(parent) { }

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