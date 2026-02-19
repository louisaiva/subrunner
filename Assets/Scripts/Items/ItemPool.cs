using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public interface ItemStorer
{
    public List<Item> Items { get; }
    public string ItemRule { get; }
    public int Count { get; }
    // public event Action<Item> OnItemGrabbed;
    // public event Action<Item> OnItemDropped;
    public bool Grab(Item item);
    public bool Drop(Item item);
}

public class ItemPool : MonoBehaviour, ItemStorer
{
    [Header("Pool ID")]
    public string PoolID = "stuff";

    [Header("Items")]
    public List<ItemStack> stacks = new List<ItemStack>();
    public List<Item> Items { get { return stacks.SelectMany(s => s.Items).ToList(); } }
    public string ItemRule { get { return item_rule; } }
    public virtual int Count { get { return Items.Count; } }
    public virtual bool HasSpaceLeft { get { return Scalable || stacks.Count < MaxStacks || stacks.Any(s => !s.IsFull); } }

    [Header("Item Stacks Parameters")]
    public int MaxStacks = 9; // the maximum number of stacks in the pool
    public int MinStacks = 0;
    public bool Scalable = false; // if true, the pool will dynamically add/remove stacks


    // [Header("Events")]
    // public event Action<Item> OnItemGrabbed = delegate { };
    // public event Action<Item> OnItemDropped = delegate { };
    // public event Action<Item> OnStacksChanged = delegate { };
    // public event Action<ItemStack> OnStackUpdated = delegate { };
    public event Action<ItemStack> OnStackCreated = delegate { };
    public event Action<ItemStack> OnStackRemoved = delegate { };



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
            if (Grab(item)) { Inventory.GrabFromLowerLevel(item); }
        }

        // we ensure we have at least MinStacks stacks (for the ui to be great)
        if (stacks.Count < MinStacks)
        {
            int stacks_to_add = MinStacks - stacks.Count;
            for (int i = 0; i < stacks_to_add; i++)
            {
                ItemStack new_stack = new ItemStack(this);
                stacks.Add(new_stack);
                OnStackCreated?.Invoke(new_stack);
            }
        }
    }


    // RULE CHECK
    /* public bool CanStore(Item item)
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
    } */
    public bool ValidateRule(Item item) { return item.ValidateRule(item_rule); }

    // GRAB / DROP
    public virtual bool Grab(Item item)
    {
        // we check if we can add the item
        if (item == null) { return false; }

        // we check if the item passes the rule
        if (!ValidateRule(item)) { return false; }

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
            finalise_grab(item, stack);
            
            if (log) { Debug.Log("(ItemPool) " + name + " grabbed : " + item.name + " in existing stack"); }
            return true;
        }

        // we have no existing stack with same reference, we try to put into an empty one
        if (empty_stacks.Count > 0) {
            empty_stacks[0].Add(item);
            // we successfully grabbed the item
            finalise_grab(item, empty_stacks[0]);
            // OnStackUpdated?.Invoke(empty_stacks[0]);
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
        ItemStack new_stack = new ItemStack(this);
        new_stack.Add(item);
        stacks.Add(new_stack);

        // we successfully grabbed the item
        finalise_grab(item, new_stack);
        OnStackCreated?.Invoke(new_stack);
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
            // dont uncomment if (stack.ItemReference != item.Reference) { continue; } - we don't want to check item ref since when ref changed we need to drop it
            if (!stack.Items.Contains(item)) { continue; }

            // we try to remove the item from this stack
            bool removed = stack.Remove(item);
            if (!removed) { continue; }


            // if we are here, we successfully dropped the item
            item.OnReferenceChanged -= handle_item_reference_changed;
            item.Grabbed = false; // we set the item to dropped (which enables the hover collider)
            // OnItemDropped.Invoke(item);

            // finally we deal with the stack
            if (Scalable && stack.IsEmpty && stacks.Count > MinStacks)
            {
                stacks.Remove(stack);
                OnStackRemoved?.Invoke(stack);
            }
            // else { OnStackUpdated?.Invoke(stack); }

            if (log) { Debug.Log("(ItemPool) " + name + " dropped : " + item.name); }
            return true;
        }

        return false;
    }

    // low level grab drop
    private void finalise_grab(Item item,ItemStack stack)
    {
        // we check if the item is already grabbed somewhere, if so we drop it
        if (item.Grabbed && item.ItemPoolHolder != null) { item.ItemPoolHolder.Drop(item); }

        // we set the item parent and reset its local position
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;

        // we register the item callback to when it changes references
        // (will either update the stack' either split it either drop it AND THEN automatically update the UI)
        item.OnReferenceChanged += handle_item_reference_changed;

        // we add the item
        item.Grabbed = true;
    }
    private void handle_item_reference_changed(Item item)
    {
        // get the stack of the item
        ItemStack stack = GetStackOfItem(item);
        if (stack == null) { return; }

        // 4 cases :

        // 1 - item is alone in their stack
        if (stack.Items.Count == 1)
        {
            // nothing to do bcz the reference auto updates in ItemStack
            // todo maybe check that UI updates itself since no event was fired
            return;
        }

        // 2 - item is not alone and we can grab it in this Pool
        if (Grab(item)) { return; } // this is successful and calls ui events so perfect

        // 3 - item is not alone, we can't grab it, so we try to grab it in the inventory
        if (Inventory != null && Inventory.Grab(item)) { return; }

        // 4 - we drop it
        Inventory?.Drop(item); // we try to drop it from the inventory if it's in, this will update the UI and drop it in the world if it's not in the inventory anymore
    }


    // STACK MANAGEMENT
    public void AddEmptyStack()
    {
        if (!Scalable) { return; }
        if (stacks.Count >= MaxStacks) { return; }
        ItemStack new_stack = new ItemStack(this);
        stacks.Add(new_stack);
        OnStackCreated?.Invoke(new_stack);
    }
    public void DestroyEmptyStacks()
    {
        // we remove all empty stacks we can find in the pool
        // from the last one to the first
        // we stop only if we are at MinStacks
        while (stacks.Count > MinStacks)
        {
            ItemStack stack = stacks.LastOrDefault(s => s.IsEmpty);
            if (stack == null) { break; }
            stacks.Remove(stack);
            OnStackRemoved?.Invoke(stack);
        }
    }



    // todo rework all this, we don't want to directly swap stacks between itempools, we should rather swap item per item
    public bool HasStack(ItemStack stack)
    {
        return stacks.Contains(stack);
    }
    public void AddStack(ItemStack stack)
    {
        if (stack == null) { return; }
        if (!Scalable && stacks.Count >= MaxStacks) { return; }
        stacks.Add(stack);
        OnStackCreated?.Invoke(stack);

        // we make sure the inventory registered the grab
        for (int i = 0; i < stack.Items.Count; i++)
        {
            Item item = stack.Items[i];
            Inventory.GrabFromLowerLevel(item);
        }
    }
    public void RemoveStack(ItemStack stack)
    {
        if (stack == null) { return; }
        if (!stacks.Contains(stack)) { return; }
        stacks.Remove(stack);
        OnStackRemoved?.Invoke(stack);
    }
    public void MergeStackIntoStack(ItemStack from_stack, ItemStack to_stack)
    {
        if (from_stack == null || to_stack == null) { return; }
        if (!stacks.Contains(to_stack)) { return; }

        // we add all items from from_stack into to_stack until we can't anymore
        while (from_stack.Items.Count > 0)
        {
            Item item = from_stack.Items[0];
            bool added = to_stack.Add(item);
            if (!added) { break; }
            from_stack.Remove(item);
            Inventory.GrabFromLowerLevel(item);
        }
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
    public ItemStack GetStackOfItem(Item item)
    {
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.IsEmpty) { continue; }

            // -> we don't want to check reference since maybe the reference just changed so sometimes it's not the same
            // dont uncomment lol -> if (stack.ItemReference != item.Reference) { continue; }

            // we check if the item is in this stack
            if (stack.Items.Contains(item)) { return stack; }
        }
        return null;
    }
}

