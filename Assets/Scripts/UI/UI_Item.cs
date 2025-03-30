using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Item : MonoBehaviour, I_UI_Slot
{

    [Header("Item Reference")]
    public string reference;

    // hoover
    public bool is_hoovered { get; set; }
    public Action<InputAction.CallbackContext> ActivateCallback
    {
        get
        {
            return ctx => OnPointerClick(null);
        }
    }

    [Header("Sprites")]
    public Sprite base_sprite;
    public Sprite hoover_sprite;

    [Header("UI_Inventory")]
    public UI_Inventory ui_inventory;

    // unity functions
    protected void Awake()
    {
        // on récupère les sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/item_slots");
        base_sprite = sprites[0];
        hoover_sprite = sprites[1];
    }

    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        // on change le sprite du slot
        GetComponent<Image>().sprite = hoover_sprite;

        // on met à jour le fait qu'on est survolé
        is_hoovered = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        is_hoovered = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("clicked on " + gameObject.name);
    }

    // reset hoover
    public void resetHoover()
    {
        if (!is_hoovered) return;

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