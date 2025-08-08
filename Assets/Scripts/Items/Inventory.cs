using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{

    [Header("Items")]
    public List<Item> Items = new List<Item>();
    public int Count { get { return Items.Count; } }

    [Header("Events")]
    public UnityEvent OnGrab;
    public UnityEvent OnDrop;


    [Header("Components")]
    [SerializeField] private List<UI_Inventory> uis = new List<UI_Inventory>();
    public UI_Inventory ui { get { return uis.Count > 0 ? uis[0] : null; } }
    public Capable capable { get { return transform.parent.GetComponent<Capable>(); } }

    [Header("Logs")]
    [SerializeField] private bool debug = false;

    // AWAKE
    void Awake()
    {
        if (capable is Perso && uis.Count > 0 && uis[0] == null)
        {
            // we just revived we don't have any uis, so we make them
            uis = new List<UI_Inventory>
            {
                UI_Manager.Instance.GetPool("inventory").transform.Find("ui_inventory").GetComponent<UI_Inventory>(),
                UI_Manager.Instance.GetPool("hud").GetComponent<UI_HUD>().perso_quick_inventory
            };
        }

        // we attach the inventory to the UI
        foreach (UI_Inventory ui in uis)
        {
            if (ui == null) { continue; }
            ui.Inventory = this;
        }
    }

    // START
    void Start()
    {
        // on initialise l'UI
        foreach (UI_Inventory ui in uis)
        {
            if (ui == null)
            {
                Debug.LogWarning("(Inventory) " + name + $" has a null UI_Inventory : {ui.name}, skipping initialization");
                continue;
            }
            ui.Init();
        }

        // on récupère les items
        foreach (Transform child in transform)
        {
            Grab(child.GetComponent<Item>());
        }
    }


    // GRAB / DROP
    public virtual bool Grab(Item item, List<UI_Inventory> uis_to_ignore = null)
    {
        // we check if we can add the item
        if (item == null) { return false; }

        // we check if we have at least one ui_inventory
        if (ui != null)
        {
            // we try to make the first ui_inventory (which is our reference ui_inventory) to grab it
            // if it can grab it, all the others can grab it.
            // if no, we return false
            if (uis_to_ignore == null) { uis_to_ignore = new List<UI_Inventory>(); }
            if (!uis_to_ignore.Contains(ui) && !ui.UI_Grab(item))
            {
                if (debug) { Debug.LogWarning("(Inventory) " + capable.name + " can't grab : " + item.name + " in " + ui.name); }
                return false; // if the first ui_inventory can't grab it, we return false
            }
            for (int i = 1; i < uis.Count; i++)
            {
                if (uis_to_ignore.Contains(uis[i])) { continue; } // we skip the ui_to_ignore
                uis[i].UI_Grab(item); // we try to make the other ui_inventories grab it (we don't care if it can't grab as long as the 1st can)
            }
        }

        // we check if the item is already grabbed somewhere, if so we drop it
        if (item.Grabbed && item.HolderInventory != null) { item.HolderInventory.Drop(item, uis_to_ignore); }

        // we add the item
        Items.Add(item);
        item.Grabbed = true;

        // we set the item parent and reset its local position
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;

        // we trigger the event
        OnGrab.Invoke();

        if (debug) { Debug.Log("(Inventory) " + capable.name + " grabbed : " + item.name); }

        return true;
    }
    public virtual bool Drop(Item item, List<UI_Inventory> uis_to_ignore = null)
    {
        // we check if we can remove the item
        if (item == null) { return false; }
        if (!Items.Contains(item)) { return false; }

        // we remove the item
        Items.Remove(item);

        // we set the item to dropped (which enables the hover collider)
        item.Grabbed = false;

        // we trigger the event
        OnDrop.Invoke();

        // we update the UI
        if (uis_to_ignore == null) { uis_to_ignore = new List<UI_Inventory>(); }
        foreach (UI_Inventory ui in uis)
        {
            if (uis_to_ignore.Contains(ui)) { continue; } // we skip the ui_to_ignore
            ui.UI_Drop(item);
        }

        if (debug) { Debug.Log("(Inventory) " + capable.name + " dropped : " + item.name); }

        return true;
    }
    public virtual bool Remove(Item item)
    {
        // only for items that are going to be destroyed

        // we check if we can remove the item
        if (item == null) { return false; }
        if (!Items.Contains(item)) { return false; }

        // we remove the item
        Items.Remove(item);

        // we update the UI
        uis.ForEach(ui => ui.UI_Drop(item));

        if (debug) { Debug.Log("(Inventory) " + capable.name + " removed : " + item.name); }

        return true;
    }

    // GETTERS
    public Inventory GetInteractingInventory()
    {
        string s = "(Inventory) " + capable.name + " is looking for an interacting inventory\n\n";

        // check if we are the interactable (so we look for the interactor)
        // typically for Chest
        if (capable is Interactable)
        {
            // this is the other capable
            s += "we are the interactable\n";
            InteractCapacity interactor = (capable as Interactable).Interactor;

            // check if we have an interactor
            if (interactor == null)
            {
                if (debug) { Debug.LogWarning(s + "we don't have an interactor\n"); }
                return null;
            }

            // yes we do !! return its inventory
            if (debug)
            {
                Debug.Log(s + "we have an interactor : " + interactor.capable.name
                + "\nand its inventory is " + interactor.capable.Inventory.name);
            }
            return interactor.capable.Inventory;
        }


        // check if we are the interactor (so we look for the interactable)
        // typically for Being
        else if (capable.GetCapacity<InteractCapacity>() != null)
        {
            // this is our capable
            s += "we are the interactor\n";
            InteractCapacity interactor = capable.GetCapacity<InteractCapacity>();

            // check if we have an interactable
            Capable interactable = interactor.interactable as Capable;
            if (interactable == null)
            {
                if (debug) { Debug.LogWarning(s + "we don't have an interactable\n"); }
                return null;
            }

            s += "we have an interactable : " + interactable.name + "\n";
            // checks if the interactable is an Openable and is not closed
            if (interactable is Openable)
            {
                s += "and it's an Openable\n";
                Openable openable = interactable as Openable;

                // fermé et pas en train de s'ouvrir
                if (!openable.is_open && !openable.is_moving)
                {
                    if (debug) { Debug.LogWarning(s + "but it's closed & not opening\n"); }
                    return null;
                }

                // en train de se fermer
                else if (openable.is_open && openable.is_moving)
                {
                    if (debug) { Debug.LogWarning(s + "but it's closing\n"); }
                    return null;
                }

                s += "and it's open !!\n";
            }
            else if (interactable.Inventory == null)
            {
                if (debug) { Debug.LogWarning(s + "but it doesn't have an inventory\n"); }
                return null;
            }

            // we return the interactable's inventory
            if (debug) { Debug.Log(s + "and its inventory is " + interactable.Inventory.name + "\n\n"); }
            return interactable.Inventory;
        }

        // we return null
        return null;
    }
    public Item GetItem(string reference)
    {
        // we check if the item is in the inventory
        foreach (Item item in Items)
        {
            if (item.Reference == reference) { return item; }
        }
        return null;
    }
    public List<Item> GetItemsByType<T>() where T : Item
    {
        // we get all the items of type T
        List<Item> items = new List<Item>();
        foreach (Item item in Items)
        {
            if (item is T) { items.Add(item); }
        }
        return items;
    }
    public List<Item> GetItemsByRule(string rule = "", bool exclusion_rule = false)
    {
        // we get all the items that match the rule
        List<Item> items = new List<Item>();
        foreach (Item item in Items)
        {
            if (!exclusion_rule && item.ValidateRule(rule)) { items.Add(item); }
            else if (exclusion_rule && !item.ValidateRule(rule)) { items.Add(item); }
        }
        return items;
    }

    // UI MANAGEMENT
    public void RemoveUI(UI_Inventory ui_inventory)
    {
        if (!uis.Contains(ui_inventory)) { return; }

        // we remove the UI from the list
        uis.Remove(ui_inventory);
        ui_inventory.Inventory = null;
        if (debug) { Debug.Log("(Inventory) " + capable.name + " removed UI_Inventory : " + ui_inventory.name); }
    }
    public void AddUI(UI_Inventory ui_inventory)
    {
        if (uis.Contains(ui_inventory)) { return; }

        // we add the UI to the list
        uis.Add(ui_inventory);
        ui_inventory.Inventory = this;
        if (debug) { Debug.Log("(Inventory) " + capable.name + " added UI_Inventory : " + ui_inventory.name); }
    }

}
