using UnityEngine;

/// <summary>
/// UI_ModulePool is a helper class to manage the module pool in the UI_Laptop
/// it directly inherits from UI_ItemPool
/// and is used to manage the modules in the laptop
/// </summary>
public class UI_ModulePool : UI_ItemPool
{

    public override void Init()
    {
        // we get the item bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();

        // no need for creating slots
    }
}