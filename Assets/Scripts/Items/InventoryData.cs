using System;
using System.Collections.Generic;

[Serializable] public class InventoryData
{
    public List<ItemPoolData> item_pools_data;

    // DETAILS
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
            string item_ref = (stacks_data[i].items_ids[0] ?? "");
            details += $"      - {stacks_data[i].items_ids.Count} {item_ref}\n";
        }
        return details;
    }
}

[Serializable] public class ItemStackData
{
    public List<string> items_ids;
}
