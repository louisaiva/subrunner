using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class SortingCapacity : Capacity
{
    public void ReceiveMovable(Movable movable, Vector3 world_position)
    {
        ChunkEngine.Instance.FreeCapable(movable.data);
        movable.DisableMovements();
        movable.transform.SetParent(transform);

        // we set the player position to the sitting position
        movable.transform.position = world_position;
    }
    public void RemoveMovable(Movable movable, Vector3 world_position)
    {
        if (movable.transform.parent != transform) { return; }
        movable.transform.SetParent(World.Instance.MovablesParent);
        movable.transform.position = world_position;
        movable.EnableMovements();
        ChunkEngine.Instance.AttachCapable(movable.data);
    }
}