using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : Movable, Interactable
{
    public InteractCapacity Interactor => null;
    public InteractType InteractionType => InteractType.Other;

    [Header("Items offset")]
    public Vector3 items_offset = new Vector3(0f, 0.2f, 0f);

    // ON INTERACT
    public void OnInteract(Capable interactor)
    {
        anim_player.Play("interact");
        anim_player.AddToPile("idle_open");

        StartCoroutine(destroyObject());
    }
    private IEnumerator destroyObject()
    {
        // 1 - DROP ITEMS
        if (Inventory != null && Inventory.Count > 0)
        {
            // we get the drop capacity
            DropCapacity dropper = GetCapacity<DropCapacity>();
            if (dropper == null)
            {
                // we add it if not present
                AddCapacity("drop");

                // we wait a frame
                yield return null;
                dropper = GetCapacity<DropCapacity>();
            }
            dropper.random_direction = true;
            dropper.offset_drop = items_offset;
            dropper.DropMagnitude = 0.1f;

            // we drop all items
            int i = 0;
            while (i < Inventory.Items.Count)
            {
                Item item_to_drop = Inventory.Items[i];
                if (item_to_drop == null)
                {
                    Inventory.Items.RemoveAt(i);
                    continue; // skip null item_to_drops
                }

                // we drop the item_to_drop
                dropper.Select(item_to_drop);
                dropper.Use(this);
            }
        }


        // 2 - DESTROYING CAPACITIES
        if (log) { Debug.Log("Destroying capacities of " + name); }

        // we destroy all capacities (except DieCapacity FOR NOW)
        List<Capacity> capacities = new List<Capacity>(GetCapacities());
        while (capacities.Count > 0)
        {
            RemoveCapacity(capacities[0].name);
            capacities.RemoveAt(0);
        }


        // 3 - DESTROYING OTHER ELEMENTS
        if (transform.Find("inventory") is Transform inventory && inventory != null) { Destroy(inventory.gameObject); }
        if (transform.Find("feet") is Transform feet && feet != null) { Destroy(feet.gameObject); }
        if (transform.Find("light") is Transform light && light != null) { Destroy(light.gameObject); }

        // 4 - HANDLE PHYSICS
        ForceStop();
        // we switch the rigidbody collision detection to discrete since the dead body won't move very fast (not affected by our forces)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

        // we wait for a frame in order to the capacities to be destroyed & hover to be instanced
        yield return null;



        // 5 - TURNING TO ITEM
        Item item = gameObject.AddComponent<Item>();
        item.name = "package_leftovers";
        item.Reference = "other:package_leftovers";


        // 6 - DESTROYING OLD PACKAGE
        Destroy(this);
    }
}