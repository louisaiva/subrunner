using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// HDD module
/// </summary>
public class Module_HDD : Module
{
    protected override void apply_upgrade()
    {
        StoreCapacity store = GetCapacity<StoreCapacity>();
        if (store == null) { return; }

        // we get the capacity value from the effect
        int new_disk_capacity = Mathf.RoundToInt(get_upgrade_effect("storage capacity") * 1000); // in bytes
        store.SetCapacity(new_disk_capacity);
    }
}