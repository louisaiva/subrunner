using System.Collections.Generic;
using UnityEngine;
using System;


public interface ICapacityData : IData
{
    string id { get; set; }
    ICapacityData Duplicate();
}

[Serializable] public class CapacityData : ICapacityData
{
    [field: SerializeField] public string id { get; set; }
    public string kind; // used to determine which kind of capacity it is. i.e. open,close,hover,interact etc (CapableBank uses this to instantiate the right prefab)
    public Vector2 local_position;
    public int layer = 0;
    public string tag = "";


    // DUPLICATE
    public virtual ICapacityData Duplicate()
    {
        CapacityData new_data = new CapacityData
        {
            id = this.id + "_copy", // we add _copy to the id to avoid conflicts, it will be changed later in GenerateUniqueId
            kind = this.kind,
            layer = this.layer,
            tag = this.tag,
        };

        return new_data;
    }

    // GET DETAILS
    public virtual string GetDetails()
    {
        string details = $"Capacity {id} :\n";
        details += $"  - kind : {kind}\n";
        details += $"  - local_position : {local_position}\n";
        details += $"  - layer : {LayerMask.LayerToName(layer)} ({layer})\n";
        details += $"  - tag : {tag}\n";
        return details;
    }
}
