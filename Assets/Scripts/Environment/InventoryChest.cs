using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class InventoryChest : Chest
{ 
    // inventory
    public UI_ChestInventory inventory;

    [Header("INVENTORY CHEST")]
    [SerializeField] protected bool randomize_on_start = true;
    [SerializeField] protected string randomize_cat = "all";

    // unity functions
    protected override void Awake()
    {
        base.Awake();

        // on récupère l'inventaire
        inventory = transform.Find("inventory").GetComponent<UI_ChestInventory>();

    }

    void Start()
    {
        // randomize
        if (inventory == null) {return;}
        if (randomize_on_start) { inventory.randomize(randomize_cat); }
    }

    // OPENING
    protected override void success_open()
    {
        base.success_open();

        // on met à jour l'inventaire
        // inventory.setShow(true);
        inventory.show();
    }

    protected override void close()
    {
        // on met à jour l'inventaire
        // inventory.setShow(false);
        inventory.hide();

        base.close();
    }


    // INVENTORY FUNCTIONS
    public bool grab(Item item)
    {
        // item.transform.SetParent(inventory.transform);
        // return inventory.addItem(item);
        inventory.grabItem(item);
        return true;
    }

    public void forceGrab(Item item)
    {
        // inventory.forceAddItem(item);
    }

}

