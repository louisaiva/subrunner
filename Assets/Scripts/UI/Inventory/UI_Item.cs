using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Item : UI_Slot
{

    [Header("Item Reference")]
    private List<Item> items = new List<Item>();
    public string Reference { get => items.Count > 0 ? items[0].Reference : ""; }


    [Header("Item Stacking")]
    public TextMeshProUGUI quantity_text;
    public bool Stackable { get => MaxQty > 1; }
    public int MaxQty { get => items.Count > 0 ? items[0].MaxQty : 1; }
    public int Quantity { get => items.Count; }

    [Header("Components")]
    public ItemBank bank;

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
        if (Quantity == 1) { set_ui(item); }

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

    // ON POINTER CLICK
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we check if we have an item
        if (Quantity == 0) { return; }
        if (debug) { Debug.Log("OnPointerClick on " + gameObject.name); }

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
        }
        else
        {
            // the inventory simply drops the item (we may be in a chest)
            inventory.Drop(item);
        }
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
    private void set_ui(Item item)
    {
        // we check if we have the bank
        if (bank == null) { bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>(); }

        // on récupère le sprite de l'item
        Sprite sprite = bank.GetSprite(item.Reference);

        // on change le sprite de l'image
        Image img = transform.Find("item").GetComponent<Image>();
        img.sprite = sprite;
        img.color = new Color(1, 1, 1, 1);

        // on calcule la taille de l'image
        RectTransform rt = img.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);

        // on change le nom du prefab
        name = "ui_" + Reference;

        // on enable le slot
        Enable();
    }
    public void ClearUI()
    {
        // on change le sprite de l'image
        Image img = transform.Find("item").GetComponent<Image>();
        img.sprite = null;
        img.color = new Color(0, 0, 0, 0);

        // on change le nom du prefab
        name = "ui_empty";

        // on disable le slot
        Disable();
    }




    // ! DEPRECATED
    public void resetHoover()
    {
        if (!is_hovered) return;

        OnPointerExit(null);
    }
    public void setItem(OldItem item) {}
    public void setUIInventory(GameObject ui_inventory) {}

}