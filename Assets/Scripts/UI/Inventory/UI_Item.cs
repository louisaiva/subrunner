using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Obsolete("UI_Item is deprecated, use UI_ItemStack instead")]
public class UI_Item : UI_ImageSlot, Droppable, Descriptable
{

    public Sprite drag_sprite;
    public Sprite drag_hover_sprite;

    // [Header("Item Reference")]
    // protected List<Item> items = new List<Item>();
    // public string Reference { get => items.Count > 0 ? items[0].Reference : ""; }
    [Header("Item Stack Data")]
    public ItemStack Stack = null;

    [Header("Descriptable")]
    public string Name => Stack.ItemReference;
    public string Description => Stack != null
                                ? Stack.Item != null
                                    ? Stack.Item.ItemDescription
                                    : ""
                                : "";


    [Header("Item Stacking")]
    public TextMeshProUGUI quantity_text;
    // public bool Stackable { get => MaxQty > 1; }
    // public virtual int MaxQty { get => items.Count > 0 ? items[0].MaxQty : 1; }
    // public int Quantity => Stack.Quantity;

    [Header("Components")]
    [SerializeField] protected Image item_image;
    [SerializeField] protected Sprite current_item_sprite;
    public Sprite ItemSprite => current_item_sprite;
    // public Item Item => Stack != null && Stack.Quantity > 0 ? Stack.Items[0] : null;
    // public event Action<List<Item>> OnItemChanged = delegate { };
    
    
    // GETTERS
    private UI_ItemPool _item_pool;
    public UI_ItemPool UI_ItemPool
    {
        get
        {
            if (_item_pool != null) { return _item_pool; }
            _item_pool = GetComponentInParent<UI_ItemPool>(includeInactive: true);
            return _item_pool;
        }
    }
    // public ItemPool ItemPool => UI_ItemPool?.pool;
    // public Inventory Inventory => ItemPool?.Inventory;
    /* private ItemStack _item_stack;
    public ItemStack ItemStack
    {
        get
        {
            if (_item_stack != null) { return _item_stack; }
            _item_stack = ItemPool?.GetStackOfItem(Item);
            return _item_stack;
        }
    } */

    // AWAKE
    public virtual void Init()
    {
        item_image = transform.Find("item").GetComponent<Image>();
    }

    // GETTERS
    /* public bool CanStore(Item item)
    {
        return CanStore(new List<Item> { item });
    }
    public bool CanStore(List<Item> items)
    {
        // todo we already have this method in UI_ItemPool, shouldn't we use only one ?

        // checks if we can add the item to the slot (store or stack it on the slot)
        Item item = items.Count > 0 ? items[0] : null;

        // we check if the item is valid
        if (item == null) { return false; }

        // if we don't have any item, we can store it
        if (Quantity == 0) { return true; }

        // here we have already an item
        // we check if both items are stackable
        if (!item.Stackable || !Stackable) { return false; }

        // we check if the item is the same
        if (Reference != item.Reference) { return false; }

        // we check if they are modules and have the same upgrades
        if (item is Module module && Item is Module current_module)
        {
            if (!module.HasSameUpgrades(current_module)) { return false; }
        }

        // we check if the item is full
        if (Quantity + items.Count > MaxQty) { return false; }

        // we can stack the item !!
        return true;
    } */
    /* public bool CanStore(List<Item> items)
    {
        // we get our ItemStack from our UI_ItemPool
        ItemStack stack = ItemPool?.GetStackOfItem(items[0]);
        if (stack == null) { return false; }
        
        return stack.CanAdd(items);
    } */
    /* public List<Item> GetItems()
    {
        // we return a copy of the items list
        return Stack.Items;
    } */


    // STORE / UNSTORE / CLEAR / SWITCH ITEMS
    public virtual void Attach(ItemStack stack)
    {
        if (Stack != null) { Detach(); }

        this.Stack = stack;

        // we DO have items, so we update the sprite
        update_ui();

        // we update the UI
        update_ui_qty();
        
        // subscribe to stack callbacks
        // stack.OnChanged += update_ui;
        // stack.OnChanged += update_ui_qty;
    }
    public virtual void Detach()
    {
        if (Stack == null) { return; }

        // unsubscribe from stack callbacks
        // Stack.OnChanged -= update_ui;
        // Stack.OnChanged -= update_ui_qty;

        Stack = null;

        // we update the UI
        ClearUI();
        update_ui_qty();
    }
    /* public virtual void SwitchItems(ItemStack new_stack)
    {
        Detach();
        Attach(new_stack);
    } */

    // UI
    protected void update_ui_qty()
    {
        int quantity = Stack != null ? Stack.Quantity : 0;

        // we check if we have a quantity text
        if (quantity_text == null) { return; }

        // we update the text
        quantity_text.text = quantity.ToString();

        // we show or hide the text
        quantity_text.gameObject.SetActive(quantity > 1);
    }
    protected virtual void update_ui()
    {
        if (Stack == null || Stack.Quantity == 0)
        {
            ClearUI();
            return;
        }

        // on charge le sprite de l'image
        current_item_sprite = ItemBank.Instance.GetSprite(Stack.Item);
        set_ui(current_item_sprite);

        // on change le nom du prefab
        name = "ui_" + Stack.ItemReference;

        // on enable le slot
        Enable();
    }
    protected virtual void set_ui(Sprite sprite)
    {
        // Debug.Log("(UI_Item) setting UI for item_image :" + item_image + " with sprite " + (sprite != null ? sprite.name : "null"));
        if (item_image == null)
        {
            Debug.LogWarning($"(UI_Item) item_image of {name} is null, cannot set UI");
            return;
        }
        item_image.sprite = sprite;
        item_image.color
                = sprite != null
                ? new Color(1, 1, 1, 1)
                : new Color(0, 0, 0, 0);
        if (sprite == null){ return; }

        // on calcule la taille de l'image
        RectTransform rt = item_image.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
    }
    protected void ClearUI()
    {
        // on change le sprite de l'image
        set_ui(null);
        current_item_sprite = null;

        // on change le nom du prefab
        name = "ui_empty";

        // on disable le slot
        // if (UI_ItemPool != null && UI_ItemPool.DoNotDisableEmptySlots) { return; }
        Disable();
    }
    /* protected void handle_item_reference_changed(Item item)
    {
        // we check if this is the only one we have we simply change the ui
        if (Quantity == 1)
        {
            setItem(item);
            // on remet le sprite du slot si on est hovered
            image.sprite = Hovered ? hover_sprite : base_sprite;

            // on invoke les events
            OnItemChanged?.Invoke(items);
            UI_ItemPool?.NotifyPoolChanged(this);
            return;
        }

        // else we try to make the ui_inventory to regrab this item
        // bool regrabbed = Inventory?.ui?.UI_Regrab(item) ?? false;
        // if (regrabbed) { return; }

        // else we could not regrab it so we simulate a PointerDropped to make it drop
        drop_item(item);
    } */

    // ON POINTER
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we check if we have an item
        if (Stack == null || Stack.Quantity == 0) { return; }
        if (log) { Debug.Log("OnPointerClick on " + gameObject.name); }

        // we check if the item is an usable
        if (Stack.Item is Usable usable)
        {
            // usable.Use(Inventory.capable);
            UI_Manager.Instance.SwitchToHUD();
            return;
        }

        // we check if the item is an inspectable
        if (Stack.Item is Inspectable inspectable) { inspectable.Inspect(); }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        set_ui(current_item_sprite);
    }

    // ON POINTER DROPPED
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
        Inventory inventory = item.transform.parent.GetComponent<Inventory>();

        // on cherche l'inventory qui reçoit l'item
        Inventory inventory_to_drop = inventory.GetInteractingInventory();

        // we drop the item in the other inventory
        if (inventory_to_drop != null)
        {
            inventory_to_drop.Grab(item);
            // OnItemChanged?.Invoke(this.items);
            // UI_ItemPool?.NotifyPoolChanged(this);
            return;
        }

        // we don't have an inventory to drop so we drop on the ground
        // we check if we have a DropCapacity
        DropCapacity dropper = inventory.capable.GetCapacity<DropCapacity>();
        if (dropper != null)
        {
            dropper.Select(item);
            dropper.random_direction = true;
            dropper.Use(inventory.capable);
            // OnItemChanged?.Invoke(this.items);
            // UI_ItemPool?.NotifyPoolChanged(this);
            dropper.random_direction = false;
        }
        else
        {
            // the inventory simply drops the item (we may be in a chest)
            inventory.Drop(item);
            // OnItemChanged?.Invoke(this.items);
            // UI_ItemPool?.NotifyPoolChanged(this);
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
    public virtual void OnPointerDragEnter(UI_Item moving_ui_item)
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
        if (Stack.Quantity == 0) { switch_icon = ItemBank.Instance.GetUI_Icon("move"); }
        set_ui(switch_icon);
    }
    public void OnPointerDragUp()
    {
        if (log) { Debug.Log("OnPointerDragUp on " + gameObject.name); }
        OnPointerEnter(null);
    }
}