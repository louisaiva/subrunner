using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


public class ItemPool : MonoBehaviour, ItemStorer
{
    [Header("Pool ID")]
    [field:SerializeField] public string PoolID {get; set;} = "stuff";

    [Header("Items")]
    [field: SerializeField] public ItemType ItemType { get; set; }
    public List<ItemStack> stacks = new List<ItemStack>();
    public List<ItemStack> Stacks { get { return stacks; } }
    public List<Item> Items { get { return stacks.SelectMany(s => s.Items).ToList(); } }
    // public string ItemRule { get { return item_rule; } }
    public virtual int Count { get { return Items.Count; } }
    public virtual int StackCount { get { return stacks.Count; } }
    public virtual bool HasSpaceLeft { get { return Scalable || stacks.Count < MaxStacks || stacks.Any(s => !s.IsFull); } }


    [Header("Item Stacks Parameters")]
    public int MaxStacks = 9; // the maximum number of stacks in the pool
    public int MinStacks = 0;
    public bool Scalable = false; // if true, the pool will dynamically add/remove stacks

    // EVENTS
    public event Action<Item> OnItemGrabbed = delegate { };
    public event Action<Item> OnItemDropped = delegate { };
    public event Action<ItemStack> OnStackCreated = delegate { };
    public event Action<ItemStack> OnStackRemoved = delegate { };



    [Header("Item Rule")]
    public string item_rule = ""; // the rule to check if the item is valid

    [Header("Inventory & Capable")]
    public Inventory Inventory;
    public Capable Capable { get { return Inventory?.Capable; } }


    [Header("Logs")]
    [SerializeField] protected bool log_start_grabbing = false;
    [SerializeField] protected bool log_loading = false;
    [SerializeField] protected bool log_grab = false;
    [SerializeField] protected bool log_merge = false;
    [SerializeField] protected bool log_stacks = false;



    // ATTACH & START
    public void AttachToInventory(Inventory inventory)
    {
        this.Inventory = inventory;
    }
    /* private void Start()
    {
        // if we are an insider we don't even start
        if (!CapableEngine.Instance.IsOutsider(Capable?.ID))
        {
            create_enough_stacks();
            return;
        }
        
        // else we try to grab the items we already have in inventory
        bool old_log_grab = log_grab;
        if (log_start_grabbing)
        {
            Debug.Log($"(ItemPool - {PoolID} - {name} - {Capable?.ID}) is starting grabbing items from its children. item rule is {item_rule}");
            log_grab = true;
        }

        // we go through all children to try to grab them
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null || child.gameObject.activeSelf == false) { continue; }
            Item item = child.GetComponent<Item>();
            if (item == null) { continue; }
            if (Grab(item)) { Inventory.GrabFromLowerLevel(item); }
        }
        create_enough_stacks();

        log_grab = old_log_grab;
    } */
    private void ensure_at_least_min_stacks_exist()
    {
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




    // LOAD DATA
    public void LoadPoolData(ItemPoolData data)
    {
        // we load base data
        this.PoolID = data.pool_id;
        this.MaxStacks = data.max_stacks;
        this.MinStacks = data.min_stacks;
        this.Scalable = data.scalable;
        this.item_rule = data.item_rule;
        this.ItemType = data.item_type;

        // we want to load the items inside the stacks
        for (int i = 0; i < data.stacks_data.Count; i++)
        {
            ItemStackData stack_data = data.stacks_data[i];
            ItemStack new_stack = new ItemStack(this);
            stacks.Add(new_stack);

            for (int j = 0; j < stack_data.items_ids.Count; j++)
            {
                string item_id = stack_data.items_ids[j];
                Item item = CapableEngine.Instance.LoadCapableInstantly(item_id) as Item;
                if (item == null)
                {
                    Debug.LogError($"(ItemPool) Failed to load item with id {item_id} for pool {name}");
                    continue;
                }
                
                // verify that the item matches the rule
                if (!ValidateRule(item))
                {
                    /* if (log_loading) {  */Debug.LogWarning($"(ItemPool) Loaded item {item.ID} doesn't match the rule of pool {name}, dropping it"); //}
                    item.BeDropped(this.Inventory?.Capable);
                    continue;
                }

                finalise_grab(item, new_stack);
            }

            OnStackCreated?.Invoke(new_stack);
        }
        ensure_at_least_min_stacks_exist();

        if (log_loading) { Debug.Log($"(ItemPool) Loaded pool data for pool {name} ({Capable?.ID}) : \n  -{data.GetDetails()}"); }
    }
    public void UnloadPoolData()
    {
        // we unload items
        List<string> item_ids = Items.Select(i => i.data.id).ToList();
        CapableEngine.Instance.UnloadCapables(item_ids);

        // clears stacks
        stacks.Clear();

        // clearing delegates
        foreach (Delegate d in OnStackCreated.GetInvocationList())
        {
            OnStackCreated -= (Action<ItemStack>)d;
        }
        foreach (Delegate d in OnStackRemoved.GetInvocationList())
        {
            OnStackRemoved -= (Action<ItemStack>)d;
        }
    }
    string ItemStorer.GetDetails()
    {
        string log = "ItemPool '" + name + "' (PoolID : " + PoolID +") of capable '" + (Capable?.ID ?? "null") + "'";
        log += $"\n  -- Min/Max Stacks : {MinStacks}/{MaxStacks}";
        log += $"\n  -- Scalable : {Scalable}";
        log += $"\n  -- Stacks : {stacks.Count}";
        log += $"\n  -- ItemType : {ItemType}";
        log += $"\n  -- Rule : {item_rule}";
        return log;
    }

    // GET STATIC DATA
    public ItemPoolData GetStaticPoolData()
    {
        ItemPoolData data = new ItemPoolData()
        {
            pool_id = this.PoolID,
            max_stacks = this.MaxStacks,
            min_stacks = this.MinStacks,
            scalable = this.Scalable,
            item_rule = this.item_rule,
            stacks_data = new List<ItemStackData>(),
            item_type = this.ItemType
        };

        // we go through all children to try to grab them
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null || child.gameObject.activeSelf == false) { continue; }
            Item item = child.GetComponent<Item>();
            if (item == null) { continue; }
            AddItemToPoolData(ref data, item.ID, item.Reference);
        }

        return data;
    }
    public static bool AddItemToPoolData(ref ItemPoolData data, string item_id, string item_ref)
    {
        // we check if we already have a item_stack_data with the same ref
        for (int j = 0; j < data.stacks_data.Count; j++)
        {
            if (data.stacks_data[j].item_ref == item_ref)
            {
                data.stacks_data[j].items_ids.Add(item_id);
                return true;
            }
        }

        // if we are here, we have no stack with the same ref, we create a new one and we put the item id in it
        data.stacks_data.Add(new ItemStackData() { item_ref = item_ref, items_ids = new List<string>() { item_id } });
        return true; // for now we don't check special edges cases like not the right ref, etc etc so always true
    }
    public List<Item> GetStaticItems()
    {
        List<Item> items = new List<Item>();

        // we go through all children to try to grab them
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null || child.gameObject.activeSelf == false) { continue; }
            Item item = child.GetComponent<Item>();
            if (item == null) { continue; }
            items.Add(item);
        }

        return items;
    }

    // GET DYNAMIC DATA
    /// <summary>
    /// this method is the opposite of GetStaticPoolData()
    /// since it only works at runtime, when the ItemPool has some stacks in
    /// their memory. So it is easier to update because we just need
    /// to update the items' id that we currently have in each ui_itemstack :)
    /// </summary>
    /// <returns></returns>
    public ItemPoolData GetDynamicPoolData()
    {
        ItemPoolData data = new ItemPoolData()
        {
            pool_id = this.PoolID,
            max_stacks = this.MaxStacks,
            min_stacks = this.MinStacks,
            scalable = this.Scalable,
            item_rule = this.item_rule,
            stacks_data = new List<ItemStackData>()
        };

        // we go through all stacks to get their data
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            ItemStackData stack_data = new ItemStackData() { items_ids = stack.Items.Select(item => item.data.id).ToList() };
            data.stacks_data.Add(stack_data);
        }

        return data;
    }


    // RULE CHECK
    public bool ValidateRule(Item item) { return item.ValidateRule(item_rule); }

    // GRAB / DROP
    public virtual bool Grab(Item item)
    {
        // we check if we can add the item
        if (item == null)
        {
            if (log_grab) { Debug.LogWarning("(ItemPool) " + name + " can't grab null item"); }
            return false;
        }

        // we check if the item passes the rule
        if (!ValidateRule(item))
        {
            if (log_grab) { Debug.LogWarning("(ItemPool) " + name + " can't grab : " + item.name + " because it doesn't match the rule"); }
            return false;
        }

        // we check if we already have a stack for this item reference
        List<ItemStack> empty_stacks = new List<ItemStack>();
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.IsEmpty) { empty_stacks.Add(stack); continue; }
            if (stack.ItemReference != item.Reference) { continue; }

            // we try to add the item to this stack
            bool can_add = stack.CanAdd(item);
            if (!can_add) { continue; }

            // we successfully grabbed the item
            finalise_grab(item, stack);
            
            if (log_grab) { Debug.Log("(ItemPool) " + name + " grabbed : " + item.name + " in existing stack"); }
            return true;
        }

        // we have no existing stack with same reference, we try to put into an empty one
        if (empty_stacks.Count > 0) {

            // no need for checking the can add, the stack is empty

            // we successfully grabbed the item
            finalise_grab(item, empty_stacks[0]);
            // OnStackUpdated?.Invoke(empty_stacks[0]);
            if (log_grab) { Debug.Log("(ItemPool) " + name + " grabbed : " + item.name + " in empty stack"); }
            return true;
        }

        // if we are here, we need to create a new stack for this item, we check if we are scalable
        if (!Scalable)
        {
            if (stacks.Count >= MaxStacks)
            {
                if (log_grab) { Debug.LogWarning("(ItemPool) " + name + " can't grab : " + item.name + " because it has reached the max stacks and is not scalable"); }
                return false;
            }
            // if we are here, we are not scalable but we still can add some stacks before the max, so we continue !
        }

        // we create a new stack for this item
        ItemStack new_stack = new ItemStack(this);
        stacks.Add(new_stack);

        // we successfully grabbed the item
        finalise_grab(item, new_stack);
        OnStackCreated?.Invoke(new_stack);
        if (log_grab) { Debug.Log("(ItemPool) " + name + " grabbed : " + item.name + " in new stack"); }
        return true;
    }
    public bool Drop(Item item,bool on_ground = true)
    {
        if (item == null) { return false; }

        // we look for the stack containing this item
        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            if (stack.IsEmpty) { continue; }
            if (!stack.Items.Contains(item)) { continue; }
            // dont uncomment if (stack.ItemReference != item.Reference) { continue; } - we don't want to check item ref since when ref changed we need to drop it

            // we remove the item from this stack
            stack.Remove(item);


            // if we are here, we successfully dropped the item
            item.OnReferenceChanged -= handle_item_reference_changed;
            if (on_ground) { item.BeDropped(this.Inventory?.Capable); } // we set the item to dropped (which loads the hover capacity)

            // we trigger the event
            OnItemDropped?.Invoke(item);
            Inventory?.DropFromLowerLevel(item); // we try to drop it from the inventory if it's in, this will update the UI and drop it in the world if it's not in the inventory anymore

            // finally we deal with the stack
            if (Scalable && stack.IsEmpty && stacks.Count > MinStacks)
            {
                stacks.Remove(stack);
                OnStackRemoved?.Invoke(stack);
            }

            if (log_grab) { Debug.Log("(ItemPool) " + name + " dropped : " + item.name + $"{(on_ground ? " on ground" : " in inventory")}"); }
            return true;
        }

        return false;
    }
    public bool GrabInStack(Item item, ItemStack stack)
    {
        if (item == null || stack == null) { return false; }
        if (!stacks.Contains(stack)) { return false; }
        if (!ValidateRule(item)) { return false; }

        // we try to add the item to this stack
        bool can_add = stack.CanAdd(item);
        if (!can_add) { return false; }

        // we successfully grabbed the item
        finalise_grab(item, stack);
        if (log_grab) { Debug.Log("(ItemPool) " + name + " grabbed : " + item.name + $" in specific stack {stacks.IndexOf(stack)}"); }
        return true;
    }

    // low level grab drop
    private void finalise_grab(Item item, ItemStack stack)
    {
        if (log_grab) { Debug.Log($"(ItemPool) finalising grab of item {item.name} into stack {stacks.IndexOf(stack)}. ItemPoolHolder is {(item.ItemPoolHolder as ItemStorer)?.GetDetails() ?? "null"}"); }

        // we check if the item is already grabbed somewhere, if so we drop it
        if (item.Grabbed && item.ItemPoolHolder != null) { item.ItemPoolHolder.Drop(item, on_ground: false); }

        // we finally grab it into the stack
        stack.Add(item);

        // we set the item parent and reset its local position
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;

        // we register the item callback to when it changes references
        // (will either update the stack' either split it either drop it AND THEN automatically update the UI)
        item.OnReferenceChanged += handle_item_reference_changed;

        // we add the item
        item.BeGrabbed(this.Inventory?.Capable);

        // we trigger the event
        OnItemGrabbed?.Invoke(item);
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
        ItemStack new_stack = new ItemStack(this);
        stacks.Add(new_stack);
        OnStackCreated?.Invoke(new_stack);
        if (log_stacks) { Debug.Log($"(ItemPool) Added empty stack to pool {name}, total stacks : {stacks.Count}"); }
    }
    public void DestroyEmptyStacks()
    {
        // we remove all empty stacks we can find in the pool
        // from the last one to the first
        // we stop only if we are at MinStacks

        int removed_count = 0;
        while (stacks.Count > MinStacks)
        {
            ItemStack stack = stacks.LastOrDefault(s => s.IsEmpty);
            if (stack == null) { break; }
            stacks.Remove(stack);
            OnStackRemoved?.Invoke(stack);
            removed_count++;
        }

        if (log_stacks) { Debug.Log($"(ItemPool) Removed {removed_count} empty stacks from pool {name}, total stacks : {stacks.Count}"); }
    }


    public bool HasStack(ItemStack stack)
    {
        return stacks.Contains(stack);
    }
    public void MergeIntoStack(ItemStack from_stack, ItemStack to_stack)
    {
        if (from_stack == null || to_stack == null) { return; }
        if (!HasStack(to_stack)) { return; } // must hold the to_stack

        // we add all items from from_stack into to_stack until we can't anymore
        while (from_stack.Items.Count > 0)
        {
            Item item = from_stack.Items[0];
            bool added = GrabInStack(item, to_stack);
            if (log_merge) { Debug.Log($"(ItemPool) Merged item {item} into stack {stacks.IndexOf(to_stack)}"); }
            if (!added) { break; }
            from_stack.Remove(item);
            // if (log_merge) { Debug.Log($"(ItemPool) Removed item {item} from stack {from_stack.Pool} - {from_stack.Pool.stacks.IndexOf(from_stack)}"); }
        }
    }
    public void SwapStacks(ItemStack stack1, ItemStack stack2)
    {
        if (stack1 == null || stack2 == null) { return; }
        if (!stacks.Contains(stack1) && !stacks.Contains(stack2)) { return; } // must at least hold one of those two stacks

        // we store the items
        List<Item> items1 = new List<Item>(stack1.Items);
        List<Item> items2 = new List<Item>(stack2.Items);

        // we clear the stacks
        stack1.Clear();
        stack2.Clear();

        // we get the ItemPools for those stacks
        ItemStorer pool1 = stack1.Storer;
        ItemStorer pool2 = stack2.Storer;

        // we add the items to the opposite stacks
        for (int i = 0; i < items1.Count; i++)
        {
            Item item = items1[i];
            bool added = pool2.GrabInStack(item, stack2);
            if (!added) { Debug.LogError($"(ItemPool - swap) Failed to add {item} to stack {stack2} in ItemPool {pool2}"); break; }
        }
        for (int i = 0; i < items2.Count; i++)
        {
            Item item = items2[i];
            bool added = pool1.GrabInStack(item, stack1);
            if (!added) { Debug.LogError($"(ItemPool - swap) Failed to add {item} to stack {stack1} in ItemPool {pool1}"); break; }
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

