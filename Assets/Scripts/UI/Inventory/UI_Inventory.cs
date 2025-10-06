using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// UI_Inventory is the highest UI representation of the Inventory.
/// it is never triggered directly, but is showed by the Capacities & updated by the Inventory.
/// </summary>

public class UI_Inventory : MonoBehaviour, I_UI_Slottable
{

    [Header("UI_Item Pools")]
    public List<UI_ItemPool> pools = new List<UI_ItemPool>();

    [Header("Components")]
    public Inventory Inventory;

    [Header("Logs")]
    [SerializeField] protected bool log = false;

    public void Init()
    {
        // we check if we have some pools, otherwise we set ourself as the pool
        if (pools.Count == 0)
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
    public void Refresh()
    {
        // on clear les pools
        foreach (UI_ItemPool pool in pools)
        {
            if (pool == null) { continue; } // skip null UIs
            pool.DestroyAllSlots();
        }

        if (Inventory == null)
        {
            if (log) { Debug.LogWarning("(UI_Inventory) " + name + " has no Inventory, cannot refresh"); }
            return;
        }

        // on récupère tous les items de l'Inventaire et on les fait grab si possible par nous mêmes
        foreach (Item item in Inventory.Items)
        {
            bool grabbed = UI_Grab(item);
            if (!grabbed)
            {
                Debug.LogWarning("(UI_Inventory) could not grab item " + item.Reference + " in " + name +
                ", maybe the pools are full or the item is incompatible");
            }
        }
    }

    // SHOW / HIDE
    public virtual async void Show()
    {
        gameObject.SetActive(true);
        await Task.Yield(); // wait for the next frame to ensure the UI is active

        
        UI_XboxNavigator.Instance.Enable(this, true);
    }
    public virtual void Hide()
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

        UI_XboxNavigator.Instance.Disable(this);
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

    // GRAB
    public virtual bool UI_Grab(Item item)
    {
        foreach (UI_ItemPool pool in pools)
        {
            // we try to grab the item in the pool
            bool grabbed = pool.Grab(item);
            if (grabbed)
            {
                if (log) { Debug.Log("(UI_Inventory) grabbed " + item.Reference + " in " + pool.name); }
                return true;
            }
        }

        // if we are here, no pool could take the item
        if (log)
        {
            Debug.LogWarning("(UI_Inventory) no pool could take the item " + item.Reference +
        " in " + Inventory.capable.name + "'s ui_inventory, maybe they are full or the item is incompatible");
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
                if (log) { Debug.Log("(UI_Inventory) dropped " + item.Reference + " in " + pool.name); }
                return true;
            }
        }

        if (log)
        {
            Debug.LogWarning("(UI_Inventory) no pool could drop the item " + item.Reference +
        " in " + Inventory.capable.name + "'s ui_inventory, please check the pools and the item type");
        }

        return false;
    }

    // ITEM RULE
    public string ItemRule
    {
        get
        {
            // todo concaten all pools' item rules
            if (pools.Count > 0)
            {
                // we return the first pool's rule
                return pools[0].item_rule;
            }
            return "";
        }
    }






    // SLOTTABLE
    public Action<InputAction.CallbackContext> CancelCallback => throw new NotImplementedException();
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        if (log) { Debug.Log($"(UI_Inventory) {name} getting slots"); }
        List<GameObject> slots = new List<GameObject>();
        Vector2 position = Vector2.negativeInfinity;
        foreach (UI_ItemPool pool in pools)
        {
            foreach (Transform child in pool.transform)
            {
                // we check if the ui_slot is enabled
                if (!child.gameObject.activeSelf) { continue; }

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
    public Vector2 SavedPosition { get => new Vector2(0f, Screen.height); }
}