using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// HDD module
/// </summary>
public class Module_HDD : Module
{

    // UPGRADE
    protected override void apply_upgrade()
    {
        StoreCapacity store = GetCapacity<StoreCapacity>();
        if (store == null) { return; }

        // we get the capacity value from the effect
        int new_disk_capacity = Mathf.RoundToInt(get_upgrade_effect("storage") * 1000); // in bytes
        store.SetCapacity(new_disk_capacity);
    }


    // INSPECTABLE
    public override string InspectLabel => "explore";
    public override void Inspect()
    {
        // we open the HDD info
        StoreCapacity disk = GetCapacity<StoreCapacity>();
        if (disk == null) { return; }
        if (debug) { Debug.Log($"(Module_HDD) Inspecting HDD, opening HDD info"); }
        UI_Manager.Instance.GetPool("hdd").gameObject.GetComponent<UI_HDD>().SetDisk(disk);
        UI_Manager.Instance.StackPool("hdd");
    }
}