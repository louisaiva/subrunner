using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_ItemMover : MonoBehaviour
{
    private UI_Navigator manager;


    [Header("Moving Item")]
    [SerializeField] private UI_ItemStack moving_ui_item = null;
    [SerializeField] private UI_ItemStack potential_moving_item = null;
    public UI_ItemStack MovingUIItem { get => moving_ui_item; }
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
    public void StartMovingItem(UI_ItemStack item)
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
    public void SetPotentialMovingItem(UI_ItemStack item)
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
        UI_ItemStack destination = manager.CurrentSlot as UI_ItemStack;
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
    private void moveItems(UI_ItemStack destination)
    {
        // on choisit le mode d'action qu'il faut pour echanger les items
        // entre moving_ui_item & destination

        // on choisit en fonction des différentes situations :
        if (destination != moving_ui_item && destination.Stack.CanAdd(moving_ui_item.Stack))
        {
            merge_items(moving_ui_item, destination); // ce sont les mêmes items, on peut alors les merge ensemble
        }
        else { switch_items(moving_ui_item, destination); }
    }
    private void switch_items(UI_ItemStack stack1, UI_ItemStack stack2)
    {
        // on échange les items entre les deux UI_Items
        if (stack1 == null || stack2 == null) { return; }

        if (log_moving_items) { Debug.Log($"(UI_ItemMover) switching items between {stack1.gameObject.name} and {stack2.gameObject.name}"); }

        stack1.Stack.Pool.SwapStacks(stack1.Stack,stack2.Stack);
    }
    private void merge_items(UI_ItemStack from, UI_ItemStack to)
    {
        // on merge les items de from dans to
        if (from == null || to == null || to.Stack.Pool == null) { return; }
        if (log_moving_items) { Debug.Log($"(UI_ItemMover) merging items from {from.gameObject.name} into {to.gameObject.name}"); }
        to.Stack.Pool.MergeIntoStack(from.Stack, to.Stack);
    }


    // ENABLE / DISABLE UI_ITEM_POOL SLOTS FOR MOVING
    private void enable_only_recevable_slots(UI_ItemStack moving_ui)
    {
        UI_ItemPool moving_pool = moving_ui.UI_ItemPool;
        Item moving_item = moving_ui.Stack.Item;
        if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) enabling only receivable slots for item {moving_item.name} in pool {moving_pool.name}"); }

        // on récup toutes les ui_item_pools qui sont affichées
        List<UI_ItemPool> ui_item_pools = new List<UI_ItemPool>();
        get_ui_items_pools_from_slottables(manager.Slottables, ref ui_item_pools);
        // List<UI_ItemPool> pools = get_items_pools_from_slottables(manager.Slottables);
        ui_item_pools = ui_item_pools.Where(ui_pool => ui_pool.pool != null).ToList();


        // log preparatiob
        string log_slottables = "(UI_ItemMover) got " + ui_item_pools.Count + " ui_item_pools to enable/disable slots from \n";
        List<UI_Slot> slots = new List<UI_Slot>();

        // on parcourt tous les ui item pools
        for (int i = 0; i < ui_item_pools.Count; i++)
        {
            UI_ItemPool ui_item_pool = ui_item_pools[i];

            log_slottables += $"- {ui_item_pool.name} ({ui_item_pool.GetType()}) ";

            // si le moving item ne matche pas la rule de la ui_item_pool on désactive toute la ui_item_pool
            if (!ui_item_pool.pool.ValidateRule(moving_item))
            {
                // on désactive tous les slots de la ui_item_pool
                slots.Clear();
                slots.AddRange(ui_item_pool.GetAllSlots());
                log_slottables += $"--> cannot store {moving_item.name}, disabling all {slots.Count} slots \n";
                for (int j = 0; j < slots.Count; j++) { slots[j].Disable(); }
                continue;
            }

            // on récupère les slots de la ui_item_pool
            slots.Clear();
            slots.AddRange(ui_item_pool.GetAllSlots());
            log_slottables += $"--> can store {moving_item.name}, enabling receivable slots \n";
            for (int k = 0; k < slots.Count; k++)
            {
                UI_Slot slot = slots[k];
                if (slot is not UI_ItemStack ui_item) { slot.Disable(); continue; }
                if (ui_item == moving_ui) { continue; } // on ne désactive pas le slot en cours de drag

                // on regarde si le moving_ui_item_pool peut recevoir l'item du slot (scénario inverse de juste avant)
                if (ui_item.Stack.Item != null && !moving_pool.pool.ValidateRule(ui_item.Stack.Item))
                {
                    ui_item.Disable(); // on désactive le slot
                    continue;
                }

                // sinon on active le slot
                log_slottables += $"        --> enabled slot {ui_item.name}\n";
                ui_item.Enable();
            }

            // on regarde si la ui_pool est full et scalable -> on ajout un ui_item vide dedans
            if (ui_item_pool == moving_pool) { continue; }
            if (!ui_item_pool.pool.Scalable) { continue; }
            if (ui_item_pool.EmptyCount > 0) { continue; }
            ui_item_pool.pool.AddEmptyStack();
            // UI_ItemStack empty_slot = ui_item_pool.CreateItemSlot();
            // empty_slot.Enable();
            log_slottables += $"        --> added empty slot bcz scalable & full\n";
        }

        if (log_receivable_slots) { Debug.Log(log_slottables); }

        // si on a un UI_InventoryMenu dans nos uis alors on refresh ses UI_ItemPools
        /* if (UI_Manager.Instance.CurrentPool == "inventory")
        {
            UI_InventoryMenu inventory_menu = UI_Manager.Instance.GetPool<UI_InventoryMenu>();
            if (inventory_menu != null)
            {
                if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) refreshing inventory menu item pools"); }
                inventory_menu.RefreshItemPools();
                return;
            }
        }
        if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) no inventory menu to refresh"); } */

        // refreshing navigator ui_slots
        manager.UpdateSlots();
    }
    private void disable_only_empty_slots(bool except_modules = false)
    {
        // on sauvegarde les item pools qu'on trouve
        // List<UI_ItemPool> item_pools = get_items_pools_from_slottables(manager.Slottables);
        List<UI_ItemPool> ui_item_pools = new List<UI_ItemPool>();
        get_ui_items_pools_from_slottables(manager.Slottables, ref ui_item_pools);
        ui_item_pools = ui_item_pools.Where(ui_pool => ui_pool.pool != null).ToList();
        if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) disabling empty slots in {ui_item_pools.Count} item pools"); }

        for (int i = 0; i < ui_item_pools.Count; i++)
        {
            UI_ItemPool ui_pool = ui_item_pools[i];

            // on récupère les slots du inventory
            List<UI_ItemStack> slots = ui_pool.GetAllSlots();
            for (int j = 0; j < slots.Count; j++)
            {
                UI_ItemStack ui_item = slots[j];

                // on regarde si le slot n'a pas d'item on le désactive
                if (ui_item.Stack.Item != null) { ui_item.Enable(); continue; }
                if (except_modules && ui_item is UI_Module) { ui_item.Enable(); continue; } // on ne désactive pas les modules
                if (ui_pool.DoNotDisableEmptySlots) { ui_item.Enable(); continue; }
                ui_item.Disable(); // on désactive le slot
            }

            if (ui_pool.pool.Scalable) { ui_pool.pool.DestroyEmptyStacks(); }
        }

        // si on a un UI_InventoryMenu dans nos uis alors on refresh ses UI_ItemPools
        /* if (UI_Manager.Instance.CurrentPool == "inventory")
        {
            UI_InventoryMenu inventory_menu = UI_Manager.Instance.GetPool<UI_InventoryMenu>();
            if (inventory_menu != null)
            {
                inventory_menu.RefreshItemPools();
            }
        } */
    }
    /* private List<UI_Inventory> get_inventories_from_slottables(List<Slottable> slottables)
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
    } */
    private void get_ui_items_pools_from_slottables(List<Slottable> slottables, ref List<UI_ItemPool> found_pools)
    {
        // List<UI_ItemPool> item_pools = new List<UI_ItemPool>();

        // convert slottables into ui_item_pools
        while (slottables.Count > 0)
        {
            Slottable slottable = slottables[0];
            slottables.RemoveAt(0);

            // if slottable is directly an item pool it s perfect
            if (slottable is UI_ItemPool item_pool && !found_pools.Contains(item_pool)) { found_pools.Add(item_pool); }

            if (slottable is not UI_SlottableMixer mixer) { continue; }

            // if this is a slottable mixer we go through their slottables too
            List<Slottable> mixer_slottables = mixer.Slottables.Cast<Slottable>().ToList();
            get_ui_items_pools_from_slottables(mixer_slottables, ref found_pools);
        }
    }
}