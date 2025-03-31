using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class Inventory : MonoBehaviour {

    [Header("Items")]
    public List<Item> Items = new List<Item>();

    [Header("Inventory parameters")]
    public int MaxItems = 9;
    public bool Scalable = false;

    [Header("Events")]
    public UnityEvent OnGrab;
    public UnityEvent OnDrop;


    [Header("Components")]
    public UI_Inventory ui;
    public Capable capable { get { return transform.parent.GetComponent<Capable>(); } }

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    // AWAKE
    void Awake()
    {
        // on vérifie si on a un ui_inventory
        if (ui != null)
        {
            ui.inventory = this;
        }
    }

    // START
    void Start()
    {
        // on initialise l'UI
        ui?.Init();

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
        if (Items.Count >= MaxItems && !Scalable) { return false; }

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
        
        // we update the UI
        ui?.UI_Grab(item);

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
        ui?.UI_Drop(item);
        
        if (debug) { Debug.Log("(Inventory) " + capable.name + " dropped : " + item.name); }

        return true;
    }

    // GETTERS
    public Inventory GetInteractingInventory()
    {
        // check if we are the interactable (so we look for the interactor)
        // typically for Chest
        if (capable is Interactable)
        {
            // this is the other capable
            InteractCapacity interactor = (capable as Interactable).Interactor;

            // check if we have an interactor
            if (interactor == null) { return null; }

            // yes we do !! return its inventory
            return interactor.capable.inventory;
        }


        // check if we are the interactor (so we look for the interactable)
        // typically for Being
        else if (capable.GetCapacity<InteractCapacity>() != null)
        {
            // this is our capable
            InteractCapacity interactor = capable.GetCapacity<InteractCapacity>();
            
            // check if we have an interactable
            Capable interactable = interactor.interactable as Capable;
            if (interactable == null) { return null; }

            // checks if the interactable is an Openable and is closed
            if (interactable is Openable && !(interactable as Openable).is_open) { return null; }

            // we return the interactable's inventory
            return interactable.inventory;
        }

        // we return null
        return null;
    }




    // ! DEPRECATED
    public Hack[] getHacks() { return new Hack[0]; }
    public void setShow(bool show) { }
}
