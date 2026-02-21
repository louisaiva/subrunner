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


    [Header("UI Items Stacks")]
    [SerializeField] protected List<UI_ItemStack> ui_stacks = new List<UI_ItemStack>();
    public int Count { get { return ui_stacks.Count; } }
    public int EmptyCount { get { return ui_stacks.Count(ui_stack => ui_stack.Stack.Item == null); } }
    public int FullCount { get { return Count - EmptyCount; } }
    public int EnabledCount { get { return ui_stacks.Count(ui_stack => !ui_stack.Disabled); } }
    
    // [Obsolete("This variable is not used anymore, as not disabling allow us to navigate to it. Use AlwaysShow if you want to always show the pool")]
    // public bool DoNotDisableEmptySlots = false;
    public bool AlwaysShow = false; // if true, the pool will always be shown even if it is empty/disabled (affects UI_InventoryMenu)

    [Header("Components")]
    [SerializeField] protected ItemBank bank;

    [Header("Logs")]
    [SerializeField] protected bool log_storage = false;

    // START
    public virtual void InitStart()
    {
        bank = ItemBank.Instance;

        // we inspect all the ui_items we already have and delete them
        List<UI_ItemStack> ui_items_found_at_start = new List<UI_ItemStack>(GetComponentsInChildren<UI_ItemStack>(includeInactive: true));
        ui_stacks.AddRange(ui_items_found_at_start);

        // clear the ui_items
        destroyAllStacks();
    }

    // ITEMPOOL ATTACHMENT
    public void AttachToPool(ItemPool pool)
    {
        if (pool == null) { return; }

        // remove all callbacks
        if (this.pool != null) { DetachFromPool(); }

        // set new pool and register callbacks
        this.pool = pool;
        pool.OnStackCreated += add_ui_stack;
        pool.OnStackRemoved += remove_ui_stack;

        // create the UI_ItemStack for matching the ItemStack of the ItemPool
        createStacksForPool();
    }
    public void DetachFromPool()
    {
        if (pool == null) { return; }

        // remove all callbacks
        pool.OnStackCreated -= add_ui_stack;
        pool.OnStackRemoved -= remove_ui_stack;

        // clear pool reference
        pool = null;

        // clear the UI
        destroyAllStacks();
    }

    // UI_ITEMSTACK MANAGEMENT
    protected void createStacksForPool()
    {
        if (pool == null) { return; }

        // we go through the stacks of the pool and create a UI_ItemStack for each of them
        for (int i = 0; i < pool.stacks.Count; i++)
        {
            ItemStack stack = pool.stacks[i];
            add_ui_stack(stack);
        }
    }
    protected void destroyAllStacks()
    {
        // we destroy all the slots
        for (int i = 0; i < ui_stacks.Count; i++)
        {
            Destroy(ui_stacks[i].gameObject);
        }
        ui_stacks.Clear();
    }

    // LOW LEVEL UI_ITEMSTACK MANAGEMENT
    protected void add_ui_stack(ItemStack stack)
    {
        UI_ItemStack ui_stack = create_ui_stack(stack); // creates the ui_stack
        ui_stacks.Add(ui_stack); // add it
    }
    protected UI_ItemStack create_ui_stack(ItemStack stack)
    {
        // we create the item
        GameObject ui_slot = bank.CreateUI_Item(transform, item_slot_type);

        // we change the layer of the slot to the same as the pool
        ui_slot.layer = gameObject.layer;

        // reset the scale to 1 and local position to 0,0
        ui_slot.transform.localScale = Vector3.one;
        ui_slot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // initialize the ui_item and return it
        UI_ItemStack ui_stack = ui_slot.GetComponent<UI_ItemStack>();
        ui_stack.Init(stack);

        return ui_stack;
    }
    protected void remove_ui_stack(ItemStack stack)
    {
        // we find the corresponding ui_itemstack to the stack
        UI_ItemStack ui_stack = get_ui_stack_attached_to_stack(stack);
        if (ui_stack == null) { return; }

        // we destroy the ui_stack
        ui_stacks.Remove(ui_stack);
        Destroy(ui_stack.gameObject);
    }


    // GETTERS
    protected UI_ItemStack get_ui_stack_attached_to_stack(ItemStack stack)
    {
        for (int i = 0; i < ui_stacks.Count; i++)
        {
            UI_ItemStack ui_stack = ui_stacks[i];
            if (ui_stack.Stack == stack) { return ui_stack; }
        }
        return null;
    }
    public virtual int GetItemSlotIndex(Item item)
    {
        // we go through the ui_stacks and check if one of the slot contains the item,
        // if yes we return the index
        for (int i = 0; i < ui_stacks.Count; i++)
        {
            UI_ItemStack ui_stack = ui_stacks[i];
            if (ui_stack.Stack.HasItem(item)) { return i; }
        }
        return -1;
    }
    public UI_ItemStack GetSlotAt(int index)
    {
        if (index < 0 || index >= ui_stacks.Count) { return null; }
        return ui_stacks[index];
    }
    public List<UI_ItemStack> GetAllSlots()
    {
        return ui_stacks;
    }


    // SLOTTABLE
    public override List<UI_Slot> GetSlots()
    {
        return ui_stacks.Cast<UI_Slot>().Where(s => !s.Disabled).ToList();
    }
    public override bool IsYourSlot(UI_Slot slot)
    {
        if (ui_stacks.Contains(slot)) { return true; }
        return false;
    }
}