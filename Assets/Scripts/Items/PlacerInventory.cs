using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this special inventory places items when grabbed. it NEEDS to have a max placable items
/// so it has a maximum items limit.
/// It then has x local position where to put the items when placed
/// </summary>
public class PlacerInventory : Inventory
{
    [Header("Inventory placer")]
    [SerializeField] List<Vector2> place_positions = new List<Vector2>();
    List<int> occupied_positions = new List<int>();
    [SerializeField] private string item_rule_placing = ""; // item rule for placing items, default is all items are placed
    [SerializeField] private bool authorize_overplace = false; // if true, will still grab the items but won't place them if we don't have position

    // GRAB DROP REMOVE
    public override bool Grab(Item item, List<UI_Inventory> uis_to_ignore = null)
    {
        // we try to get a free position where we will put the item
        Vector2? place_position = occupy_free_position();

        // if we have no free position & we don't ensure authorize overplace
        // then we can't grab at all, we return
        if (place_position == null && !authorize_overplace)
        {
            if (log) { Debug.LogWarning("(PlacerInventory) Can't grab item " + item.name + " because no free place position!"); }
            return false;
        }

        // we try to grab it in the ui
        bool ui_grabbed = base.Grab(item, uis_to_ignore);
        if (!ui_grabbed)
        {
            free_position(place_position);
            return false;
        }

        // we check if we can place the item, if not we free the position & return true bcz we grabbed it !
        if (place_position == null || !item.ValidateRule(item_rule_placing))
        {
            free_position(place_position);
            return true;
        }

        // we place the item
        item.Placed = true;
        item.transform.localPosition = place_position.Value;
        return true;
    }
    public override bool Drop(Item item, List<UI_Inventory> uis_to_ignore = null)
    {
        // we free the position
        Vector2 item_local_pos = item.transform.localPosition;
        free_position(item_local_pos);

        // no need for unplacing the item since it will be grabbed in another inventory
        // (placed item can't move and can't be interacted with so it will necessary be grabbed)

        // we drop the item from the inventory
        return base.Drop(item, uis_to_ignore);
    }
    public override bool Remove(Item item)
    {
        // we free the position
        Vector2 item_local_pos = item.transform.localPosition;
        free_position(item_local_pos);
        return base.Remove(item);
    }

    // PLACING LOW LEVEL
    private Vector2? occupy_free_position()
    {
        for (int i = 0; i < place_positions.Count; i++)
        {
            if (occupied_positions.Contains(i)) { continue; }
            occupied_positions.Add(i);
            return place_positions[i];
        }

        if (log) { Debug.LogWarning("(PlacerInventory) Trying to occupy a position but all are occupied!"); }
        return null;
    }
    private void free_position(Vector2? position)
    {
        if (position == null) { return; }

        for (int i = 0; i < place_positions.Count; i++)
        {
            if (place_positions[i] == position)
            {
                occupied_positions.Remove(i);
                return;
            }
        }

        if (log) { Debug.LogWarning("(PlacerInventory) Trying to free a position that was not occupied!"); }
    }
}