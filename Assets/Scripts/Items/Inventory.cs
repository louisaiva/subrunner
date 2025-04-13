using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class Inventory : MonoBehaviour {

    [Header("Items")]
    public List<Item> Items = new List<Item>();

    [Header("Events")]
    public UnityEvent OnGrab;
    public UnityEvent OnDrop;


    [Header("Components")]
    [SerializeField] private List<UI_Inventory> uis = new List<UI_Inventory>();
    public UI_Inventory ui { get { return uis.Count > 0 ? uis[0] : null; } }
    public Capable capable { get { return transform.parent.GetComponent<Capable>(); } }

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    // AWAKE
    void Awake()
    {
        // on informe les UI de l'inventaire que l'on est là
        uis.ForEach(ui => ui.inventory = this);
    }

    // START
    void Start()
    {
        // on initialise l'UI
        uis.ForEach(ui => ui.Init());

        // on récupère les items
        foreach (Transform child in transform)
        {
            Grab(child.GetComponent<Item>());
        }
    }


    // GRAB / DROP
    public bool Grab(Item item)
    {
        // we check if we can add the item
        if (item == null) { return false; }

        // we check if we have an ui_inventory & if we can store the item in it
        if (ui != null)
        {
            // we have at least one ui_inventory
            // we try to make it grab in the first ui_inventory
            // if he can't, we do not grab it and we return false
            if (!ui.UI_Grab(item)) { return false; }

            // if he can, we grab it in all ui_inventories
            for (int i=1; i < uis.Count; i++) { uis[i].UI_Grab(item); }
        }

        // we check if the item is already grabbed somewhere, if so we drop it
        if (item.Grabbed) { item.transform.parent.GetComponent<Inventory>().Drop(item); }

        // we add the item
        Items.Add(item);

        // we set the item to grabbed (which disables the hover collider)
        item.Grabbed = true;

        // we set the item parent
        item.transform.SetParent(transform);

        // we trigger the event
        OnGrab.Invoke();

        if (debug) { Debug.Log("(Inventory) " + capable.name + " grabbed : " + item.name); }

        return true;
    }
    public bool Drop(Item item)
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
        uis.ForEach(ui => ui.UI_Drop(item));
        
        if (debug) { Debug.Log("(Inventory) " + capable.name + " dropped : " + item.name); }

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
            s+="we are the interactable\n";
            InteractCapacity interactor = (capable as Interactable).Interactor;

            // check if we have an interactor
            if (interactor == null)
            {
                if (debug) { Debug.LogWarning(s + "we don't have an interactor\n");}
                return null;
            }

            // yes we do !! return its inventory
            if (debug)
            {
                Debug.Log(s + "we have an interactor : " + interactor.capable.name
                + "\nand its inventory is " + interactor.capable.inventory.name );
            }
            return interactor.capable.inventory;
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
                if (debug) { Debug.LogWarning(s + "we don't have an interactable\n");}
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
                    if (debug) { Debug.LogWarning(s + "but it's closed & not opening\n");}
                    return null;
                }

                // en train de se fermer
                else if (openable.is_open && openable.is_moving)
                {
                    if (debug) { Debug.LogWarning(s + "but it's closing\n");}
                    return null;
                }

                s+= "and it's open !!\n";
            }
            else if (interactable.inventory == null)
            {
                if (debug) { Debug.LogWarning(s + "but it doesn't have an inventory\n");}
                return null;
            }

            // we return the interactable's inventory
            if (debug) { Debug.Log(s + "and its inventory is " + interactable.inventory.name + "\n\n");}
            return interactable.inventory;
        }

        // we return null
        return null;
    }




    // ! DEPRECATED
    public Hack[] getHacks() { return new Hack[0]; }
    public void setShow(bool show) { }
}
