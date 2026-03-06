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
    

    // DUPLICATE
    public virtual ICapacityData Duplicate()
    {
        CapacityData new_data = new CapacityData();
        new_data.id = this.id + "_copy"; // we add _copy to the id to avoid conflicts, it will be changed later in GenerateUniqueId
        new_data.kind = this.kind;

        return new_data;
    }

    // GET DETAILS
    public virtual string GetDetails()
    {
        string details = $"Capacity {id} :\n";
        details += $"  - kind : {kind}\n";
        return details;
    }
}
