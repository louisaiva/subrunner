using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// UI_Inventory is the lowest UI representation of the Inventory.
/// it is never triggered directly, but Capacities of the Capable storing items
/// activate it & update it.
/// </summary>

public class UI_Inventory : MonoBehaviour, I_UI_Slottable
{
    
    [Header("Components")]
    [SerializeField] private ItemBank bank;

    public Action<InputAction.CallbackContext> CancelCallback => throw new NotImplementedException();

    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        throw new NotImplementedException();
    }



    // GRAB
    public void Grab(Item item)
    {
        // we instantiate the item
        GameObject ui_item = bank.CreateUI_Item(item);
        ui_item.transform.SetParent(transform);
    }
    public void Drop(Item item)
    {
        // we destroy the item
        foreach (Transform child in transform)
        {
            if (child.GetComponent<UI_Item>().item == item)
            {
                Destroy(child.gameObject);
                return;
            }
        }
    }
}