using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;


public interface ICapacityData : IData
{
    string owner_id { get; set; }
    ICapacityData Duplicate();
}

[Serializable] public class CapacityData : ICapacityData
{
    [field: SerializeField] public string id { get; set; }
    [field: SerializeField] public string owner_id { get; set; }

    public string kind; // used to determine which kind of capacity it is. i.e. open,close,hover,interact etc (CapableBank uses this to instantiate the right prefab)
    public Vector2 local_position;
    public int layer = 0;
    public string tag = "";

    // CONSTRUCTORS
    public CapacityData() { }
    protected CapacityData(CapacityData parent) { copy_from_parent(parent); }
    private void copy_from_parent(CapacityData parent)
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
    public virtual ICapacityData Duplicate()
    {
        CapacityData new_data = new CapacityData
        {
            id = this.id + "_copy", // we add _copy to the id to avoid conflicts, it will be changed later in GenerateUniqueId
            local_position = this.local_position,
            owner_id = this.owner_id,
            kind = this.kind,
            layer = this.layer,
            tag = this.tag,
        };

        return new_data;
    }

    // GET DETAILS
    public virtual string GetDetails()
    {
        string details = $"Capacity {id} ({GetType().Name}):\n";
        details += $"  - owner_id : {owner_id}\n";
        details += $"  - kind : {kind}\n";
        details += $"  - local_position : {local_position}\n";
        details += $"  - layer : {LayerMask.LayerToName(layer)} ({layer})\n";
        details += $"  - tag : {tag}\n";
        return details;
    }
}
