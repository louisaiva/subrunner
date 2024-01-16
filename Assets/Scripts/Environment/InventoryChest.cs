using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class InventoryChest : Chest
{ 
    // inventory
    public Inventory inventory;

    [Header("INVENTORY CHEST")]
    [SerializeField] private bool randomize_on_start = true;

    // unity functions
    protected override void Awake()
    {
        base.Awake();

        // on récupère l'inventaire
        inventory = transform.Find("inventory").GetComponent<Inventory>();

        // randomize
        if (randomize_on_start) { inventory.randomize(); }
    }


    // OPENING
    protected override void success_open()
    {
        base.success_open();

        // on met à jour l'inventaire
        inventory.setShow(true);
    }

    protected override void close()
    {
        // on met à jour l'inventaire
        inventory.setShow(false);

        base.close();
    }


    // INVENTORY FUNCTIONS
    public bool grab(Item item)
    {
        // item.transform.SetParent(inventory.transform);
        return inventory.addItem(item);
    }

    public void forceGrab(Item item)
    {
        inventory.forceAddItem(item);
    }

}

