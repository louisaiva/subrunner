using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour {


    // items
    // public List<Item> items = new List<Item>();
    public int max_items = 10;

    // prefabs
    public GameObject item_prefab;

    // UI

    // unity functions
    void Start()
    {
        // on récupère le prefab
        item_prefab = Resources.Load("prefabs/ui/item") as GameObject;
    }

    void Update()
    {
        
    }

    // functions
    public void addItem(Item item)
    {
        // on regarde si on peut ajouter l'item
        if (transform.childCount >= max_items) { return; }

        // on récupère le gameobject de l'item
        GameObject item_go = item.gameObject;

        // on ajoute l'item a l'inventaire
        item_go.transform.SetParent(transform);
    }

    public void removeItem(Item item)
    {
        GameObject item_go = item.gameObject;

        // on regarde si l'item est dans l'inventaire
        if (!item.transform.parent == this.transform) { return; }

        // on desactive l'item
        // ! temporaire
        // items.Remove(item);
        item_go.SetActive(false);
    }

    public void createItem(string item_name)
    {
        // on crée l'item
        GameObject item_go = Instantiate(item_prefab, transform.position, Quaternion.identity) as GameObject;
        Item item = item_go.GetComponent<Item>();

        // on change le nom de l'item
        item.item_name = item_name;

        // on ajoute l'item
        addItem(item);
    }

    // getters

    public List<Item> getItems()
    {
        // on récupère les items
        List<Item> items = new List<Item>();
        foreach (Transform child in transform)
        {
            // on ajoute l'item
            items.Add(child.GetComponent<Item>());
        }
        return items;
    }

    public Item getItem(string item_name)
    {
        // on cherche l'item
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<Item>().item_name == item_name)
            {
                // on retourne l'item
                return child.gameObject.GetComponent<Item>();
            }
        }
        return null;
    }

    public List<Hack> getHacks()
    {
        // on récupère les hacks
        List<Hack> hacks = getItems().OfType<Hack>().ToList();
        return hacks;
    }
}