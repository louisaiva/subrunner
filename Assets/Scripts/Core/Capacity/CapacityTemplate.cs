
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CapacityTemplate is a template for creating different types of capacities.
/// </summary>

public class CapacityTemplate : Capacity
{

    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData owner)
    {
        if (data is not CapacityTemplateData ctdata) { base.LoadData(data, owner); return; }

        base.LoadData(data, owner);
    }
    public override void UnloadData()
    {
        base.UnloadData();
    }

    // SAVE DYNAMIC DATA
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();

        // update the dynamic fields data with the current values of the capacity
        if (this.data == null) { return; }
        if (this.data is not CapacityTemplateData ctdata) { return; }
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        CapacityTemplateData static_data = new CapacityTemplateData(base.GetStaticData())
        {
            
        };

        return static_data;
    }
}

[Serializable] public class CapacityTemplateData : CapacityData
{


    // CONSTRUCTOR
    public CapacityTemplateData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new CapacityTemplateData(base.Duplicate() as CapacityData)
        {
            
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - this is a template capacity\n";
        return base.GetDetails() + details;
    }
}