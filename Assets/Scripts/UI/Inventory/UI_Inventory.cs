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
    [SerializeField] private ItemBank bank;
    public Inventory inventory;

    // AWAKE
    void Awake()
    {
        // on récupère le bank
        if (bank == null) { bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();}
    }

    public void Init()
    {
        // si on a pas de bank, on a pas Awake() peut-etre tout simplement qu'on est désactivé de base
        // ce qui est plutôt commun chez les UI
        if (bank == null) { Awake(); }        


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

    }

    // SHOW / HIDE
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
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