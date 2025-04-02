using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// UI_Inventory is the lowest UI representation of the Inventory.
/// it is never triggered directly, but is showed by the Capacities & updated by the Inventory.
/// </summary>

public class UI_Inventory : MonoBehaviour, I_UI_Slottable
{
    
    [Header("Components")]
    [SerializeField] private UI_XboxNavigator navigator;
    [SerializeField] private ItemBank bank;
    public Inventory inventory;

    public void Init()
    {
        // on récupère les composants
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();
        navigator = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();


        // si on a pas d'inventory, il y a un problème
        if (inventory == null)
        {
            Debug.LogError("(UI_Inventory) missing inventory on " + transform.parent.parent.parent.name +
            ", you need to set it in the inspector");
            return;
        }

        // si l'inventory est non scalable, on affiche les slots vides
        if (!inventory.Scalable)
        {
            for (int i = inventory.Items.Count; i < inventory.MaxItems; i++)
            {
                // we create empty slots
                GameObject ui_item = bank.CreateUI_Item();
                ui_item.transform.SetParent(transform);

                // reset the scale to 1
                ui_item.transform.localScale = Vector3.one;
            }
        }

        // on regarde si on est l'ui_inventory de l'inventaire du joueur
        // et dans ce cas on active tout de suite xbox_navigator
        /* if (transform.parent.parent.name == "ui")
        {
            // on active le navigator
            navigator.Enable(this);
        } */

    }

    // SHOW / HIDE
    public void Show()
    {
        gameObject.SetActive(true);

        // we enable the navigator if we are not the perso quick inventory
        if (transform.parent.name != "hud")
        {
            navigator.Enable(this);
            /* GameObject perso_quick_ui = GameObject.Find("/ui/hud/perso_quick_inventory");
            if (perso_quick_ui == null)
            {
                Debug.LogError("(UI_Inventory) could not find the perso quick inventory, please check the hierarchy (should be in /ui/hud/perso_quick_inventory)");
                return;
            }
            navigator.Enable(perso_quick_ui.GetComponent<UI_Inventory>()); */
        }
    }
    public void Hide()
    {
        // we unhover all the slots
        foreach (Transform child in transform)
        {
            UI_Item ui_item = child.GetComponent<UI_Item>();
            if (ui_item == null) { continue; }
            ui_item.OnPointerExit(null);
        }

        gameObject.SetActive(false);

        // we enable the navigator if we are not the perso quick inventory
        if (transform.parent.name != "hud")
        {
            // we disable the navigator
            navigator.Disable(this);
            /* GameObject perso_quick_ui = GameObject.Find("/ui/hud/perso_quick_inventory");
            if (perso_quick_ui == null)
            {
                Debug.LogError("(UI_Inventory) could not find the perso quick inventory, please check the hierarchy (should be in /ui/hud/perso_quick_inventory)");
                return;
            }
            navigator.Disable(perso_quick_ui.GetComponent<UI_Inventory>()); */
        }
    }
    public void Toggle()
    {
        // we check if the inventory is already shown
        if (gameObject.activeSelf)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    // SLOTTABLE
    public Action<InputAction.CallbackContext> CancelCallback => throw new NotImplementedException();
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        List<GameObject> slots = new List<GameObject>();
        Vector2 position = Vector2.negativeInfinity;
        foreach (Transform child in transform)
        {
            // checks if the slot is disabled
            if (child.GetComponent<UI_Item>().is_disabled) { continue; }
            slots.Add(child.gameObject);

            // we update the position
            if (position == Vector2.negativeInfinity)
            {
                position = child.position;
            }
        }
        return slots;
    }
    public bool IsYourSlot(GameObject slot)
    {
        // we check if the slot is in the inventory
        foreach (Transform child in transform)
        {
            if (child.gameObject == slot) { return true; }
        }
        return false;
    }

    // GRAB
    public void UI_Grab(Item item)
    {
        // we check if we have a scalable inventory
        if (inventory.Scalable)
        {
            // we create the item
            GameObject ui_slot = bank.CreateUI_Item(item);
            ui_slot.transform.SetParent(transform);

            // reset the scale to 1
            ui_slot.transform.localScale = Vector3.one;
            return;
        }

        // if we are here, we have a non scalable inventory
        // we go through the children to find an empty slot
        foreach (Transform ui_slot in transform)
        {
            // we check if the slot is empty
            UI_Item ui_item = ui_slot.GetComponent<UI_Item>();
            if (ui_item.item != null) { continue; }

            // we set the item
            bank.SetUI_Item(ui_item, item);
            return;
        }
    }
    public void UI_Drop(Item item)
    {
        // we go through the children
        foreach (Transform ui_slot in transform)
        {
            // we check if the item is the one we want to drop
            if (ui_slot.GetComponent<UI_Item>().item != item) { continue; }
            
            // if we are here, we have the item
            // we check if the inventory is scalable
            if (inventory.Scalable)
            {
                // we destroy the item
                Destroy(ui_slot.gameObject);
                return;
            }
            else
            {
                // we clear the item
                bank.ClearUI_Item(ui_slot.GetComponent<UI_Item>());
                return;
            }
        }
    }
}