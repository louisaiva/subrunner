using System;
using UnityEngine;

/// <summary>
/// UI_ItemPool is the visual representation of a logical ItemPool. It contains UI_ItemStack
/// and needs to be attached to an ItemPool to work correclty
/// </summary>
public class UI_ItemPool : UI_ItemSlottable
{
    [Header("Item Pool")]
    public string PoolID = "stuff"; // the ID of the pool, used to link an UI_ItemPool and a ItemPool via Controller
    [SerializeField] protected ItemStorer storer;
    public override ItemStorer Storer { get { return storer; } }


    // ITEMPOOL ATTACHMENT
    public void AttachToPool(ItemStorer pool)
    {
        if (pool == null) { return; }

        // remove all callbacks
        if (this.storer != null) { DetachFromPool(); }

        // set new pool and register callbacks
        this.storer = pool;
        pool.OnStackCreated += add_ui_stack;
        pool.OnStackRemoved += remove_ui_stack;

        // apply the item type of the storer to the ui item pool
        apply_item_type_to_ui(pool.ItemType);

        // create the UI_ItemStack for matching the ItemStack of the ItemPool
        createStacksForPool();
    }

    private void apply_item_type_to_ui(ItemType itemType)
    {
        // if all or none we leave it as it is
        if (itemType == ItemType.All || itemType == ItemType.None) { return; }

        // if virtual we set file
        if (itemType == ItemType.Virtual) { this.item_slot_type = "file"; return; }

        // otherwise we set it to item
        this.item_slot_type = "item";
    }

    public void DetachFromPool()
    {
        if (storer == null) { return; }

        // remove all callbacks
        storer.OnStackCreated -= add_ui_stack;
        storer.OnStackRemoved -= remove_ui_stack;

        // clear pool reference
        storer = null;

        // clear the UI
        destroyAllStacks();
    }

    // UI_ITEMSTACK MANAGEMENT
    protected void createStacksForPool()
    {
        if (storer == null) { return; }

        // we go through the stacks of the pool and create a UI_ItemStack for each of them
        for (int i = 0; i < storer.Stacks.Count; i++)
        {
            ItemStack stack = storer.Stacks[i];
            add_ui_stack(stack);
        }
    }
}