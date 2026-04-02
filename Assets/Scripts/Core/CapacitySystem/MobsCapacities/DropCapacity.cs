using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// DropCapacity is a Capacity that allows the Capable to drop items.
/// It applies a force to the item in the direction of the Capable's orientation.
/// only for dropping items on the ground !!
/// when dropping an item from an inventory to another, it is done via UI_Item that directly calls Inventory.Grab(item), never the Drop one but it is ok !
/// </summary>

public class DropCapacity : Capacity
{
    public override bool Able
    {
        get
        {
            // checks if we have a selected item
            if (selected_item == null) { return false; }
            return true;
        }
    }

    [Header("Selection")]
    [SerializeField] private Item selected_item;

    [Header("Drop parameters")]
    public bool random_direction = true;
    [SerializeField] private float drop_magnitude = 200f;
    public float DropMagnitude { get => drop_magnitude; set => drop_magnitude = Mathf.Max(0, value); }
    public bool lock_magnitude = false; // if true the dropping force won't be influenced by capable's movement
    public Vector3 offset_drop = Vector3.zero; // if true the dropping force won't be influenced by capable's movement
    // [SerializeField] private Transform parent_to_drop_items;

    [Header("Components")]
    [SerializeField] private Inventory inventory;

    [Header("Input & Callbacks")]
    [SerializeField] private InputActionReference dropInput;
    private InputAction dropAction;
    private event System.Action<InputAction.CallbackContext> dropCallback;

    // START
    private void Start()
    {
        // on récupère la bank
        inventory = Capable.Inventory;

        // on récupère l'action drop
        dropAction = InputManager.Instance.GetComponent<InputManager>().GetAction(dropInput);

        // on définit le callback
        dropCallback = ctx => Use(Capable);
    }

    // SELECT / DESELECT
    public void Select(Item item)
    {

        selected_item = item;

        // we set the callback
        if (dropAction != null) { dropAction.performed += dropCallback; }

        if (log) { Debug.Log("(DropCapacity) selected (and callback set) : " + item.name); }
    }
    public void Deselect()
    {
        if (selected_item == null) { return; }

        if (log) { Debug.Log("(DropCapacity) deselected (and callback removed) : " + selected_item.name); }

        selected_item = null;

        // we remove the callback
        if (dropAction != null) { dropAction.performed -= dropCallback; }

    }


    public void SelectLastItem()
    {
        // we check if we have items
        if (inventory.Items.Count == 0) { return; }

        // we select the last item
        Select(inventory.Items[inventory.Items.Count - 1]);
    }

    // USE
    public override void Use(Capable capable)
    {
        // we check if we have a selected item
        if (selected_item == null)
        {
            if (log) { Debug.LogError("(DropCapacity) no selected item"); }
            return;
        }
        Item item = selected_item;

        // we check if it s a shuriken, if so we throw it
        if (item is Shuriken) { (item as Shuriken).Use(capable); }

        // we try to drop the item
        bool drop = inventory.Drop(selected_item);
        if (!drop)
        {
            // we could not drop the item ooops
            if (log) { Debug.LogError("(DropCapacity) could not drop : " + item.name); }
            return;
        }

        // we successfully dropped the item !!
        // we calculate the offset we drop it
        Vector3 offset_position = random_direction ? offset_drop : capable.Orientation * 0.2f;

        // we move the item back to the world
        item.transform.position = capable.transform.position + offset_position;
        item.transform.SetParent(World.Instance.ItemsParent);
        item.transform.localScale = Vector3.one;

        // we make sure the item is not Placed
        item.Placed = false;

        // we add a force to the item
        float force_magnitude = -888f;
        if (item is not Shuriken)
        {
            Force force = new Force("drop", random_direction ? Random.insideUnitCircle : capable.Orientation, drop_magnitude);
            if (capable is Movable && !lock_magnitude)
            {
                // we add the current moving velocity to the force (for dropping items while moving)
                force.magnitude += (capable as Movable).Velocity.magnitude * 2f;
            }
            item.AddForce(force);
            force_magnitude = force.magnitude;
        }

        if (log)
        {
            Debug.Log("(DropCapacity) " + capable.name + " dropped : " + item.name +
                (force_magnitude != -888f ? " with force of magnitude : " + force_magnitude :
                " and item is a SHURIKEN so no force applied")
                );
        }

    }

    private void OnDestroy()
    {
        // we remove the callback
        if (dropAction != null) { dropAction.performed -= dropCallback; }
    }
}