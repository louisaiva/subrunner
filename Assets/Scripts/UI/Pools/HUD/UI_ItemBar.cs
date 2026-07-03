using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemBar : MonoBehaviour
{

    [Header("Prefabs")]
    [SerializeField] private UI_ItemStack conso_prefab;
    [SerializeField] private UI_ItemStack shoes_prefab;
    [SerializeField] private UI_ItemStack weapon_prefab;
    [SerializeField] private UI_ItemStack device_prefab;
    [SerializeField] private UI_ItemStack file_prefab;
    private List<UI_ItemStack> item_stacks = new List<UI_ItemStack>();

    [Header("Default color")]
    [SerializeField] private Color default_color;

    [Header("Callbacks")]
    private Dictionary<(ItemPool,ActionSwitcher), Action<Item>> callbacks = new Dictionary<(ItemPool,ActionSwitcher), Action<Item>>();
    private Dictionary<ItemPool,Graphic> notches = new Dictionary<ItemPool, Graphic>();


    // ATTACH TO INVENTORY
    public void AttachToInventory(Inventory inventory)
    {
        if (inventory == null) { return; }
        
        // we get all the item pools
        List<ItemPool> pools = inventory.GetItemPools();

        // we only care about the pools that have 1 max stack, and that are not empty
        List<ItemPool> relevant_pools = new List<ItemPool>();
        List<int> pool_indices = new List<int>();
        foreach (ItemPool pool in pools)
        {
            if (!is_relevant_pool(pool, out int index)) { continue; }
            relevant_pools.Add(pool);
            pool_indices.Add(index);
        }

        // we sort the relevant pools by their ID, based on the order of the relevant_pool_ids list
        relevant_pools = sort_pools_by_id(relevant_pools, pool_indices);

        // finally we create their UI_ItemStack
        foreach (ItemPool pool in relevant_pools) { create_ui_item_stack(pool); }
    }
    private List<string> relevant_pool_ids = new List<string>() { "conso", "weapon", "shoes", "device", "files" };
    private bool is_relevant_pool(ItemPool pool, out int index)
    {
        index = -1;
        if (pool.MaxStacks != 1) { return false; }
        if (pool.StackCount == 0) { return false; }
        
        if (relevant_pool_ids.Contains(pool.PoolID))
        {
            index = relevant_pool_ids.IndexOf(pool.PoolID);
            return true;
        }
        foreach (string id in relevant_pool_ids)
        {
            if (pool.PoolID.Contains(id)) // conso_1, conso_2, etc.
            {
                index = relevant_pool_ids.IndexOf(id);
                return true;
            }
        }
        return false;
    }
    private List<ItemPool> sort_pools_by_id(List<ItemPool> pools, List<int> indices)
    {
        List<ItemPool> sorted_pools = new List<ItemPool>(pools);
        
        for (int i = 0; i < indices.Count - 1; i++)
        {
            for (int j = 0; j < indices.Count - i - 1; j++)
            {
                if (indices[j] > indices[j + 1])
                {
                    // swap pools
                    ItemPool temp_pool = sorted_pools[j];
                    sorted_pools[j] = sorted_pools[j + 1];
                    sorted_pools[j + 1] = temp_pool;

                    // swap indices
                    int temp_index = indices[j];
                    indices[j] = indices[j + 1];
                    indices[j + 1] = temp_index;
                }
                else if (indices[j] == indices[j + 1]) // if they have the same index, we sort them by their ID
                {
                    if (string.Compare(sorted_pools[j].PoolID, sorted_pools[j + 1].PoolID) > 0)
                    {
                        // swap pools
                        ItemPool temp_pool = sorted_pools[j];
                        sorted_pools[j] = sorted_pools[j + 1];
                        sorted_pools[j + 1] = temp_pool;

                        // swap indices
                        int temp_index = indices[j];
                        indices[j] = indices[j + 1];
                        indices[j + 1] = temp_index;
                    }
                }
            }
        }

        return sorted_pools;
    }


    // CREATE UI ITEM STACK
    private void create_ui_item_stack(ItemPool pool)
    {
        UI_ItemStack prefab = null;
        switch (pool.PoolID)
        {
            case "shoes": prefab = shoes_prefab; break;
            case "weapon": prefab = weapon_prefab; break;
            case "device": prefab = device_prefab; break;
            default: prefab = conso_prefab; break;
        }
        if (prefab == null) { return; }

        UI_ItemStack new_stack = Instantiate(prefab, transform);
        
        // get the stack
        ItemStack stack = pool.Stacks[0];
        new_stack.Init(stack);
        item_stacks.Add(new_stack);

        // get item, switcher & notch
        Item item = stack.Item;
        ActionSwitcher switcher = new_stack.GetComponentInChildren<ActionSwitcher>(includeInactive:true);
        Graphic notch = new_stack.transform.Find("notch").GetComponent<Graphic>();
        if (notch != null) { notches[pool] = notch; }

        // register callback
        Action<Item> callback = assign_callback(pool,switcher);
        if (item == null || switcher == null) { return; }
        callback.Invoke(item); // we fire the callback, which will switch the action, & set the colors
    }


    // CALLBACKS
    private Action<Item> assign_callback(ItemPool pool, ActionSwitcher switcher)
    {
        if (pool == null || switcher == null) { return null; }
        Action<Item> callback = (item) => handle_item_received(item, pool, switcher);
        callbacks[(pool,switcher)] = callback;
        pool.OnItemGrabbed += callback;
        return callback;
    }
    private void handle_item_received(Item item, ItemPool pool, ActionSwitcher switcher)
    {
        if (pool == null || switcher == null) { return; }
        Color color = item != null ? item.Color : default_color;
        switcher.SwitchAction(InputManager.Instance.GetActionFromItemPool(pool), color);
        if (notches.ContainsKey(pool)) { notches[pool].color = color; }
    }
    private void clear_callbacks()
    {
        foreach (var kvp in callbacks)
        {
            ItemPool pool = kvp.Key.Item1;
            if (pool == null) { continue; }
            pool.OnItemGrabbed -= kvp.Value;
        }
        callbacks.Clear();
    }


    // UPDATE
    private void Update()
    {
        foreach (UI_ItemStack stack in item_stacks)
        {
            if (stack == null) { continue; }
            if (stack.Stack == null) { continue; }
            
            // we toggle the gameObject based on the stack's quantity
            stack.gameObject.SetActive(stack.Stack.Quantity > 0);
        }
    }

    // CLEAR
    public void Clear()
    {
        clear_callbacks();
        notches.Clear();
        clear_item_stacks();
    }
    private void clear_item_stacks()
    {
        foreach (UI_ItemStack stack in item_stacks)
        {
            if (stack == null) { continue; }
            stack.UnregisterCallbacks();
            Destroy(stack.gameObject);
        }
        item_stacks.Clear();
    }
}