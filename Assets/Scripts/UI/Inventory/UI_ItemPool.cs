using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI_ItemPool is a helper class to manage the item pool in the UI.
/// It can have a rule reference to filter the items that can be added to the pool.
/// It is a smaller pool of item inside a bigger UI_Pool (mainly UI_InventoryMenu)
/// </summary>
public class UI_ItemPool : MonoBehaviour
{
    [Header("Item Pool Parameters")]
    public int MaxSlots = 9; // the maximum number of slots in the pool
    public int MinSlots = 0;
    public bool Scalable = false; // if true, the pool will dynamically add/remove slots
    [SerializeField] protected List<UI_Item> ui_items = new List<UI_Item>();
    public int Count { get { return ui_items.Count; } }
    public int EmptyCount { get { return ui_items.Where(ui_item => ui_item.Item == null).Count(); } }
    public int FullCount { get { return Count - EmptyCount; } }
    public int EnabledCount { get { return ui_items.Where(ui_item => !ui_item.is_disabled).Count(); } }
    [SerializeField] protected bool destroy_empty_on_init = true; // if true, the empty slots will be destroyed on init
    public bool DoNotDisableEmptySlots = false;

    [Header("Item Rule")]
    public string item_rule = ""; // the rule to check if the item is valid

    [Header("Components")]
    [SerializeField] protected ItemBank bank;
    // public Description Descriptor; // the description of the item pool
    public UI_Inventory UI_Inventory;
    // protected CanvasGroup group;
    // protected Transitioner transitioner;

    // [Header("Tween")]
    // [SerializeField] protected Sequence? fade_sequence = null;

    [Header("Logs")]
    [SerializeField] protected bool log = false;
    [SerializeField] protected bool log_storage = false;
    // [SerializeField] protected bool log_fading = false;

    public virtual void Init(UI_Inventory ui)
    {
        // we set the UI_Inventory
        this.UI_Inventory = ui;

        // we get the item bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();

        // we get the CanvasGroup
        // group = GetComponentInParent<CanvasGroup>(includeInactive: true);
        // if (group != null) { group.alpha = 0f; }
        // transitioner = GetComponentInParent<Transitioner>(includeInactive: true);
        // if (transitioner != null) { transitioner.Hide(0f); }

        // we clear the ui_items
        ui_items.Clear();

        // we add all existing uis to ui_items
        foreach (Transform child in transform)
        {
            // we check if the child is an empty slot
            UI_Item ui_item = child.GetComponent<UI_Item>();
            if (ui_item == null) { continue; }

            // we init the slot
            ui_item.Init();
            ui_items.Add(ui_item);

            if (!DoNotDisableEmptySlots && ui_item.Item == null) { ui_item.Disable(); }
            else { ui_item.Enable(); }
        }
        int awake_slots = ui_items.Count;

        // we destroy the existing empty slots & init the others
        if (destroy_empty_on_init) { DestroyEmptySlots(); }

        // we check if we are scalable or not
        if (log)
        {
            Debug.Log($"(UI_ItemPool) {name} just finished Init(), had {awake_slots} awake slots, now has {Count} slots\ndestroyed empty slots (& hereby may have recreated some to reach min or max slots)");
        }
    }

    // POOL EVENTS
    public event Action<UI_Item> OnPoolChanged = delegate { };
    public void NotifyPoolChanged(UI_Item ui_item)
    {
        OnPoolChanged.Invoke(ui_item);
    }


    // RULE CHECK
    public bool CanStore(Item item)
    {
        // we check if the item is valid
        if (item == null) { return false; }
        bool validate = item.ValidateRule(item_rule);
        if (!validate && log_storage)
        {
            Debug.LogWarning($"(UI_ItemPool) {name} can't store item {item.Reference} because it doesn't match the rule {item_rule}");
        }
        else if (log_storage)
        {
            Debug.Log($"(UI_ItemPool) {name} can store item {item.Reference} because it matches the rule {item_rule}");
        }

        return validate;
    }


    // GRAB / DROP
    public bool Grab(Item item)
    {
        // we check if we can add the item
        if (!CanStore(item))
        {
            if (log_storage) { Debug.Log("(UI_ItemPool) item " + item.Reference + " is not valid for this pool"); }
            return false;
        }

        // we try to store the item in the existing slots
        foreach (UI_Item ui_item in ui_items)
        {
            // we check if the slot can take the item
            bool stored = ui_item.Store(item);
            if (stored) { return true; }
        }

        // if we are here, we didn't find a slot to stack the item
        if (!Scalable)
        {
            if (log_storage)
            {
                Debug.Log("(UI_ItemPool) item " + item.Reference
            + $" is valid for this pool but no slot to store it found :// ({Count} slots currently in the pool)");
            }
            return false;
        }

        // if we are here, we have a scalable inventory
        CreateItemSlot(item);
        return true;
    }
    public bool Drop(Item item)
    {
        // we check if we can remove the item
        if (item == null) { return false; }

        // we go through the children to find the item
        foreach (UI_Item ui_item in ui_items)
        {
            // we try to unstore the item
            bool unstored = ui_item.Unstore(item);
            if (unstored)
            {
                // we successfully unstored the item !!
                // we check if the slot is empty & we are scalable
                if (ui_item.Quantity == 0 && Scalable)
                {
                    // we destroy the ui_item
                    Destroy(ui_item.gameObject);
                    ui_items.Remove(ui_item);
                    if (Count < MinSlots) { CreateEmptySlots(MinSlots - Count); }
                }
                return true;
            }
        }

        return false;
    }


    // DESTROY / CREATE EMPTY ITEM SLOT
    public void DestroyEmptySlots()
    {
        // we go through the children to find the empty slots
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
        }
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
        GameObject ui_slot = bank.CreateUI_Item();
        ui_slot.transform.SetParent(transform);

        // reset the scale to 1
        ui_slot.transform.localScale = Vector3.one;

        // we change the layer of the slot to the same as the pool
        ui_slot.layer = gameObject.layer;

        UI_Item ui_item = ui_slot.GetComponent<UI_Item>();
        ui_item.Init();

        // we assign the item to the UI_Item
        if (item != null) { ui_item.Store(item); }
        else { ui_item.Clear(); }

        // we add the item to the list
        ui_items.Add(ui_item);

        return ui_slot;
    }

    // FADE
    /* public async virtual Awaitable Fade(float duration = 0.1f, bool fade_in = true)
    {
        if (group == null) { return; }

        if (log_fading) { Debug.Log($"(UI_ItemPool) {name} fading {(fade_in ? "in" : "out")} with duration {duration} (from {group.alpha} to {(fade_in ? 1f : 0f)})"); }

        // we stop the last sequence
        if (fade_sequence != null && fade_sequence.Value.isAlive)
        {
            fade_sequence.Value.Stop();
            fade_sequence = null;
        }

        // we create a new sequence
        fade_sequence = Sequence.Create(useUnscaledTime: true)
            .Group(Tween.Custom(group.alpha, fade_in ? 1f : 0f, duration: duration,
                onValueChange: ctx => group.alpha = ctx));

        // we await til it's completed or stopped
        while (fade_sequence.Value.isAlive) { await System.Threading.Tasks.Task.Yield(); }
    } */
    /* public async virtual Awaitable Fade(float duration = 0.1f, bool fade_in = true)
    {
        if (transitioner == null) { return; }

        if (log_fading) { Debug.Log($"(UI_ItemPool) {name} fading {(fade_in ? "in" : "out")} with duration {duration}"); }

        // we call the transitioner
        if (fade_in) { await transitioner.Show(duration); }
        else { await transitioner.Hide(duration); }
    } */

    // public bool Transitionning => transitioner != null && transitioner.Transitioning;
    // public bool Hidden => transitioner != null && transitioner.Hidden;
    // public bool Shown => transitioner != null && transitioner.Shown;

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
    public List<Item> GetAllItems()
    {
        List<Item> items = new List<Item>();
        foreach (UI_Item ui_item in ui_items)
        {
            items.AddRange(ui_item.GetItems());
        }
        return items;
    }
}