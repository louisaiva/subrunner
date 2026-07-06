using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class SortingCapacity : Capacity
{
    // private List<Item> sorted_items = new List<Item>();

    public async void ReceiveMovable(Movable movable, Vector3 world_position)
    {
        ChunkEngine.Instance.FreeCapable(movable.data);
        movable.DisableMovements();
        movable.transform.SetParent(transform);

        // we set the player position to the sitting position
        movable.transform.position = world_position;
        if (Capable is Container container) { container.ContainCapable(movable.ID); }
        // Debug.Log("(SortingCapacity) Received movable " + movable.ID + " at " + world_position);

        // we also show the capable if we are shown :D
        if (AnimPlayer.IsVisible()) { movable.AnimPlayer.Show(); }

        // we wait one frame or 2 and we reset the position (to avoid potential colliders issues)
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        movable.transform.position = world_position;
        // Debug.Log("(SortingCapacity) Re forced the position of movable " + movable.ID + " at " + world_position);
    }
    public void RemoveMovable(Movable movable, Vector3 world_position)
    {
        if (movable.transform.parent != transform) { return; }
        movable.transform.SetParent(World.Instance.MovablesParent);
        movable.transform.position = world_position;
        movable.EnableMovements();
        ChunkEngine.Instance.AttachCapable(movable.data);

        if (Capable is Container container) { container.FreeCapable(movable.ID); }
        // Debug.Log("(SortingCapacity) Removed movable " + movable.ID + " at " + world_position);
    }

    // todo : yes we need this only to remove the items from the container, otherwise when unloading then reloading the container,
    // todo : it will re call LoadCapablesAccordingly which is bad...
    /* private void handle_on_item_grabbed(Item item, Capable capable)
    {

        // if (!sorted_items.Contains(item)) { return; }
        item.OnGrabbed -= handle_on_item_grabbed;
        if (Capable is Container container) { container.FreeCapable(item.ID); }
    } */
}