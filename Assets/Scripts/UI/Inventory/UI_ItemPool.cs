using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI_ItemPool is a helper class to manage the item pool in the UI.
/// It can have a rule reference to filter the items that can be added to the pool.
/// It is a smaller pool of item inside a bigger UI_Pool (mainly UI_InventoryMenu)
/// 
/// 
/// 
/// todo move all the checks to ItemPool, a logical version of this which only
/// todo make sure we have space to put an item stack, etc
/// todo ItemPool  can work without UI_ItemPool, but not the opposite
/// </summary>
public class UI_ItemPool : UI_Slottable, Startable
{
    // [Header("Item Pool Parameters")]
    // public int MaxSlots = 9; // the maximum number of slots in the pool
    // public int MinSlots = 0;
    // public bool Scalable = false; // if true, the pool will dynamically add/remove slots

    [Header("Item Pool")]
    public string PoolID = "stuff"; // the ID of the pool, used to link an UI_ItemPool and a ItemPool via Controller
    public ItemPool pool;
    [SerializeField] protected string item_slot_type = "item"; // the type of the item slot to create, used to get the right prefab from the ItemBank


    [Header("UI Items")]
    [SerializeField] protected List<UI_Item> ui_items = new List<UI_Item>();
    public int Count { get { return ui_items.Count; } }
    public int EmptyCount { get { return ui_items.Where(ui_item => ui_item.Item == null).Count(); } }
    public int FullCount { get { return Count - EmptyCount; } }
    public int EnabledCount { get { return ui_items.Where(ui_item => !ui_item.Disabled).Count(); } }
    [SerializeField] protected bool destroy_empty_on_init = true; // if true, the empty slots will be destroyed on init
    public bool DoNotDisableEmptySlots = false;

    [Header("Components")]
    [SerializeField] protected ItemBank bank;
    public UI_Inventory UI_Inventory;

    [Header("Logs")]
    [SerializeField] protected bool log_storage = false;

    // START
    public virtual void InitStart()
    {
        bank = ItemBank.Instance;

        // we inspect all the ui_items we already have and delete them
        List<UI_Item> ui_items_found_at_start = new List<UI_Item>(GetComponentsInChildren<UI_Item>(includeInactive: true));
        ui_items.AddRange(ui_items_found_at_start);

        // clear the ui_items
        DestroyAllSlots();
    }

    // ITEM POOL ATTACHMENT
    public void AttachToPool(ItemPool pool)
    {
        if (pool == null) { return; }

        // remove all callbacks
        if (this.pool != null) { DetachFromPool(); }

        // set new pool and register callbacks
        this.pool = pool;
        pool.OnItemGrabbed += SyncUIWithPool;
        pool.OnItemDropped += SyncUIWithPool;
        pool.OnStacksChanged += SyncUIWithPool;

        // sync the UI with pool
        SyncUIWithPool();
    }
    public void DetachFromPool()
    {
        if (pool == null) { return; }

        // remove all callbacks
        pool.OnItemGrabbed -= SyncUIWithPool;
        pool.OnItemDropped -= SyncUIWithPool;
        pool.OnStacksChanged -= SyncUIWithPool;

        // clear pool reference
        pool = null;

        // clear the UI
        DestroyAllSlots();
    }


    // STACK SYNCING

    /// <summary>
    /// this method ensures that all Item found in the ItemPool stakcs
    /// have an equivalent UI_Item. Also make sure empty ItemPool stacks
    /// have an empty equivalent UI_Item. Also ensures the index matching,
    /// so that ItemPool.stacks[i] corresponds to UI_ItemPool.ui_items[i]
    /// </summary>
    protected void SyncUIWithPool(Item item = null)
    {
        if (pool == null) { return; }

        // we get the stacks
        List<ItemStack> stacks = pool.stacks;

        // we clear the ui_items // todo we can improve this by not clearing and modifying only modified ones but if it works without it sbetter ahah
        // todo this is bruteforce lol we should better do as the other todo says
        DestroyAllSlots();

        // we go through them all and check if we have corresponding ui_items
        for (int i=0;i<stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            UI_Item ui_item = CreateItemSlot().GetComponent<UI_Item>();
            ui_item.Store(stack);
            ui_items.Add(ui_item);
        }
    }

    // DESTROY / CREATE EMPTY ITEM SLOT
    public void DestroyEmptySlots()
    {
        /* // we go through the children to find the empty slots
        int i = MinSlots;
        while (i < Count)
        {
            UI_Item ui_item = ui_items[i];
            if (ui_item.Quantity == 0)
            {
                // we destroy the empty slot
                Destroy(ui_item.gameObject);
                ui_items.RemoveAt(i);
                continue; // we don't increment i, we just remove the empty slot
            }

            i++;
        }

        // we disable the first ones if we have some
        for (int j = 0; j < MinSlots && j < Count; j++)
        {
            UI_Item ui_item = ui_items[j];
            if (ui_item == null || ui_item.Quantity > 0) { continue; }
            ui_item.Disable();
        }

        // we verify that we still have more slots than the MinSlot
        if (Scalable && Count < MinSlots)
        {
            // we create the missing slots
            CreateEmptySlots(MinSlots - Count);
            if (log) { Debug.Log($"(UI_ItemPool) {name} created {MinSlots - Count} empty slots to reach the minimum of {MinSlots} slots"); }
        }
        else if (!Scalable && Count < MaxSlots)
        {
            // we create the missing slots
            CreateEmptySlots(MaxSlots - Count);
            if (log) { Debug.Log($"(UI_ItemPool) {name} created {MaxSlots - Count} empty slots to reach the maximum of {MaxSlots} slots"); }
        } */


        SyncUIWithPool();
    }
    public void DestroyAllSlots()
    {
        // we destroy all the slots
        foreach (UI_Item ui_item in ui_items)
        {
            Destroy(ui_item.gameObject);
        }
        ui_items.Clear();
    }
    public void CreateEmptySlots(int count)
    {
        // we create the empty slots
        for (int i = 0; i < count; i++) { CreateItemSlot(); }
        if (log) { Debug.Log($"(UI_ItemPool) created {count} empty slots in {name}"); }
    }
    public virtual GameObject CreateItemSlot(Item item = null)
    {
        // we create the item
        GameObject ui_slot = bank.CreateUI_Item(transform, item_slot_type);
        // ui_slot.transform.SetParent(transform);


        // we change the layer of the slot to the same as the pool
        ui_slot.layer = gameObject.layer;

        UI_Item ui_item = ui_slot.GetComponent<UI_Item>();
        ui_item.Init();

        // we assign the item to the UI_Item
        if (item != null) { ui_item.Store(item); }
        else { ui_item.Clear(); }

        // we add the item to the list
        ui_items.Add(ui_item);


        // reset the scale to 1 and local position to 0,0
        ui_slot.transform.localScale = Vector3.one;
        ui_slot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        return ui_slot;
    }

    // GETTERS
    public virtual int GetItemSlotIndex(Item item)
    {
        // we go through the ui_items and check if one of the slot contains the item,
        // if yes we return the index
        for (int i = 0; i < ui_items.Count; i++)
        {
            UI_Item ui_item = ui_items[i];
            foreach (Item ui_item_item in ui_item.GetItems())
            {
                // we check if the item is the same as the one we are looking for
                if (ui_item_item == item) { return i; }
            }
        }
        return -1;
    }
    /* public List<Item> GetAllItems()
    {
        List<Item> items = new List<Item>();
        foreach (UI_Item ui_item in ui_items)
        {
            items.AddRange(ui_item.GetItems());
        }
        return items;
    }
    public List<UI_Item> GetFilledSlots()
    {
        List<UI_Item> filled_slots = new List<UI_Item>();
        foreach (UI_Item ui_item in ui_items)
        {
            if (ui_item.Quantity > 0) { filled_slots.Add(ui_item); }
        }
        return filled_slots;
    } */
    public UI_Item GetSlotAt(int index)
    {
        if (index < 0 || index >= ui_items.Count) { return null; }
        return ui_items[index];
    }
    public List<UI_Item> GetAllSlots()
    {
        return ui_items;
    }




    // SLOTTABLE
    public override List<UI_Slot> GetSlots() { return ui_items.Cast<UI_Slot>().ToList(); }
    public override bool IsYourSlot(UI_Slot slot)
    {
        if (ui_items.Contains(slot)) { return true; }
        return false;
    }
}