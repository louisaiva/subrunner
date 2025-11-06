using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_ItemMover : MonoBehaviour
{
    private UI_Navigator manager;


    [Header("Moving Item")]
    [SerializeField] private UI_Item moving_ui_item = null;
    [SerializeField] private UI_Item potential_moving_item = null;
    public UI_Item MovingUIItem { get => moving_ui_item; }
    public bool IsMovingItem { get => moving_ui_item != null; }

    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_moving_items = false;
    [SerializeField] private bool log_receivable_slots = false;

    // START
    private void Start()
    {
        manager = GetComponent<UI_Navigator>();
    }

    // MOVING ITEM HIGH LEVEL
    public void StartMovingItem(UI_Item item)
    {
        if (item == null) { FinishMovingItem(); return; }

        // sets the moving item directly
        moving_ui_item = item;
        if (log) { Debug.Log($"(UI_ItemMover) started moving ui_item : {moving_ui_item.name}"); }

        // we call the drag down event
        moving_ui_item.OnPointerDragDown();

        // on met à jour les slots pour enable que les slots qui peuvent recevoir l'ui item
        enable_only_recevable_slots(moving_ui_item);
    }
    public void SetPotentialMovingItem(UI_Item item)
    {
        if (item == null) { FinishMovingItem(); return; }

        // sets the potential moving item
        potential_moving_item = item;
        if (log) { Debug.Log($"(UI_ItemMover) set potential moving item : {item.name}"); }

        // on met à jour les slots pour enable que les slots qui peuvent recevoir l'ui item
        enable_only_recevable_slots(potential_moving_item);
    }
    public void PotentiallyStartMovingItem()
    {
        // checks if we have a potential moving item
        if (potential_moving_item == null) { return; }

        // else we start moving item !
        moving_ui_item = potential_moving_item;
        potential_moving_item = null;

        // we call the drag down event
        moving_ui_item.OnPointerDragDown();
        if (log) { Debug.Log($"(UI_ItemMover) started moving ui_item : {moving_ui_item.name}"); }
    }
    public void FinishMovingItem()
    {
        // checks if we are moving an item
        if (moving_ui_item == null)
        {
            if (log) { Debug.Log($"(UI_ItemMover) no moving ui_item to move"); }
            disable_only_empty_slots(true);
            potential_moving_item = null;
            return;
        }

        // checks if the current slot is an ui_item and not the same as the moving item one
        UI_Item destination = manager.CurrentSlot as UI_Item;
        // UI_Slot saved_moving_ui_item = moving_ui_item;

        // on exit le slot si c pas la destination
        if (destination != moving_ui_item && destination != null)
        {
            moving_ui_item.OnPointerExit(null);

            // finally we move the items
            if (log) { Debug.Log($"(UI_ItemMover) moving ui_item : {moving_ui_item.name} --> {destination.name}"); }
            moveItems(destination);
        }

        // on relache le drag
        disable_only_empty_slots(true);
        moving_ui_item = null;
        potential_moving_item = null;

        // et on navigue vers la destination (seulement si on utilise le gamepad)
        if (destination != null) { manager.HoverSlot(destination, prevent_same_slot: false); }
        if (manager.Navigator is MouseNavigator mouse)
        {
            if (log) { Debug.Log($"(UI_ItemMover) mouse navigator detected, unhovering if hovered ui_item is not {(destination == null ? "null" : destination.name)}"); }
            mouse.UnhoverIfNotHovering(destination);
        }
    }



    // MOVING ITEM LOW LEVEL
    private void moveItems(UI_Item destination)
    {
        // on choisit le mode d'action qu'il faut pour echanger les items
        // entre moving_ui_item & destination

        // sinon on choisit en fonction des différentes situations
        if (destination != moving_ui_item && destination.CanStore(moving_ui_item.GetItems()))
        {
            merge_items(moving_ui_item, destination); // ce sont les mêmes items, on peut alors les merge ensemble
        }
        else if (destination is UI_Module ui_module
            && moving_ui_item is not UI_Module
            && moving_ui_item.Quantity > 1)
        {
            split_items(ui_module, moving_ui_item); // on split l'item
        }
        else if (moving_ui_item is UI_Module ui_module2
            && destination is not UI_Module
            && destination.Quantity > 1)
        {
            split_items(ui_module2, destination); // on split l'item
        }
        else { switch_items(moving_ui_item, destination); }
    }
    private void switch_items(UI_Item item1, UI_Item item2)
    {
        // on échange les items entre les deux UI_Items
        if (item1 == null || item2 == null) { return; }

        if (log_moving_items) { Debug.Log($"(UI_ItemMover) switching items between {item1.gameObject.name} and {item2.gameObject.name}"); }

        // on sauvegarde les items
        List<Item> items1 = item1.GetItems();
        List<Item> items2 = item2.GetItems();

        // on echange les items
        item1.SwitchItems(items2);
        item2.SwitchItems(items1);

        // on regarde si on est dans deux inventaires différents
        Inventory inventory1 = item1.Inventory;
        Inventory inventory2 = item2.Inventory;
        if (inventory1 == null || inventory2 == null)
        {
            if (log)
            {
                Debug.LogWarning($"(UI_ItemMover) switched items between {item1.gameObject.name} "
            + $"and {item2.gameObject.name} but at least one inventory is null : {inventory1?.capable.name} and {inventory2?.capable.name}");
            }
            return;
        }
        if (inventory1 == inventory2) { return; } // we stay inside the same inventory so no need to update Items's inventories

        List<UI_Inventory> uis_to_ignore = new List<UI_Inventory>() { item1.ItemPool.UI_Inventory, item2.ItemPool.UI_Inventory };

        // on met à jour les inventories des items
        foreach (Item item in items1)
        {
            inventory2.Grab(item, uis_to_ignore); // on ignore les ui_inventory parce qu'ils ont déjà été grab dans ces UI_Inventory
        }
        foreach (Item item in items2)
        {
            inventory1.Grab(item, uis_to_ignore); // pareil
        }
    }
    private void split_items(UI_Module ui_module, UI_Item ui_item)
    {
        if (log_moving_items) { Debug.Log($"(UI_ItemMover) splitting items between {ui_item.gameObject.name} and {ui_module.gameObject.name}"); }


        // on vérifie que y'a pas déjà un module installé (sinon ça va tout kc)
        // todo : faire en sorte que si un module est déjà installé il est juste drop dans l'inventaire et ça
        // todo : switch quand mm le 1er module
        if (ui_module.Item != null)
        {
            if (log)
            {
                Debug.LogWarning($"(UI_ItemMover) cannot split items from {ui_item.gameObject.name} to {ui_module.gameObject.name} because it already has a module installed.");
            }
            return;
        }

        // on récupère le 1er item de ui_item sous la forme d'une liste
        Item item_to_move = ui_item.Item;
        List<Item> remaining_items = ui_item.GetItems();
        remaining_items.Remove(item_to_move);

        // on echange les items
        ui_module.SwitchItems(new List<Item>() { item_to_move });
        ui_item.SwitchItems(remaining_items);

        // on regarde si on est dans deux inventaires différents
        Inventory ui_item_inv = ui_item.Inventory;
        Inventory ui_module_inv = ui_module.Inventory;
        if (ui_item_inv == null || ui_module_inv == null)
        {
            if (log)
            {
                Debug.Log($"(UI_ItemMover) switched items between {ui_item.gameObject.name} "
            + $"and {ui_module.gameObject.name} but at least one inventory is null : {ui_item_inv?.capable.name} and {ui_module_inv?.capable.name}");
            }
            return;
        }
        if (ui_item_inv == ui_module_inv) { return; } // we stay inside the same inventory so no need to update Items's inventories

        List<UI_Inventory> uis_to_ignore = new List<UI_Inventory>() { ui_item.ItemPool.UI_Inventory, ui_module.ItemPool.UI_Inventory };
        ui_module_inv.Grab(item_to_move, uis_to_ignore); // on ignore les ui_inventory parce qu'ils ont déjà été grab dans ces UI_Inventory
    }
    private void merge_items(UI_Item item1, UI_Item item2)
    {
        if (log_moving_items) { Debug.Log($"(UI_ItemMover) merging items between {item1.gameObject.name} and {item2.gameObject.name}"); }

        // on merge les items de item1 dans item2
        List<Item> items = item1.GetItems();
        List<Item> transfered_items = new List<Item>();
        while (item2.Store(items[0]))
        {
            transfered_items.Add(items[0]); // on ajoute l'item à la liste des items transférés
            items.RemoveAt(0);
            if (items.Count == 0) { break; } // si on a plus d'items on sort de la boucle
        }

        // on appelle SwitchItems() ce qui va mettre tout bien
        item2.SwitchItems(item2.GetItems()); // on clear item2 et on lui refile ses items
        item1.SwitchItems(items); // on clear item1 et on lui refile les items restants

        // on regarde si on est dans deux inventaires différents
        Inventory inventory1 = item1.Inventory;
        Inventory inventory2 = item2.Inventory;
        if (inventory1 == null || inventory2 == null)
        {
            if (log)
            {
                Debug.Log($"(UI_ItemMover) merged items between {item1.gameObject.name} "
            + $"and {item2.gameObject.name} but at least one inventory is null : {inventory1?.capable.name} and {inventory2?.capable.name}");
            }
            return;
        }
        if (inventory1 == inventory2) { return; } // we stay inside the same inventory so no need to update Items's inventories

        if (log)
        {
            Debug.Log($"(UI_ItemMover) merged items between {item1.ItemPool.UI_Inventory.name} "
            + $"and {item2.ItemPool.UI_Inventory.name} with {items.Count} items left in {item1.gameObject.name}");
        }

        // on met à jour les inventories des items
        List<UI_Inventory> uis_to_ignore = new List<UI_Inventory>() { item1.ItemPool.UI_Inventory, item2.ItemPool.UI_Inventory };
        foreach (Item item in transfered_items)
        {
            inventory2.Grab(item, uis_to_ignore); // on ignore les ui_inventory parce qu'ils ont déjà été grab dans ces UI_Inventory
        }
    }

    // ENABLE / DISABLE UI_ITEM_POOL SLOTS FOR MOVING
    private void enable_only_recevable_slots(UI_Item moving_ui)
    {
        UI_ItemPool moving_pool = moving_ui.ItemPool;
        Item moving_item = moving_ui.Item;
        if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) enabling only receivable slots for item {moving_item.name} in pool {moving_pool.name}"); }

        List<UI_ItemPool> pools = get_items_pools_from_slottables(manager.Slottables);
        List<UI_Slot> slots = new List<UI_Slot>();

        // convert slottables into ui_inventories
        string log_slottables = "(UI_ItemMover) got " + pools.Count + " pools to enable/disable slots from \n";

        // on récupère les item pools
        for (int i = 0; i < pools.Count; i++)
        {
            UI_ItemPool pool = pools[i];

            log_slottables += $"- {pool.name} ({pool.GetType()}) ";

            // si le moving item ne matche pas la rule de la pool on désactive toute la pool
            if (!pool.CanStore(moving_item))
            {
                // on désactive tous les slots de la pool
                slots.Clear();
                slots.AddRange(pool.GetAllSlots());
                log_slottables += $"--> cannot store {moving_item.name}, disabling all {slots.Count} slots \n";
                for (int j = 0; j < slots.Count; j++) { slots[j].Disable(); }
                continue;
            }

            // on récupère les slots de la pool
            slots.Clear();
            slots.AddRange(pool.GetAllSlots());
            log_slottables += $"--> can store {moving_item.name}, enabling receivable slots \n";
            for (int k = 0; k < slots.Count; k++)
            {
                UI_Slot slot = slots[k];
                if (slot is not UI_Item ui_item) { slot.Disable(); continue; }
                if (ui_item == moving_ui) { continue; } // on ne désactive pas le slot en cours de drag

                // on regarde si le slot a un item qui peut etre recu par le moving_ui_item_pool
                if (ui_item.Item != null && !moving_pool.CanStore(ui_item.Item))
                {
                    ui_item.Disable(); // on désactive le slot
                    continue;
                }

                // sinon on active le slot
                log_slottables += $"        --> enabled slot {ui_item.name}\n";
                ui_item.Enable();
            }

            // on regarde si la pool est full et scalable -> on ajout un ui_item vide dedans
            if (pool == moving_pool) { continue; }
            if (!pool.Scalable) { continue; }
            if (!pool.CanStore(moving_item)) { continue; }
            if (pool.EmptyCount > 0) { continue; }
            GameObject empty_slot = pool.CreateItemSlot();
            empty_slot.GetComponent<UI_Item>().Enable();
            log_slottables += $"        --> added empty slot bcz scalable & full\n";
        }

        if (log_receivable_slots) { Debug.Log(log_slottables); }

        // si on a un UI_InventoryMenu dans nos uis alors on refresh ses UI_ItemPools
        if (UI_Manager.Instance.CurrentPool == "inventory")
        {
            UI_InventoryMenu inventory_menu = UI_Manager.Instance.GetPool<UI_InventoryMenu>();
            if (inventory_menu != null)
            {
                if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) refreshing inventory menu item pools"); }
                inventory_menu.RefreshItemPools();
                return;
            }
        }
        if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) no inventory menu to refresh"); }

        // refreshing navigator ui_slots
        manager.UpdateSlots();
    }
    private void disable_only_empty_slots(bool except_modules = false)
    {
        // on sauvegarde les item pools qu'on trouve
        List<UI_ItemPool> item_pools = get_items_pools_from_slottables(manager.Slottables);
        if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) disabling empty slots in {item_pools.Count} item pools"); }

        for (int i = 0; i < item_pools.Count; i++)
        {
            UI_ItemPool pool = item_pools[i];

            // on récupère les slots du inventory
            List<UI_Item> slots = pool.GetAllSlots();
            for (int j = 0; j < slots.Count; j++)
            {
                UI_Item ui_item = slots[j];

                // on regarde si le slot n'a pas d'item on le désactive
                if (ui_item.Item != null) { ui_item.Enable(); continue; }
                if (except_modules && ui_item is UI_Module) { ui_item.Enable(); continue; } // on ne désactive pas les modules
                if (pool.DoNotDisableEmptySlots) { ui_item.Enable(); continue; }
                ui_item.Disable(); // on désactive le slot
            }

            if (pool.Scalable) { pool.DestroyEmptySlots(); }
        }

        // si on a un UI_InventoryMenu dans nos uis alors on refresh ses UI_ItemPools
        if (UI_Manager.Instance.CurrentPool == "inventory")
        {
            UI_InventoryMenu inventory_menu = UI_Manager.Instance.GetPool<UI_InventoryMenu>();
            if (inventory_menu != null)
            {
                inventory_menu.RefreshItemPools();
            }
        }
    }
    private List<UI_Inventory> get_inventories_from_slottables(List<Slottable> slottables)
    {
        List<UI_Inventory> inventories = new List<UI_Inventory>();

        // convert slottables into ui_inventories
        while (slottables.Count > 0)
        {
            Slottable slottable = slottables[0];
            slottables.RemoveAt(0);

            // if slottable is the UI_Laptop we remove it (idk why we do this but okeyy buddy)
            // if (slottable is UI_Laptop) { continue; }

            // if slottable is directly an inventory it s perfect
            if (slottable is UI_Inventory inventory && !inventories.Contains(inventory)) { inventories.Add(inventory); }

            // if this is a slottable mixer we add their slottables to the slottables list
            if (slottable is UI_SlottableMixer mixer) { slottables.AddRange(mixer.Slottables); }
        }

        return inventories;
    }
    private List<UI_ItemPool> get_items_pools_from_slottables(List<Slottable> slottables)
    {
        List<UI_ItemPool> item_pools = new List<UI_ItemPool>();

        // convert slottables into ui_inventories
        List<UI_Inventory> inventories = get_inventories_from_slottables(new List<Slottable>(slottables));

        // on récupère les item pools
        for (int i = 0; i < inventories.Count; i++)
        {
            UI_Inventory inventory = inventories[i];
            if (inventory == null) { continue; }

            // on récupère tous les UI_ItemPool de l'inventory
            for (int j = 0; j < inventory.pools.Count; j++)
            {
                UI_ItemPool item_pool = inventory.pools[j];
                if (!item_pools.Contains(item_pool)) { item_pools.Add(item_pool); }
            }
        }

        return item_pools;
    }
}