using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class InventoryChest : Chest, I_Grabber
{ 
    // inventory
    public UI_ChestInventory inventory;
    public Transform go_parent;

    [Header("INVENTORY CHEST")]
    [SerializeField] protected bool randomize_on_start = true;
    [SerializeField] protected string randomize_cat = "all";

    // unity functions
    protected override void Awake()
    {
        base.Awake();

        // on récupère l'inventaire
        inventory = transform.Find("inventory").GetComponent<UI_ChestInventory>();

        // on récupère le parent
        go_parent = transform.Find("gameobjects");

    }

    protected void Start()
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


    // GRABBER
    public bool canGrab()
    {
        return inventory.canGrab();
    }

    public void grab(GameObject target)
    {
        if (target.GetComponent<Item>())
        {
            if (grab(target.GetComponent<Item>()))
            {
                // on le met dans l'inventaire
                target.transform.SetParent(go_parent);

                // on désactive l'item
                target.SetActive(false);
                Debug.Log("grab " + target.name + " in " + gameObject.name +" & applied setactive : "+target.activeSelf);
            }
        }
    }

    // INVENTORY FUNCTIONS
    public bool grab(Item item)
    {
        // on vérifie si on a affaire à un item déjà instancié ou pas
        if (item.gameObject.scene.name == null)
        {
            // on instancie l'item
            item = Instantiate(item, transform.position, Quaternion.identity) as Item;

            // on met le zoom à 0.5
            item.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
        item.transform.SetParent(go_parent);

        // on ajoute l'item à l'inventaire
        return inventory.grabItem(item);
    }

    public void forceGrab(Item item)
    {
        // inventory.forceAddItem(item);
    }

}

