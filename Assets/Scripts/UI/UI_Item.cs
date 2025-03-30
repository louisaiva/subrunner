using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Item : MonoBehaviour, I_UI_Slot
{

    [Header("Item Reference")]
    public Item item;

    // hover
    public bool is_hovered { get; set; }
    public Action<InputAction.CallbackContext> ActivateCallback
    {
        get
        {
            return ctx => OnPointerClick(null);
        }
    }

    [Header("Sprites")]
    public Sprite base_sprite;
    public Sprite hover_sprite;

    // unity functions
    protected void Awake()
    {
        // on récupère les sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/item_slots");
        base_sprite = sprites[0];
        hover_sprite = sprites[1];
    }

    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        // on change le sprite du slot
        GetComponent<Image>().sprite = hover_sprite;

        // on met à jour le fait qu'on est survolé
        is_hovered = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        is_hovered = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // on récupère l'inventory qui drop l'item
        item.transform.parent.GetComponent<Inventory>().Drop(item);
    }

    // reset hover
    public void resetHoover()
    {
        if (!is_hovered) return;

        OnPointerExit(null);
    }

    public string getDescription()
    {
        throw new NotImplementedException();
    }

    public bool shouldDescriptionBeShown()
    {
        throw new NotImplementedException();
    }

    // ! DEPRECATED
    public void setItem(OldItem item) {}
    public void setUIInventory(GameObject ui_inventory) {}
}