using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This container class is useful
/// for capables that can contain other capables.
/// ie sofas. It dynamically load the contained capables,
/// saves whether a capable is contained or not, and
/// also have specific methods for SaveEngine to gather the
/// contained capables when saving (because contained capable are not registered in any Chunk)
/// </summary>
public class Container : Capable
{
    protected ContainerData contain_data => (ContainerData)data;

    public void ContainCapable(string id)
    {
        if (string.IsNullOrEmpty(id)) { return; }
        if (contain_data.contained_capable_ids.Contains(id)) { return; }
        contain_data.contained_capable_ids.Add(id);
    }
    public void FreeCapable(string id)
    {
        if (string.IsNullOrEmpty(id)) { return; }
        if (!contain_data.contained_capable_ids.Contains(id)) { return; }
        contain_data.contained_capable_ids.Remove(id);
    }


    protected virtual void load_capable_accordingly(Capable capable)
    {
        // here the lower container classes can decide to do something when we load a capable that
        // is contained. for exaple, we want to call the capable Sit capacity if we are a sofa
    }

    // DATA MANAGEMENT
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);
        if (data is not ContainerData contain_data) { return; }
        if (contain_data.contained_capable_ids == null) { return; }

        // we load all the capable in the contained_capable_ids list
        List<string> ids_to_load = new List<string>(contain_data.contained_capable_ids);
        foreach (string capable_id in ids_to_load)
        {
            if (string.IsNullOrEmpty(capable_id)) { continue; }
            
            // we want to load the capable bcz it has no chunk, so it won't ever be loaded
            Capable capable = CapableEngine.Instance.LoadCapableInstantly(capable_id);
            
            // then we update manually the show/hide of the capable bcz it was not get 
            /* bool is_showing = capable.AnimPlayer.IsVisible();
            if (is_showing) { capable.AnimPlayer.Show(); }
            else { capable.AnimPlayer.Hide(); } */

            load_capable_accordingly(capable);
        }
    }
    public override void UnloadData()
    {
        if (contain_data.contained_capable_ids == null) { return; }
        
        // we unload all the capable in the contained_capable_ids list
        foreach (string capable_id in contain_data.contained_capable_ids)
        {
            if (string.IsNullOrEmpty(capable_id)) { continue; }

            CapableEngine.Instance.UnloadCapableInstantly(capable_id);
        }
        base.UnloadData();
    }
    public override ICapableData GetStaticData()
    {
        ContainerData static_data = new ContainerData((CapableData)base.GetStaticData())
        {
            contained_capable_ids = new List<string>(this.contain_data.contained_capable_ids)
        };

        return static_data;
    }
}


public class ContainerData : CapableData
{
    public List<string> contained_capable_ids = new List<string>();

    // CONSTRUCTOR
    public ContainerData() : base() { }
    public ContainerData(CapableData parent) : base(parent) { }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        return new ContainerData(base.Duplicate() as CapableData)
        {
            contained_capable_ids = new List<string>(this.contained_capable_ids)
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = base.GetDetails();
        details += $"  - contained_capable_ids: {contained_capable_ids.Count}\n";
        foreach (string capable_id in contained_capable_ids)
        {
            details += $"    - {capable_id}\n";
        }
        return details;
    }
}