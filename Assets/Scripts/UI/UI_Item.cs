using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Item : MonoBehaviour, I_UI_Slot
{


    // callback
    private System.Action<InputAction.CallbackContext> activateCallback;
    public System.Action<InputAction.CallbackContext> ActivateCallback
    {
        get
        {
            return activateCallback;
        }
    }

    // hoover
    public bool is_hoovered { get; set; }

    public GameObject description_ui;

    // item
    public OldItem item;

    // ui_inventory
    public GameObject ui_inventory;

    // slot sprite
    public Sprite base_sprite;
    public Sprite hoover_sprite;


    // unity functions
    protected void Awake()
    {
        // on récupère le description_ui
        description_ui = GameObject.Find("/ui/hoover_description");

        // on récupère les sprites
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/item_slots");
        base_sprite = sprites[0];
        hoover_sprite = sprites[1];

        // define the callback
        activateCallback = ctx =>
        {
            OnPointerClick(null);
            Debug.Log("UI_Item : ActivateCallback !! Should we navigate to closest with Xbox Navigator ??");
        };
    }

    // main functions
    public void setItem(OldItem item)
    {
        this.item = item;
    }

    public void setUIInventory(GameObject ui_inventory)
    {
        this.ui_inventory = ui_inventory;
    }

    // getters
    public string getDescription()
    {
        string s = item.item_name + "\n\n" + item.item_description;
        return s;
    }

    public bool shouldDescriptionBeShown() {return true;}

    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().changeDescription(this);

        // on change le sprite du slot
        GetComponent<Image>().sprite = hoover_sprite;

        // on met à jour le fait qu'on est survolé
        is_hoovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().removeDescription(this);

        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        is_hoovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // on met à jour l'affichage
        description_ui.GetComponent<UI_HooverDescriptionHandler>().removeDescription(this);

        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on click
        if (ui_inventory.GetComponent<UI_OldInventory>() != null)
        {
            ui_inventory.GetComponent<UI_OldInventory>().clickOnItem(item);
        }
        else if (ui_inventory.GetComponent<UI_ChestInventory>() != null)
        {
            ui_inventory.GetComponent<UI_ChestInventory>().clickOnItem(item);
        }
        else
        {
            Debug.LogWarning("UI_Item : no UI_OldInventory or UI_ChestInventory found on " + ui_inventory.name);
        }
    }

    // reset hoover
    public void resetHoover()
    {
        if (!is_hoovered) return;

        OnPointerExit(null);
    }
}