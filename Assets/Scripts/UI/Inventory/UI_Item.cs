using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Item : UI_Slot
{

    public Sprite drag_sprite;
    public Sprite drag_hover_sprite;

    [Header("Item Reference")]
    private List<Item> items = new List<Item>();
    public string Reference { get => items.Count > 0 ? items[0].Reference : ""; }


    [Header("Item Stacking")]
    public TextMeshProUGUI quantity_text;
    public bool Stackable { get => MaxQty > 1; }
    public int MaxQty { get => items.Count > 0 ? items[0].MaxQty : 1; }
    public int Quantity { get => items.Count; }

    [Header("Components")]
    [SerializeField] public ItemBank bank;
    [SerializeField] private Image item_image;
    [SerializeField] private Sprite current_item_sprite;
    public UI_ItemPool ItemPool => transform.parent.GetComponent<UI_ItemPool>();
    public Item Item => items.Count > 0 ? items[0] : null;

    // AWAKE
    public void Init(ItemBank bank)
    {
        this.bank = bank;
        item_image = transform.Find("item").GetComponent<Image>();
    }

    // STORE ITEM
    private bool CanStore(Item item)
    {
        // checks if we can add the item to the slot (store or stack it on the slot)

        // we check if the item is valid
        if (item == null) { return false; }

        // if we don't have any item, we can store it
        if (Quantity == 0) { return true; }

        // here we have already an item
        // we check if both items are stackable
        if (!item.Stackable || !Stackable) { return false; }

        // we check if the item is the same
        if (Reference != item.Reference) { return false; }

        // we check if the item is full
        if (Quantity >= MaxQty) { return false; }

        // we can stack the item !!
        return true;
    }
    public bool Store(Item item)
    {
        // we check if we can store the item
        if (!CanStore(item)) { return false; }

        // we add the item to the slot
        items.Add(item);

        // we update the UI
        update_ui_qty();

        // we check if it is the first item we store
        if (Quantity == 1) { setItem(item); }

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

        return true;
    }

    // ITEM SWITCHING
    public void SwitchItems(List<Item> items)
    {
        // we clear the ui
        ClearUI();
        this.items.Clear();

        // we add the items to the slot
        if (items.Count > 0)
        {
            this.items.AddRange(items);
            setItem(items[0]);
        }

        // we update the UI
        update_ui_qty();
    }
    public List<Item> GetItems()
    {
        // we return a copy of the items list
        return new List<Item>(items);
    }

    // UI
    private void update_ui_qty()
    {
        // we check if we have a quantity text
        if (quantity_text == null) { return; }

        // we update the text
        quantity_text.text = Quantity.ToString();

        // we show or hide the text
        quantity_text.gameObject.SetActive(Quantity > 1);
    }
    public void setItem(Item item)
    {
        // on charge le sprite de l'image
        current_item_sprite = bank.GetSprite(item.Reference);
        set_ui(current_item_sprite);

        // on change le nom du prefab
        name = "ui_" + Reference;

        // on enable le slot
        Enable();
    }
    private void set_ui(Sprite sprite)
    {
        // Debug.Log("(UI_Item) setting UI for item_image :" + item_image + " with sprite " + (sprite != null ? sprite.name : "null"));
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
    public void ClearUI()
    {
        // on change le sprite de l'image
        set_ui(null);
        current_item_sprite = null;

        // on change le nom du prefab
        name = "ui_empty";

        // on disable le slot
        Disable();
    }


    // ON POINTER
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        string description = "";
        if (Quantity == 0) { description = "empty slot"; }
        else if (items.Count > 0) { description = items[0].Reference + "\n\n" + items[0].ItemDescription; }

        // on met à jour la description si y'en a une
        if (transform.parent.GetComponent<UI_ItemPool>() != null
        && transform.parent.GetComponent<UI_ItemPool>().Descriptor != null)
        {
            Description descriptor = transform.parent.GetComponent<UI_ItemPool>().Descriptor;
            descriptor.SetDescription(description);
        }
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we check if we have an item
        if (Quantity == 0) { return; }
        if (debug) { Debug.Log("OnPointerClick on " + gameObject.name); }

        // we check if this is a food item
        if (items.Count > 0 && items[0].GetComponent<Food>() != null)
        {
            Food food = items[0].GetComponent<Food>();

            // we make it eat by the capable
            // we get the EatCapacity
            EatCapacity eater = food.Holder.GetCapacity<EatCapacity>();
            if (eater == null) { return; }

            eater.SetFoodTarget(food);
            eater.Use(food.Holder);

            // we switch back to hud
            GameObject.Find("/ui").GetComponent<UI_Manager>().SwitchTo("hud");
            return;
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
        if (debug) { Debug.Log("OnPointerDropped on " + gameObject.name); }

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
            return;
        }

        // we don't have an inventory to drop so we drop on the ground
        // we check if we have a DropCapacity
        DropCapacity dropper = inventory.capable.GetCapacity<DropCapacity>();
        if (dropper != null)
        {
            dropper.Select(item);
            inventory.capable.Do("drop");

            // we switch back to hud
            GameObject.Find("/ui").GetComponent<UI_Manager>().SwitchTo("hud");
        }
        else
        {
            // the inventory simply drops the item (we may be in a chest)
            inventory.Drop(item);
        }
    }


    // ON POINTER DRAG
    public void OnPointerDragDown()
    {
        // check if disabled
        if (is_disabled) { return; }

        // on change le sprite du slot
        GetComponent<Image>().sprite = drag_sprite;
    }
    public void OnPointerDragEnter(UI_Item moving_ui_item)
    {
        // check if disabled
        if (is_disabled) { return; }

        // on change le sprite du slot
        GetComponent<Image>().sprite = drag_hover_sprite;


        // on met un icon de switch à la place de l'item
        Sprite switch_icon = (Item != null && Reference == moving_ui_item.Reference && Quantity < MaxQty)
            ? bank.GetUI_Icon("merge")
            : bank.GetUI_Icon("switch");
        set_ui(switch_icon);
    }
    public void OnPointerDragUp()
    {
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