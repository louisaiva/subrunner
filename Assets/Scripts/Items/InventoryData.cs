using System;
using System.Collections.Generic;

[Serializable] public class InventoryData
{
    public List<ItemPoolData> item_pools_data;
    public ItemType item_type;
    public int ItemsCount()
    {
        int count = 0;
        for (int i=0; i<item_pools_data.Count; i++)
        {
            for (int j=0; j<item_pools_data[i].stacks_data.Count; j++)
            {
                count += item_pools_data[i].stacks_data[j].items_ids.Count;
            }
        }
        return count;
    }
    public bool AddItem(ItemData item_data)
    {
        if (item_data == null) { return false; }
        string item_id = item_data.id;
        for (int i=0; i<item_pools_data.Count; i++)
        {
            ItemPoolData data = item_pools_data[i];
            if (ItemPool.AddItemToPoolData(ref data, item_id, item_data.reference))
            {
                return true;
            }
        }
        return false;
    }

    public List<string> GetAllItemsIds()
    {
        List<string> items_ids = new List<string>();
        for (int i=0; i<item_pools_data.Count; i++)
        {
            for (int j=0; j<item_pools_data[i].stacks_data.Count; j++)
            {
                items_ids.AddRange(item_pools_data[i].stacks_data[j].items_ids);
            }
        }
        return items_ids;
    }

    // DUPLICATE
    public InventoryData Duplicate()
    {
        InventoryData new_data = new InventoryData
        {
            item_pools_data = new List<ItemPoolData>(),
            item_type = item_type
        };
        for (int i=0; i<item_pools_data.Count; i++)
        {
            new_data.item_pools_data.Add(item_pools_data[i].Duplicate());
        }
        return new_data;
    }

    // GET DETAILS
    public string GetDetails()
    {
        string details = $"inventory : {item_pools_data.Count} item pools\n";
        details += $"    - item type : {item_type}\n";
        for (int i=0; i<item_pools_data.Count; i++)
        {
            details += item_pools_data[i].GetDetails();
        }
        return details;
    }
}

[Serializable] public class ItemPoolData
{
    public ItemType item_type;
    public List<ItemStackData> stacks_data;
    public string pool_id;
    public int max_stacks;
    public int min_stacks;
    public bool scalable;
    public string item_rule;

    // DUPLICATE
    public ItemPoolData Duplicate()
    {
        ItemPoolData new_data = new ItemPoolData
        {
            pool_id = pool_id,
            item_type = item_type,
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

    // DETAILS
    public string GetDetails()
    {
        // count all items
        string details = "    - item pool : " + pool_id + $" ({stacks_data.Count} stacks for item rule '{item_rule}')\n";
        for (int i = 0; i < stacks_data.Count; i++)
        {
            string item_ref = "";
            if (stacks_data[i] != null && stacks_data[i].items_ids.Count > 0)
            {
                item_ref = stacks_data[i].items_ids[0];
            }
            details += $"        - {stacks_data[i].items_ids.Count} {item_ref}\n";
        }
        return details;
    }

}

[Serializable] public class ItemStackData
{
    public string item_ref;
    public List<string> items_ids;
    
    public ItemStackData Duplicate()
    {
        return new ItemStackData
        {
            item_ref = item_ref,
            items_ids = new List<string>(items_ids)
        };
    }
}
