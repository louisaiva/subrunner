using System;
using System.Collections.Generic;
using UnityEngine;

public class TemplateInventoryReference : MonoBehaviour
{
    public string pool_id = "chest"; // ! must be exact to load the ui, ie for chest
    public ItemType item_type = ItemType.Physical; // the type of items in the inventory (physical, usable, etc)
    public int capacity = 12;
    public bool scalable = false;
    public string item_rule = "";
    public List<TemplateItemReference> items = new List<TemplateItemReference>();

    public InventoryData GetInventoryData()
    {
        InventoryData data = new InventoryData
        {
            item_pools_data = new List<ItemPoolData>(),
            item_type = this.item_type
        };
        ItemPoolData pool_data = new ItemPoolData
        {
            stacks_data = new List<ItemStackData>(),
            item_type = this.item_type,
            pool_id = "stuff",
            min_stacks = this.capacity,
            max_stacks = this.capacity,
            scalable = this.scalable,
            item_rule = this.item_rule
        };
        data.item_pools_data.Add(pool_data);
        for (int i=0; i<items.Count; i++)
        {
            TemplateItemReference item_ref = items[i];
            if (item_ref == null || string.IsNullOrEmpty(item_ref.item_id)) { continue; }
            ItemStackData stack_data = new ItemStackData
            {
                item_ref = item_ref.item_id,
                items_ids = new List<string>()
            };
            for (int j=0; j<item_ref.quantity; j++)
            {
                stack_data.items_ids.Add(item_ref.item_id);
            }
            pool_data.stacks_data.Add(stack_data);
        }
        return data;
    }

}

[Serializable] public class TemplateItemReference
{
    public string item_id = "";
    public int quantity = 1;
}