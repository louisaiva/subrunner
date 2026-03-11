using System;
using System.Collections.Generic;

[Serializable] public class InventoryData
{
    public List<ItemPoolData> item_pools_data;

    // DUPLICATE & DETAILS
    public InventoryData Duplicate()
    {
        InventoryData new_data = new InventoryData
        {
            item_pools_data = new List<ItemPoolData>()
        };
        for (int i=0; i<item_pools_data.Count; i++)
        {
            new_data.item_pools_data.Add(item_pools_data[i].Duplicate());
        }
        return new_data;
    }
    public string GetDetails()
    {
        string details = $"inventory : {item_pools_data.Count} item pools :\n";
        for (int i=0; i<item_pools_data.Count; i++)
        {
            details += item_pools_data[i].GetDetails();
        }
        return details;
    }
}

[Serializable] public class ItemPoolData
{
    public List<ItemStackData> stacks_data;
    public string pool_id;
    public int max_stacks;
    public int min_stacks;
    public bool scalable;
    public string item_rule;

    
    // DETAILS
    public string GetDetails()
    {
        // count all items
        string details = "";
        for (int i=0; i<stacks_data.Count; i++)
        {
            string item_ref = "";
            if (stacks_data[i] != null && stacks_data[i].items_ids.Count > 0)
            {
                item_ref = stacks_data[i].items_ids[0];
            }
            details += $"      - {stacks_data[i].items_ids.Count} {item_ref}\n";
        }
        return details;
    }

    public ItemPoolData Duplicate()
    {
        ItemPoolData new_data = new ItemPoolData
        {
            pool_id = pool_id,
            max_stacks = max_stacks,
            min_stacks = min_stacks,
            scalable = scalable,
            item_rule = item_rule,
            stacks_data = new List<ItemStackData>()
        };
        for (int i=0; i<stacks_data.Count; i++)
        {
            new_data.stacks_data.Add(stacks_data[i].Duplicate());
        }
        return new_data;
    }
}

[Serializable] public class ItemStackData
{
    public List<string> items_ids;
    
    public ItemStackData Duplicate()
    {
        return new ItemStackData
        {
            items_ids = new List<string>(items_ids)
        };
    }
}
