using System;
using UnityEngine;

public class DropEngine : MonoBehaviour
{
    // SINGLETON LOGIC
    public static DropEngine Instance;
    protected virtual void Awake()
    {
        Instance = this;
    }

    [Header("Logs")]
    public bool log = false;


    // USE
    public void Drop(Capable dropper, Item item, DropParameters parameters = null)
    {
        // we check if it s a shuriken, if so we throw it
        // if (item is Shuriken) { (item as Shuriken).Use(dropper); }

        // we check if the dropper has an inventory
        Inventory inventory = dropper.Inventory;
        if (inventory == null)
        {
            if (log) { Debug.LogError("(DropEngine) " + dropper.ID + " has no inventory, cannot drop item : " + item.ID); }
            return;
        }

        // we try to drop the item
        bool drop = inventory.Drop(item);
        if (!drop)
        {
            // we could not drop the item ooops
            if (log) { Debug.LogError("(DropEngine) " + dropper.ID + " could not drop : " + item.ID); }
            return;
        }

        // we get the parameters to apply the drop on the ground
        DropParameters param = parameters != null ? parameters : DropParameters.Default;

        // we successfully dropped the item !!
        // we calculate the offset we drop it
        // Vector3 offset_position = (param.offset_drop == Vector2.zero) ? dropper.Orientation * 0.2f : param.offset_drop;

        // we move the item back to the world
        item.transform.position = dropper.transform.position + (Vector3)param.offset_drop;
        item.transform.SetParent(World.Instance.ItemsParent);
        item.transform.localScale = Vector3.one;

        // we make sure the item is not Placed
        item.Placed = false;

        // we add a force to the item
        float force_magnitude = -888f;
        if (item is not Shuriken)
        {
            Force force = new Force("drop", param.random_direction ? UnityEngine.Random.insideUnitCircle : dropper.Orientation, param.drop_magnitude);
            if (dropper is Movable movable && !param.lock_magnitude)
            {
                // we add the current moving velocity to the force (for dropping items while moving)
                force.magnitude += movable.Velocity.magnitude * 2f;
            }
            item.AddForce(force);
            force_magnitude = force.magnitude;
        }

        if (log)
        {
            Debug.Log("(DropEngine) " + dropper.ID + " dropped : " + item.ID +
                (force_magnitude != -888f ? " with force of magnitude : " + force_magnitude :
                " and item is a SHURIKEN so no force applied")
                + " and offset : " + param.offset_drop + " which makes the global world pos of the item : " + item.transform.position
                );
        }
    }


}

[Serializable] public class DropParameters
{
    public bool random_direction = true;
    public float drop_magnitude = 200f;
    public bool lock_magnitude = false; // if true the dropping force won't be influenced by capable's movement
    public Vector2 offset_drop = Vector2.zero;

    public static DropParameters Default = new DropParameters()
    {
        random_direction = true,
        drop_magnitude = 200f,
        lock_magnitude = false,
        offset_drop = Vector2.zero
    };

}