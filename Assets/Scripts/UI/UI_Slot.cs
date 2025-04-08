using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Slot : MonoBehaviour, I_UI_Slot, IPointerDownHandler
{

    // hover
    public bool is_hovered { get; set; }
    public bool is_disabled { get; set; }

    [Header("Sprites")]
    public Sprite base_sprite;
    public Sprite hover_sprite;
    public Sprite down_sprite;
    public Sprite disabled_sprite;

    [Header("Debug")]
    public bool debug = false;

    // DISABLE
    public void Enable()
    {
        is_disabled = false;

        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;
    }
    public void Disable()
    {
        is_disabled = true;

        // on change le sprite du slot
        GetComponent<Image>().sprite = disabled_sprite;
    }

    // POINTER HANDLERS
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        // on change le sprite du slot
        GetComponent<Image>().sprite = hover_sprite;

        // on met à jour le fait qu'on est survolé
        is_hovered = true;
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        is_hovered = false;
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        if (debug) { Debug.Log("OnPointerDown on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = down_sprite;
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        // on change le sprite du slot
        GetComponent<Image>().sprite = is_hovered ? hover_sprite : base_sprite;

        /* // we check if we have an item
        if (item == null) { return; }
        if (debug) { Debug.Log("OnPointerClick on " + gameObject.name); }

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
        } */
    }


    // ! DEPRECATED

    public string getDescription()
    {
        throw new NotImplementedException();
    }
    public bool shouldDescriptionBeShown()
    {
        throw new NotImplementedException();
    }
}