using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public interface ItemStorer
{
    public List<Item> Items { get; }
    public string ItemRule { get; }
    public int Count { get; }
    public event Action<Item> OnItemGrabbed;
    public event Action<Item> OnItemDropped;
    public bool Grab(Item item);
    public bool Drop(Item item);
}

public class ItemPool : MonoBehaviour, ItemStorer
{
    [Header("Items")]
    [SerializeField] private List<ItemStack> stacks = new List<ItemStack>();
    public List<Item> Items { get { return stacks.SelectMany(s => s.Items).ToList(); } }
    public string ItemRule { get { return item_rule; } }
    public virtual int Count { get { return Items.Count; } }
    public virtual bool HasSpaceLeft { get { return Scalable || stacks.Count < MaxStacks || stacks.Any(s => !s.IsFull); } }

    [Header("Item Stacks Parameters")]
    public int MaxStacks = 9; // the maximum number of stacks in the pool
    public int MinStacks = 0;
    public bool Scalable = false; // if true, the pool will dynamically add/remove stacks


    // [Header("Events")]
    public event Action<Item> OnItemGrabbed = delegate { };
    public event Action<Item> OnItemDropped = delegate { };


    [Header("Item Rule")]
    public string item_rule = ""; // the rule to check if the item is valid

    [Header("Inventory & Capable")]
    public Inventory Inventory;

    [Header("Logs")]
    [SerializeField] protected bool log = false;
    [SerializeField] protected bool log_storage = false;

    public void AttachToInventory(Inventory inventory)
    {
        this.Inventory = inventory;
    }

    private void Start()
    {
        // we go through all children to try to grab them
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null || child.gameObject.activeSelf == false) { continue; }
            Item item = child.GetComponent<Item>();
            if (item == null) { continue; }
            if (Grab(item)) { Inventory.GrabAtStart(item); }
        }
    }


    // GRAB / DROP
    public virtual bool Grab(Item item)
    {
        // we check if we can add the item
        if (item == null) { return false; }

        // we check if the item passes the rule
        if (!CanStore(item)) { return false; }

        // we check if we already have a stack for this item reference
        List<ItemStack> empty_stacks = new List<ItemStack>();
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.IsEmpty) { empty_stacks.Add(stack); continue; }
            if (stack.ItemReference != item.Reference) { continue; }

            // we try to add the item to this stack
            bool added = stack.Add(item);
            if (!added) { continue; }

            // we successfully grabbed the item
            finalise_grab(item);
            if (log) { Debug.Log("(ItemPool) " + name + " grabbed : " + item.name + " in existing stack"); }
            return true;
        }

        // we have no existing stack with same reference, we try to put into an empty one
        if (empty_stacks.Count > 0) {
            empty_stacks[0].Add(item);
            // we successfully grabbed the item
            finalise_grab(item);
            if (log) { Debug.Log("(ItemPool) " + name + " grabbed : " + item.name + " in empty stack"); }
            return true;
        }

        // if we are here, we need to create a new stack for this item, we check if we are scalable
        if (!Scalable)
        {
            if (stacks.Count >= MaxStacks) { return false; }
            // if we are here, we are not scalable but we still can add some stacks before the max, so we continue !
        }

        // we create a new stack for this item
        ItemStack new_stack = new ItemStack();
        bool added_to_new_stack = new_stack.Add(item);
        stacks.Add(new_stack);

        // we successfully grabbed the item
        finalise_grab(item);
        if (log) { Debug.Log("(ItemPool) " + name + " grabbed : " + item.name + " in new stack"); }
        return true;
    }
    public bool Drop(Item item)
    {
        if (item == null) { return false; }

        // we look for the stack containing this item
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.IsEmpty) { continue; }
            if (stack.ItemReference != item.Reference) { continue; }

            // we try to remove the item from this stack
            bool removed = stack.Remove(item);
            if (!removed) { continue; }

            // if we are here, we successfully dropped the item
            OnItemDropped.Invoke(item);
            if (log) { Debug.Log("(ItemPool) " + name + " dropped : " + item.name); }
            return true;
        }

        return false;
    }

    // RULE CHECK
    public bool CanStore(Item item)
    {
        // we check if the item is valid
        if (item == null) { return false; }
        bool validate = item.ValidateRule(item_rule);
        if (!validate && log_storage)
        {
            Debug.LogWarning($"(ItemPool) {name} can't store item {item.Reference} because it doesn't match the rule {item_rule}");
        }
        else if (log_storage)
        {
            Debug.Log($"(ItemPool) {name} can store item {item.Reference} because it matches the rule {item_rule}");
        }

        return validate;
    }

    // low level grab drop
    private void finalise_grab(Item item)
    {
        // we check if the item is already grabbed somewhere, if so we drop it
        if (item.Grabbed && item.ItemPoolHolder != null) { item.ItemPoolHolder.Drop(item); }
        // todo can be improved if we make the ItemPoolHolder drop instead of making the InventoryHolder drop

        // we set the item parent and reset its local position
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;

        // we add the item
        // Items.Add(item);
        item.Grabbed = true;

        OnItemGrabbed.Invoke(item);
    }


    // GETTERS
    public T GetItem<T>() where T : Item
    {
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.IsEmpty) { continue; }
            if (stack.Items[0] is not T) { continue; }

            // we return the first item of this stack
            return stack.Items[0] as T;
        }
        return null;
    }
    public List<T> GetItemsByType<T>() where T : Item
    {
        List<T> items = new List<T>();
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.IsEmpty) { continue; }
            if (stack.Items[0] is not T) { continue; }

            // we add all items of this stack
            items.AddRange(stack.Items.Cast<T>());
        }
        return items;
    }
    public List<Item> GetItemsByRule(string rule = "")
    {
        List<Item> items = new List<Item>();
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.IsEmpty) { continue; }

            // we check the first item of the stack to see if it matches the rule
            if (!stack.Items[0].ValidateRule(rule)) { continue; }

            // we add all items of this stack
            items.AddRange(stack.Items);
        }
        return items;
    }
    public bool HasItem(Item item)
    {
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.IsEmpty) { continue; }
            if (stack.ItemReference != item.Reference) { continue; }

            // we check if the item is in this stack
            if (stack.Items.Contains(item)) { return true; }
        }
        return false;
    }
}


[Serializable] public class ItemStack 
{
    public string ItemReference = "";
    public int MaxQty = 1;
    public List<Item> Items = new List<Item>();
    public bool IsFull { get { return Items.Count >= MaxQty; } }
    public bool IsEmpty { get { return Items.Count == 0; } }
    
    // ADD / REMOVE
    public bool Add(Item item)
    {
        if (IsEmpty)
        {
            // we set the max qty & the item ref
            ItemReference = item.Reference;
            MaxQty = item.MaxQty;
            Items.Add(item);
            return true;
        }

        if (item.Reference != ItemReference) { return false; }
        if (IsFull) { return false; }

        Items.Add(item);
        return true;
    }
    public bool Remove(Item item)
    {
        if (IsEmpty) { return false; }
        if (item.Reference != ItemReference) { return false; }

        bool removed = Items.Remove(item);
        if (!removed) { return false; }

        // if we are empty, we reset the stack
        if (IsEmpty)
        {
            ItemReference = "";
            MaxQty = 1;
        }

        return true;
    }
}