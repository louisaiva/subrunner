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

    [Header("UI_Item Pools")]
    public List<UI_ItemPool> pools = new List<UI_ItemPool>();

    [Header("Components")]
    [SerializeField] private UI_XboxNavigator navigator;
    public Inventory inventory;

    [Header("Logs")]
    [SerializeField] private bool debug = false;

    public void Init()
    {
        // on récupère les composants
        navigator = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        // si on a pas d'inventory, il y a un problème
        if (inventory == null)
        {
            try
            {
                Debug.LogError("(UI_Inventory) missing inventory on " + transform.parent.parent.parent.name +
                            ", you need to set it in the inspector");

                return;
            }
            catch
            {
                Debug.LogError("(UI_Inventory) missing inventory on " + name);
            }
        }
        // we check if we have some pools, otherwise we set ourself as the pool
        else if (pools.Count == 0)
        {
            try
            {
                Debug.LogError("(UI_Inventory) no pool found on " + transform.parent.parent.parent.name +
                ", please set at least one pool in the inspector");
                return;
            }
            catch
            {
                Debug.LogError("(UI_Inventory) no pool found on " + name);
            }
        }

        // on initialise les pools
        foreach (UI_ItemPool pool in pools)
        {
            if (pool == null)
            {
                Debug.LogWarning("(Inventory) " + name + $" has a null UI_ItemPool : {pool}, skipping initialization");
                continue;
            } // skip null UIs
            pool.Init(this);
        }
    }

    // SHOW / HIDE
    public void Show()
    {
        gameObject.SetActive(true);

        // we enable the navigator if we are not the perso quick inventory
        if (transform.parent.name != "hud")
        {
            if (!navigator) { Init(); }
            navigator.Enable(this, true);
        }
    }
    public void Hide()
    {
        // we unhover all the slots
        foreach (UI_ItemPool pool in pools)
        {
            foreach (Transform child in pool.transform)
            {
                UI_Item ui_item = child.GetComponent<UI_Item>();
                if (ui_item == null) { continue; }
                ui_item.OnPointerExit(null);
            }
        }

        gameObject.SetActive(false);

        // we enable the navigator if we are not the perso quick inventory
        if (transform.parent.name != "hud")
        {
            // we disable the navigator
            navigator.Disable(this);
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
        foreach (UI_ItemPool pool in pools)
        {
            foreach (Transform child in pool.transform)
            {
                // we check if the slot is a UI_Item
                UI_Item ui_item = child.GetComponent<UI_Item>();
                if (ui_item == null) { continue; }

                slots.Add(child.gameObject);

                // we update the position to the first slot
                if (position == Vector2.negativeInfinity)
                {
                    position = child.position;
                }
            }
        }
        return slots;
    }
    public bool IsYourSlot(GameObject slot)
    {
        // we check if the slot is in the inventory
        foreach (UI_ItemPool pool in pools)
        {
            foreach (Transform child in pool.transform)
            {
                if (child.gameObject == slot) { return true; }
            }
        }
        return false;
    }

    // GRAB
    public virtual bool UI_Grab(Item item)
    {
        foreach (UI_ItemPool pool in pools)
        {
            // we try to grab the item in the pool
            bool grabbed = pool.Grab(item);
            if (grabbed)
            {
                if (debug) { Debug.Log("(UI_Inventory) grabbed " + item.Reference + " in " + pool.name); }
                return true;
            }
        }

        // if we are here, no pool could take the item
        if (debug)
        {
            Debug.LogWarning("(UI_Inventory) no pool could take the item " + item.Reference +
        " in " + inventory.capable.name + "'s ui_inventory, maybe they are full or the item is incompatible");
        }

        return false;
    }
    public virtual bool UI_Drop(Item item)
    {
        // we go through the children
        foreach (UI_ItemPool pool in pools)
        {
            // we try to drop the item in the pool
            bool dropped = pool.Drop(item);
            if (dropped)
            {
                if (debug) { Debug.Log("(UI_Inventory) dropped " + item.Reference + " in " + pool.name); }
                return true;
            }
        }

        if (debug)
        {
            Debug.LogWarning("(UI_Inventory) no pool could drop the item " + item.Reference +
        " in " + inventory.capable.name + "'s ui_inventory, please check the pools and the item type");
        }

        return false;
    }
}