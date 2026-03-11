using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This is the new version of UI_Item, which is obsolete now.
/// UI_ItemStack is the visual representation of ItemStack, and so is linked to an ItemStack from the construction of ItemStack to its death.
/// UI_ItemStack is created, handled and destroyed by UI_ItemSlottable which is the visual rep. of ItemPool (lol)
/// </summary>
public class UI_ItemStack : UI_ImageSlot, Droppable, Descriptable, ItemReceivable, ItemMovable
{


    public Sprite drag_sprite;
    public Sprite drag_hover_sprite;
    protected Sprite current_item_sprite;

    [Header("Item Stack Data")]
    public ItemStack Stack;

    [Header("Descriptable")]
    public string Name => Stack.Name;
    public string Description => Stack.Description;

    [Header("Components")]
    protected TextMeshProUGUI quantity_text;
    protected Image item_image;

    [Header("Logs part 2")]
    public bool log_drop = false;


    // UI_ItemSlottable
    private UI_ItemSlottable _item_pool;
    public UI_ItemSlottable UI_ItemSlottable
    {
        get
        {
            if (_item_pool != null) { return _item_pool; }
            _item_pool = GetComponentInParent<UI_ItemSlottable>(includeInactive: true);
            return _item_pool;
        }
    }




    // AWAKE
    public virtual void Init(ItemStack stack)
    {
        // get some components
        item_image = transform.Find("item").GetComponent<Image>();
        quantity_text = transform.Find("qty").GetComponent<TextMeshProUGUI>();

        // attach the UI_ItemStack to its stack (register to its callbacks)
        this.Stack = stack;
        stack.OnUpdated += SyncUIWithStack;

        // sync the ui
        SyncUIWithStack();
    }
    private void OnDestroy()
    {
        if (Stack == null) { return; }
        Stack.OnUpdated -= SyncUIWithStack; // we unregister from the stack callbacks
    }


    // UPDATE UI
    public void SyncUIWithStack()
    {
        if (Stack == null) { return; }

        update_ui_item();
        update_ui_qty();
    }
    protected virtual void update_ui_item()
    {
        if (Stack.Quantity == 0) { clear_ui_item(); return; }

        // on charge le sprite de l'image
        current_item_sprite = ItemBank.Instance.GetSprite(Stack.Item);
        set_ui_item(current_item_sprite);

        // on change le nom du prefab
        name = "ui_" + Stack.Item.Reference;

        // on enable le slot
        Enable();
    }
    protected void update_ui_qty()
    {
        // we update the text
        quantity_text.text = Stack.Quantity.ToString();

        // we show or hide the text
        quantity_text.gameObject.SetActive(Stack.Quantity > 1);
    }
    protected virtual void set_ui_item(Sprite sprite)
    {
        item_image.sprite = sprite;
        item_image.color
                = sprite != null
                ? new Color(1, 1, 1, 1)
                : new Color(0, 0, 0, 0);
        if (sprite == null) { return; }

        // on calcule la taille de l'image
        RectTransform rt = item_image.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
    }
    protected void clear_ui_item()
    {
        // on change le sprite de l'image
        set_ui_item(null);
        current_item_sprite = null;

        // on change le nom du prefab
        name = "ui_empty";

        // on disable le slot vu qu'on a plus rien dedans
        Disable();
    }




    // CLICK

    // ON POINTER
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we check if we have an item
        if (Stack.Quantity == 0) { return; }
        if (log) { Debug.Log("OnPointerClick on " + gameObject.name); }

        Item item = Stack.Item;

        // we check if the item is an usable
        if (item is Usable usable)
        {
            usable.Use(item.Holder);
            UI_Manager.Instance.SwitchToHUD();
            return;
        }

        // we check if the item is an inspectable
        if (item is Inspectable inspectable) { inspectable.Inspect(); }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        set_ui_item(current_item_sprite);
    }


    // DROPPABLE
    public void OnPointerDropped(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we check if we have an item
        if (Stack == null || Stack.Quantity == 0) { return; }
        if (log) { Debug.Log("OnPointerDropped on " + gameObject.name); }

        // we get the item & drop it
        drop_item(Stack.Item);
    }
    private void drop_item(Item item)
    {
        // on récupère l'inventory qui drop l'item
        Inventory inventory = item.Holder.Inventory;

        if (log_drop) { Debug.Log($"(UI_ItemStack) dropping item {item.name} from inventory of {inventory.Capable.name}"); }

        // on cherche l'inventory qui reçoit l'item
        Inventory inventory_to_drop = inventory.GetInteractingInventory();
        if (log_drop) { Debug.Log($"(UI_ItemStack) interacting inventory to drop in: {(inventory_to_drop != null ? inventory_to_drop.Capable.name : "none")}"); }

        // we drop the item in the other inventory
        if (inventory_to_drop != null)
        {
            inventory_to_drop.Grab(item);
            return;
        }

        // we don't have an inventory to drop so we drop on the ground
        // we check if we have a DropCapacity
        DropCapacity dropper = inventory.Capable.GetCapacity<DropCapacity>();
        if (dropper != null)
        {
            if (log_drop) { Debug.Log($"(UI_ItemStack) no inventory so we dropping item {item.name} on the ground with dropper of {inventory.Capable.name}"); }
            dropper.Select(item);
            dropper.random_direction = true;
            dropper.Use(inventory.Capable);
            dropper.random_direction = false;
        }
        else
        {
            if (log_drop) { Debug.Log($"(UI_ItemStack) no inventory and no dropper capacity, so we simply drop item {item.name} from inventory of {inventory.Capable.name} (it may be lost if the inventory is a chest for example)"); }
            // the inventory simply drops the item (dropper may be a chest)
            inventory.Drop(item);
        }
    }


    // ON POINTER DRAG
    public void OnPointerDragDown()
    {
        // check if disabled
        if (Disabled) { return; }
        if (log) { Debug.Log("OnPointerDragDown on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = drag_sprite;
    }
    public virtual void OnPointerDragEnter(UI_ItemStack moving_ui_item)
    {
        // check if disabled
        if (Disabled) { return; }
        if (log) { Debug.Log("OnPointerDragEnter on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = drag_hover_sprite;


        // on met un icon de switch à la place de l'item
        Sprite switch_icon = Stack.CanAdd(moving_ui_item.Stack.Item)
            ? ItemBank.Instance.GetUI_Icon("merge")
            : ItemBank.Instance.GetUI_Icon("switch");

        // si on a aucun item on met tout simplement "move"
        if (Stack.Quantity == 0) { switch_icon = ItemBank.Instance.GetUI_Icon("place"); }
        set_ui_item(switch_icon);
    }
    public void OnPointerDragUp()
    {
        if (log) { Debug.Log("OnPointerDragUp on " + gameObject.name); }
        OnPointerEnter(null);
    }
}