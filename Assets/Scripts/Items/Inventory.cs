using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class Inventory : MonoBehaviour {

    [Header("Items")]
    public List<Item> Items = new List<Item>();

    [Header("Inventory parameters")]
    [SerializeField] private int max_items = 9;
    [SerializeField] private bool scalable = false;

    [Header("Drop parameters")]
    [SerializeField] private float drop_magnitude = 50f;
    [SerializeField] private Transform parent_to_drop_items;


    [Header("Events")]
    public UnityEvent OnGrab;
    public UnityEvent OnDrop;


    [Header("Components")]
    public ItemBank bank;
    private Capable capable { get { return transform.parent.GetComponent<Capable>(); } }

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    // AWAKE
    void Awake()
    {
        // on récupère le bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();
    }

    void Start()
    {
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
        if (Items.Count >= max_items && !scalable) { return false; }

        // we add the item
        Items.Add(item);

        // we set the item to grabbed (which disables the hover collider)
        item.Grabbed = true;

        // we set the item parent
        item.transform.SetParent(transform);

        if (debug) { Debug.Log("(Inventory) " + capable.name + " grabbed : " + item.name); }

        // we trigger the event
        OnGrab.Invoke();

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

        // we move the item back to the world
        item.transform.position = capable.transform.position + ((Vector3) capable.Orientation * 0.2f);
        item.transform.SetParent(parent_to_drop_items);

        // we add a force to the item
        Force force = new Force("drop", capable.Orientation , drop_magnitude);
        if (capable is Movable)
        {
            // we add the current moving velocity to the force (for dropping items while moving)
            force.magnitude += (capable as Movable).Velocity.magnitude*2f;
        }
        item.AddForce(force);

        // we set the item parent
        item.transform.SetParent(parent_to_drop_items);

        if (debug) { Debug.Log("(Inventory) " + capable.name + " dropped : " + item.name + " with force of magnitude : " + force.magnitude); }

        // we trigger the event
        OnDrop.Invoke();

        return true;
    }


    // ! DEPRECATED
    public Hack[] getHacks() { return new Hack[0]; }
    public void setShow(bool show) { }
}
