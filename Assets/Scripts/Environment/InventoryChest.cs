using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class InventoryChest : Chest, I_Grabber
{ 
    // inventory
    [Header("INVENTORY CHEST")]
    public EnumItem bank = new EnumItem();
    public Transform go_parent;
    [SerializeField] protected List<Item> items = new List<Item>();
    [SerializeField] protected bool randomize_on_start = true;
    [SerializeField] protected string randomize_cat = "all";
    // [SerializeField] protected Vector2 drop_direction = new Vector2(0, -1);


    [Header("DROP")]
    [SerializeField] protected float drop_angle_base = -90f;
    [SerializeField] protected float drop_angle_wide = 100f;
    [SerializeField] protected float drop_force = 10f;

    [Header("GRAB")]
    [SerializeField] protected LayerMask items_layer;
    protected float grab_range = 0.1f;
    protected float attract_range = 1.5f;
    protected float attract_force_base = 1f;

    // unity functions
    protected void Start()
    {
        // on récupère le parent
        go_parent = transform.Find("items");

        // on récupère les items
        items = new List<Item>(go_parent.GetComponentsInChildren<Item>());
        if (randomize_on_start) { randomize(); }
        items_layer = LayerMask.GetMask("Items");
    }

    protected void Update()
    {
        if (is_open && is_moving)
        {
            attractItems();
        }
    }


    // OPENING
    protected override void success_open()
    {
        base.success_open();

        // on met à jour l'inventaire
        // inventory.setShow(true);
        // inventory.show();

        // on drop les items
        dropItems();
    }


    // GRABBER
    public bool canGrab() { return true; }

    public void grab(GameObject target)
    {
        if (!target.GetComponent<Item>()) { return; }

        // Debug.Log("grab " + target.name + " in " + gameObject.name + " & applied setactive : " + target.activeSelf);
        
        // on récupère l'item
        grab(target.GetComponent<Item>());
    }

    protected void dropItems()
    {
        int nb_items = items.Count;
        // float base_angle = drop_angle_base ;
        for (int i = 0; i < nb_items; i++)
        {
            // angles.Add();

            // on récupère un item
            Item item = items[0];

            // on drop sur le sol avec une force dans une direction aléatoire
            // item.transform.SetParent(transform);
            item.gameObject.SetActive(true);

            // calcule une position aléatoire autour de l'oject
            // float angle = Random.Range(drop_angle_base - drop_angle_wide , drop_angle_base + drop_angle_wide) * Mathf.Deg2Rad;
            float angle = drop_angle_base;
            if (nb_items > 1)
            {
                angle = (drop_angle_base - drop_angle_wide / 2f + (drop_angle_wide / (nb_items - 1)) * i) * Mathf.Deg2Rad;
            }
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            // Vector2 position = (Vector2)transform.position + direction * 0.5f;

            // item.transform.position = position;
            item.transform.position = transform.position;
            item.addForce(new Force(direction, drop_force));

            // on supprime l'item de l'inventaire
            items.Remove(item);
        }
    }

    protected void attractItems()
    {
        // on récupère tous les items dans un range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, grab_range, items_layer);

        // on les grab
        foreach (Collider2D collider in colliders)
        {
            if (collider.GetComponent<Item>() == null) { continue; }
            if (items.Contains(collider.GetComponent<Item>())) { continue; }

            grab(collider.gameObject);
        }

        // on récupère les items plus loin
        Collider2D[] attract_colliders = Physics2D.OverlapCircleAll(transform.position, attract_range, items_layer);

        // on applique une force à ces items
        foreach (Collider2D collider in attract_colliders)
        {
            if (collider.GetComponent<Item>() == null) { continue; }
            if (items.Contains(collider.GetComponent<Item>())) { continue; }

            Item item = collider.GetComponent<Item>();
            if (item == null) { continue; }

            // on calcule la force
            Vector2 direction = (Vector2)transform.position - (Vector2)item.transform.position;
            float distance = direction.magnitude;
            direction.Normalize();
            float force = attract_force_base / distance;

            // on applique la force
            item.addForce(new Force(direction, force));
        }
    }


    // INVENTORY FUNCTIONS
    public void randomize()
    {
        int max_items = Random.Range(1, 5);

        // on ajoute des items random
        for (int i = 0; i < max_items; i++)
        {
            // on récupère un prefab random
            Item item = bank.getRandomItem(randomize_cat);

            // on ajoute l'item
            grab(item);
        }
    }

    public void grab(Item item)
    {
        // on vérifie si on a affaire à un item déjà instancié ou pas
        if (item.gameObject.scene.name == null)
        {
            // on instancie l'item
            item = Instantiate(item, transform.position, Quaternion.identity) as Item;

            // on met le zoom à 0.5
            item.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
        
        // on clear les forces
        item.clearForces();
        
        // on désactive l'item
        item.transform.SetParent(go_parent);
        item.gameObject.SetActive(false);

        // on ajoute l'item à l'inventaire
        items.Add(item);
    }
}

