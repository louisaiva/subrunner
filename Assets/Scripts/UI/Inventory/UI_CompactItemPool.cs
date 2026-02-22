using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// UI_CompactItemPool is a modified UI_ItemPool that shows the items of a full inventory.
/// It create,holds and destroy their own ItemStacks in order to have the most compact ui possible.
/// It means it has to copy all items from a specified inventory (or set of itempools ?) and then
/// recreate their own ItemStacks for the items to be as stacked as possible, and then FINALLY it creates and attaches UI_ItemStack to those
/// stacks.
/// 
/// It also has a ItemRule that can be dynamically modified to filter the shown stacks
/// </summary>
public class UI_CompactItemPool : UI_ItemSlottable
{
    [Header("Logs")]
    [SerializeField] protected bool log_grab_drop = false;

    [Header("Item Pool")]
    [SerializeField] protected ItemStorer storer;
    public override ItemStorer Storer { get { return storer; } }

    [Header("ItemStacks")]
    protected List<ItemStack> stacks = new List<ItemStack>(); // the list of stacks that are currently shown in the pool, they are created by the pool and not linked to the stacks of the inventory, but they are updated when those are updated (quantity, item change, etc.)

    [Header("Outline Slot")]
    [SerializeField] protected UI_OutlineSlot outliner;
    public UI_OutlineSlot OutlinerReceivable => outliner;


    // ITEMPOOL ATTACHMENT
    public void AttachToStorer(ItemStorer real_holder)
    {
        if (real_holder == null) { return; }

        // remove all callbacks
        if (storer != null) { DetachFromStorer(); }

        // set new real_holder and register callbacks
        storer = real_holder;
        storer.OnItemGrabbed += grab_item;
        storer.OnItemDropped += drop_item;

        // create the UI_ItemStack for matching the ItemStack of the ItemPool
        createStacksForStorer();

        // and we make sure the outline slot is disabled
        outliner.Disable();
    }
    public void DetachFromStorer()
    {
        if (storer == null) { return; }

        // remove all callbacks
        storer.OnItemGrabbed -= grab_item;
        storer.OnItemDropped -= drop_item;

        // clear pool reference
        storer = null;

        // clear the UI
        destroyAllStacks();
    }

    // UI_ITEMSTACK MANAGEMENT
    protected void createStacksForStorer()
    {
        if (storer == null) { return; }

        // we go through all items of the storer and grab them so we copy them into our stacks
        // (that will create ui_stacks)
        for (int i = 0; i < storer.Items.Count; i++)
        {
            Item item = storer.Items[i];
            grab_item(item);
        }
    }

    // ITEM GRABBED / DROPPED
    protected void grab_item(Item item)
    {
        if (log_grab_drop) { Debug.Log($"(UI_CompactItemPool) grabbing item {item.name}"); }

        // 1 - verify we have not already grabbed this item
        if (stacks.Exists(s => s.Items.Contains(item))) { if (log_grab_drop) { Debug.Log($"(UI_CompactItemPool) item {item.name} already grabbed"); } return; }


        // 2 - try to grab it in existing stacks
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.CanAdd(item))
            {
                stack.Add(item);
                if (log_grab_drop) { Debug.Log($"(UI_CompactItemPool) added item {item.name} to existing stack"); }
                return;
            }
        }

        // 3 - creates a new ItemStack
        ItemStack new_stack = new ItemStack(storer);
        new_stack.Add(item);
        stacks.Add(new_stack);
        add_ui_stack(new_stack);
        if (log_grab_drop) { ui_stacks.LastOrDefault().log_drop = true; }

        if (log_grab_drop) { Debug.Log($"(UI_CompactItemPool) created new stack for grabbed item {item.name}"); }
    }
    protected void drop_item(Item item)
    {
        // we find the stack that contains the item and remove it
        ItemStack stack = stacks.Find(s => s.Items.Contains(item));
        if (stack == null) { return; }
        
        if (log_grab_drop) { Debug.Log($"(UI_CompactItemPool) dropping item {item.name}"); }
        stack.Remove(item);
        if (stack.IsEmpty) { stacks.Remove(stack); remove_ui_stack(stack); if (log_grab_drop) { Debug.Log($"(UI_CompactItemPool) removed stack for dropped item {item.name} because it is now empty"); } }
    }



    // SLOTTABLE
    public override List<UI_Slot> GetSlots()
    {
        List<UI_Slot> slots = base.GetSlots();
        if (!outliner.Disabled) { slots.Add(outliner); }
        return slots;
    }
    public override bool IsYourSlot(UI_Slot slot)
    {
        if (slot == outliner) { return true; }
        return base.IsYourSlot(slot);
    }

}