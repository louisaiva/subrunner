using System.Collections.Generic;
using UnityEngine;

public class UI_Laptop : UI_Inventory
{
    /* protected virtual Laptop TargetLaptop
    {
        get
        {
            if (!UI_LaptopItemSlot.Instance.HasLaptop) { return null; }
            return UI_LaptopItemSlot.Instance.Laptop;
        }
    } */

    private Device target_device;

    [Header("Components")]
    [SerializeField] private MotherboardBuilder mb;

    // START
    /* protected virtual void Start()
    {
        // UI_LaptopItemSlot.Instance.OnItemChanged += HandleLaptopChanged;
        if (mb == null)
        {
            Debug.LogError($"(UI_Laptop) {name} has no MotherboardBuilder assigned, please set one in the inspector");
        }
        // mb = GetComponentInChildren<MotherboardBuilder>();

        // we initialize ourselves as big child bcz we may not have inventory
        // to init us from the start
        Init();
    } */

    // ATTACH / DETACH DEVICE
    public void AttachDevice(Device device)
    {
        if (device == null) { return; }
        if (target_device != null) { DetachDevice(); }
        target_device = device;
    }
    public void DetachDevice() { target_device = null; }

    // LAPTOP CHANGED
    private async void HandleLaptopChanged(Device new_device = null)
    {
        // if (Inventory != null) { Inventory.RemoveUI(this); }
        if (new_device == null)
        {
            // we disable the modules
            await pools[0].GetComponentInParent<Transitioner>(includeInactive: true)?.Hide();
            (pools[0] as UI_ModulePool)?.DisableModulePool();
            
            // we refresh the ui_inventory menu
            // UI_Manager.Instance.GetPool<UI_InventoryMenu>()?.RefreshItemPools();
            return;
        }

        // if we are here we have a laptop, so we create the motherboard & its modules
        // first we warn the new inventory that we are its ui now
        // TargetLaptop.Inventory.AddUI(this);
        LaptopInventory inventory = new_device.Inventory as LaptopInventory;
        mb.AssignLaptopInventory(inventory);
        // mb.Size = new Vector2Int(inventory.Columns, inventory.Rows);

        // we change the mb color based on laptop's color
        mb.SetColor(new_device.MB_Color);

        await System.Threading.Tasks.Task.Yield(); // wait for the next frame to ensure the UI is active

        // we create the modules based on the inventory save
        if (pools == null || pools.Count == 0 || pools[0] is not UI_ModulePool modulePool)
        {
            Debug.LogError($"(UI_Laptop) {name} has no UI_ModulePool to initialize from inventory, please set one in the inspector");
            return;
        }
        // modulePool.InitFromInventory(inventory);

        // we enable the modules
        modulePool.EnableModulePool();

        // we refresh the ui_inventory menu
        // UI_Manager.Instance.GetPool<UI_InventoryMenu>()?.RefreshItemPools();
    }


    // GETTERS
    public int GetItemSlotIndex(Item item)
    {
        if (item == null) { return -1; }
        if (pools == null || pools.Count == 0)
        {
            if (log) { Debug.LogWarning($"(UI_Laptop) {name} has no pools to search for item: {item.Reference}"); }
            return -1;
        }

        // we go through the module pool and check if one of the module slot contains the item,
        // if yes we return the index
        return pools[0].GetItemSlotIndex(item);
    }
}