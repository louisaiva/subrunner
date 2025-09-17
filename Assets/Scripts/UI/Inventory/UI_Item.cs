using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Item : UI_Slot
{

    public Sprite drag_sprite;
    public Sprite drag_hover_sprite;

    [Header("Item Reference")]
    protected List<Item> items = new List<Item>();
    public string Reference { get => items.Count > 0 ? items[0].Reference : ""; }


    [Header("Item Stacking")]
    public TextMeshProUGUI quantity_text;
    public bool Stackable { get => MaxQty > 1; }
    public virtual int MaxQty { get => items.Count > 0 ? items[0].MaxQty : 1; }
    public int Quantity { get => items.Count; }

    [Header("Components")]
    [SerializeField] public ItemBank bank;
    [SerializeField] protected Image item_image;
    [SerializeField] protected Sprite current_item_sprite;
    public Sprite ItemSprite => current_item_sprite;
    public UI_ItemPool ItemPool => transform.parent.GetComponent<UI_ItemPool>();
    public Inventory Inventory => ItemPool?.UI_Inventory?.Inventory;
    public Item Item => items.Count > 0 ? items[0] : null;
    public event Action<List<Item>> OnItemChanged = delegate { };

    // AWAKE
    public virtual void Init()
    {
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();
        item_image = transform.Find("item").GetComponent<Image>();
    }

    // STORE ITEM
    public bool CanStore(Item item)
    {
        return CanStore(new List<Item> { item });
    }
    public bool CanStore(List<Item> items)
    {
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
    }
    public bool Store(Item item)
    {
        // we check if we can store the item
        if (!CanStore(item)) { return false; }

        if (log) { Debug.Log($"(UI_Item) Storing item {item.Reference} in {name}"); }

        // we add the item to the slot
        items.Add(item);

        // we update the UI
        update_ui_qty();

        // we check if it is the first item we store
        if (Quantity == 1) { setItem(item); }

        OnItemChanged?.Invoke(items);
        ItemPool?.NotifyPoolChanged(this);

        return true;
    }
    public bool Unstore(Item item)
    {
        // we check if we can unstore the item
        if (!items.Contains(item)) { return false; }

        // we remove the item from the slot
        items.Remove(item);

        // we update the UI
        update_ui_qty();

        // we check if we have no more items in the slot
        if (Quantity == 0) { ClearUI(); }

        OnItemChanged?.Invoke(items);
        ItemPool?.NotifyPoolChanged(this);
        return true;
    }
    public void Clear()
    {
        // we clear the items
        items.Clear();

        // we update the UI
        update_ui_qty();

        // we clear the UI
        ClearUI();
    }

    // ITEM SWITCHING
    public virtual void SwitchItems(List<Item> items, bool items_moved = true)
    {
        Clear();

        // we add the items to the slot
        if (items.Count > 0)
        {
            this.items.AddRange(items);
            setItem(items[0]);
        }

        // we update the UI
        update_ui_qty();

        OnItemChanged?.Invoke(this.items);
        ItemPool?.NotifyPoolChanged(this);
    }
    public List<Item> GetItems()
    {
        // we return a copy of the items list
        return new List<Item>(items);
    }

    // UI
    protected void update_ui_qty()
    {
        // we check if we have a quantity text
        if (quantity_text == null) { return; }

        // we update the text
        quantity_text.text = Quantity.ToString();

        // we show or hide the text
        quantity_text.gameObject.SetActive(Quantity > 1);
    }
    protected virtual void setItem(Item item)
    {
        // on charge le sprite de l'image
        current_item_sprite = bank.GetSprite(item);
        set_ui(current_item_sprite);

        // on change le nom du prefab
        name = "ui_" + Reference;

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
        if (ItemPool != null && ItemPool.DoNotDisableEmptySlots) { return; }
        Disable();
    }


    // ON POINTER
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        update_description();
    }
    protected void update_description()
    {
        // we check if the current ui_pool has a descriptor or not
        if (UI_Manager.Instance.CurrentPool != "inventory") { return; }

        // we get the descriptor
        UI_InventoryMenu menu = UI_Manager.Instance.GetPool("inventory").GetComponent<UI_InventoryMenu>();
        menu.Descriptor.SetDescription(this);
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we check if we have an item
        if (Quantity == 0) { return; }
        if (log) { Debug.Log("OnPointerClick on " + gameObject.name); }

        // we check if the item is an usable
        if (Item != null && Item is Usable usable)
        {
            usable.Use(Inventory.capable);
            UI_Manager.Instance.SwitchTo("hud");
        }
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
        if (Quantity == 0) { return; }
        if (log) { Debug.Log("OnPointerDropped on " + gameObject.name); }

        // we get the item
        Item item = items[0];

        // on récupère l'inventory qui drop l'item
        Inventory inventory = item.transform.parent.GetComponent<Inventory>();

        // on cherche l'inventory qui reçoit l'item
        Inventory inventory_to_drop = inventory.GetInteractingInventory();

        // we drop the item in the other inventory
        if (inventory_to_drop != null)
        {
            inventory_to_drop.Grab(item);
            OnItemChanged?.Invoke(this.items);
            ItemPool?.NotifyPoolChanged(this);
            return;
        }

        // we don't have an inventory to drop so we drop on the ground
        // we check if we have a DropCapacity
        DropCapacity dropper = inventory.capable.GetCapacity<DropCapacity>();
        if (dropper != null)
        {
            dropper.Select(item);
            dropper.random_direction = true;
            inventory.capable.Do("drop");
            OnItemChanged?.Invoke(this.items);
            ItemPool?.NotifyPoolChanged(this);
            dropper.random_direction = false;

            // we switch back to hud
            // UI_Manager.Instance.SwitchTo("hud");
        }
        else
        {
            // the inventory simply drops the item (we may be in a chest)
            inventory.Drop(item);
            OnItemChanged?.Invoke(this.items);
            ItemPool?.NotifyPoolChanged(this);
        }
    }


    // ON POINTER DRAG
    public void OnPointerDragDown()
    {
        // check if disabled
        if (is_disabled) { return; }
        if (log) { Debug.Log("OnPointerDragDown on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = drag_sprite;
    }
    public virtual void OnPointerDragEnter(UI_Item moving_ui_item)
    {
        // check if disabled
        if (is_disabled) { return; }
        if (log) { Debug.Log("OnPointerDragEnter on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = drag_hover_sprite;


        // on met un icon de switch à la place de l'item
        Sprite switch_icon = CanStore(moving_ui_item.Item)
            ? bank.GetUI_Icon("merge")
            : bank.GetUI_Icon("switch");
        set_ui(switch_icon);
    }
    public void OnPointerDragUp()
    {
        if (log) { Debug.Log("OnPointerDragUp on " + gameObject.name); }
        OnPointerEnter(null);
    }

    private void OnDrawGizmos()
    {
        Vector3 position = GetComponent<RectTransform>().TransformPoint(GetComponent<RectTransform>().rect.center);
        position = Camera.main.ScreenToWorldPoint(position);

        // we draw 2 circles to show if bank & item_image are shown
        if (bank != null) { Gizmos.color = Color.green; }
        else { Gizmos.color = Color.red; }
        Gizmos.DrawWireSphere(position - new Vector3(0.2f, 0f, 0f), 0.1f);
        if (item_image != null) { Gizmos.color = Color.green; }
        else { Gizmos.color = Color.red; }
        Gizmos.DrawWireSphere(position + new Vector3(0.2f, 0f, 0f), 0.1f);
    }
}