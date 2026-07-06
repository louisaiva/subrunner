using System.Collections.Generic;
using UnityEngine;

public class LaptopInventory : Inventory
{
    [Header("Motherboard size")]
    public int Columns = 2;
    public int Rows = 2;
    public int MaxSlots => Columns * Rows;
    public event System.Action<int> MB_SizeChanged = delegate { };

    [Header("Items slots indexes")]
    protected Dictionary<ItemStack, int> items_slots = new Dictionary<ItemStack, int>(); // store les indexes de slot de chaque item via item.ID

    // START
    protected /* override  */void Start()
    {
        // we subscribe to events
        OnItemGrabbed += HandleModuleGrabbed;
        OnItemDropped += HandleModuleDropped;
        // base.Start();
        // (capable as Laptop).Processor.OnCPU_Changed();
    }

    // GRAB DROP REMOVE ITEMS
    /* public override bool Grab(Item item, List<UI_Inventory> uis_to_ignore = null)
    {
        // we save the laptop's files
        List<File> old_laptop_files = (capable as Device).GetFiles();

        // check if we can grab the item
        if (!base.Grab(item, uis_to_ignore)) { return false; }

        // we add an new index on the dictionary
        if (ui != null)
        {
            items_slots[item] = (ui as UI_Laptop).GetItemSlotIndex(item);
        }
        else
        {
            List<int> free_slots = get_free_slots();
            if (free_slots.Count == 0)
            {
                Debug.LogWarning($"(LaptopInventory) {name} has no free slots to grab {item.Reference}");
                return false;
            }
            items_slots[item] = free_slots[0]; // we grab the first free slot
        }

        // we check if files changed & call the file written event if yes
        List<File> new_laptop_files = (capable as Device).GetFiles();
        for (int i = 0; i < new_laptop_files.Count; i++)
        {
            if (old_laptop_files.Contains(new_laptop_files[i])) { continue; }
            (capable as Device).OnFileWritten?.Invoke(new_laptop_files[i]);
        }


        if (log) { Debug.Log($"(LaptopInventory) grabbed {item.Reference} in slot {items_slots[item]}"); }
        return true;
    }
    public override bool Drop(Item item, List<UI_Inventory> uis_to_ignore = null)
    {
        // check if we can drop the item
        if (!base.Drop(item, uis_to_ignore)) { return false; }

        // we remove the item from the dictionary
        items_slots.Remove(item);
        return true;
    }
    public override bool Remove(Item item)
    {
        // check if we can remove the item
        if (!base.Remove(item)) { return false; }

        // we remove the item from the dictionary
        items_slots.Remove(item);
        return true;
    } */

    // GETTERS
    public ItemStack GetStackInSlot(int slot_index)
    {
        // List<Item> items = new List<Item>();
        foreach (KeyValuePair<ItemStack, int> kvp in items_slots)
        {
            if (kvp.Value == slot_index)
            {
                return kvp.Key;
            }
        }
        return null;
    }
    private List<int> get_free_slots()
    {
        // we count the free slots
        List<int> free_slots = new List<int>();
        for (int i = 0; i < MaxSlots; i++)
        {
            if (GetStackInSlot(i) == null)
            {
                free_slots.Add(i);
            }
        }
        return free_slots;
    }


    // MODULES GRABBED/DROPPED
    private void HandleModuleGrabbed(Item item)
    {
        if (Capable is not Device device) { return; }
        if (item is Module_CPU cpu)
        {
            device.Processor.OnProcessorGrabbed(cpu);
        }
        else if (item is Module_HDD)
        {
            device.OnHDD_Changed();
        }
        else if (item is Module_Network)
        {
            device.OnNetworkModuleChanged();
        }
    }
    private void HandleModuleDropped(Item item)
    {
        if (Capable is not Device device) { return; }
        if (item is Module_CPU cpu)
        {
            device.Processor.OnProcessorDropped(cpu);
        }
        else if (item is Module_HDD)
        {
            device.OnHDD_Changed();
        }
        else if (item is Module_Network)
        {
            device.OnNetworkModuleChanged();
        }
    }
    public void HandleUI_ModuleMoved(ItemStack stack, int new_slot_index)
    {
        if (log) { Debug.Log($"(LaptopInventory) moved {stack.ItemReference} to slot {new_slot_index}"); }
        items_slots[stack] = new_slot_index;
    }

    // CHANGE MOTHERBOARD SLOTS SIZE
    /* private void on_laptop_upgrade()
    {
        // we check how many hdd do we have
        int hdd_count = GetItemsByRule("module:hdd").Count;
        int new_max_slots = 4 + hdd_count * 2; // each hdd provides 2 additional slots (and 4 is the based slots number of the motherboard)

        if (log) { Debug.Log($"(LaptopInventory) on_hdd_changed: {new_max_slots} slots"); }

        // we check if we have too many slots
        if (new_max_slots >= MaxSlots) { update_slots_count(new_max_slots); return; }

        // else we have less slots than before so we need to drop some items maybe
        List<Item> items_to_drop = new List<Item>();
        for (int i = MaxSlots - 1; i >= new_max_slots; i--)
        {
            // todo : better way to do this would be to directly move the modules
            // todo : to the free slots
            items_to_drop.AddRange(GetItemsInSlot(i));
        }

        if (items_to_drop.Count == 0) { update_slots_count(new_max_slots); return; }

        // we find the dropper
        DropCapacity dropper = capable.GetCapacity<DropCapacity>();
        if (dropper == null)
        {
            Debug.LogError($"(LaptopInventory) {name} has no DropCapacity, but we need to drop items to reduce the slots count");
            return;
        }

        string s = $"(LaptopInventory) dropping {items_to_drop.Count} items to reduce slots count from {MaxSlots} to {new_max_slots}";

        // we drop the items
        while (items_to_drop.Count > 0)
        {
            Item item = items_to_drop[0];
            s += $"\n- {item.Reference} in slot {items_slots[item]}";

            // drop it
            dropper.Select(item);
            dropper.Use(capable);
            items_to_drop.RemoveAt(0);
        }

        if (log) { Debug.Log(s); }

        // we update the slots count
        update_slots_count(new_max_slots);
    }
    private void update_slots_count(int slots_nb)
    {
        if (slots_nb == MaxSlots) { return; }

        Columns = 2;
        Rows = slots_nb / Columns;
        if (slots_nb % Columns != 0)
        {
            Debug.LogError($"(LaptopInventory) update_slots_count: {slots_nb} is not a multiple of {Columns}, please fix the code");
        }

        if (log) { Debug.Log($"(LaptopInventory) updating size to {Columns} x {Rows}"); }

        // we invoke the event
        MB_SizeChanged.Invoke(slots_nb);
    } */
}