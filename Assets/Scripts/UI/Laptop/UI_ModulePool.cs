using UnityEngine;

/// <summary>
/// UI_ModulePool is a helper class to manage the module pool in the UI_Laptop
/// it directly inherits from UI_ItemPool
/// and is used to manage the modules in the laptop
/// </summary>
public class UI_ModulePool : UI_ItemPool
{

    public override GameObject CreateEmptyItemSlot()
    {
        // we create the item
        GameObject ui_slot = bank.CreateUI_Module(null);
        ui_slot.transform.SetParent(transform);

        if (debug) { Debug.Log("(UI_ModulePool) created an empty ui_module with the help of Bank");}

        // reset the scale to 1
        ui_slot.transform.localScale = Vector3.one;

        // we change the layer of the slot to the same as the pool
        ui_slot.layer = gameObject.layer;

        return ui_slot;
    }
}