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
    // [SerializeField] private bool log_get_pools = false;

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
        List<UI_ItemPool> ui_item_pools = get_all_valid_ui_item_pools();


        // log preparatiob
        string log_slottables = "(UI_ItemMover) got " + ui_item_pools.Count + " ui_item_pools to enable/disable slots from \n";
        List<UI_Slot> slots = new List<UI_Slot>();

        // on parcourt tous les ui item pools
        for (int i = 0; i < ui_item_pools.Count; i++)
        {
            UI_ItemPool ui_item_pool = ui_item_pools[i];

            log_slottables += $"\n - {ui_item_pool.name} ({ui_item_pool.GetType()}) ";

            // si le moving item ne matche pas la rule de la ui_item_pool on désactive toute la ui_item_pool
            if (!ui_item_pool.pool.ValidateRule(moving_item))
            {
                // on désactive tous les slots de la ui_item_pool
                slots.Clear();
                slots.AddRange(ui_item_pool.GetAllSlots());
                log_slottables += $"   --> cannot store {moving_item.name}, disabling all {slots.Count} slots \n";
                for (int j = 0; j < slots.Count; j++) { slots[j].Disable(); }
                continue;
            }

            // on regarde si la ui_pool est full et scalable -> on ajout un ui_item vide dedans
            if ((ui_item_pool != moving_pool) &&
                ui_item_pool.pool.Scalable &&
                (ui_item_pool.EmptyCount == 0))
            {
                ui_item_pool.pool.AddEmptyStack();
                log_slottables += $"   --> added empty slot bcz scalable & full\n";
            }

            // on récupère les slots de la ui_item_pool
            slots.Clear();
            slots.AddRange(ui_item_pool.GetAllSlots());
            log_slottables += $"   --> can store {moving_item.name}, enabling receivable slots \n";
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
                ui_item.Enable();
            }
        }

        if (log_receivable_slots) { Debug.Log(log_slottables); }
        manager.UpdateSlots();
    }
    private void disable_only_empty_slots(bool except_modules = false)
    {
        // on sauvegarde les item pools qu'on trouve
        List<UI_ItemPool> ui_item_pools = get_all_valid_ui_item_pools();
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
                // if (ui_pool.DoNotDisableEmptySlots) { ui_item.Enable(); continue; }
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

    /// <summary>
    /// this method find all ui_item_pools that have a linked item_pool. It searches for them from
    /// the ui_navigator slottables, and uses recursive method below to track down all slottables in these slottables / slottables mixer
    /// </summary>
    /// <returns>a list containing the found & valid ui_item_pools</returns>
    private List<UI_ItemPool> get_all_valid_ui_item_pools()
    {
        List<UI_ItemPool> found_pools = new List<UI_ItemPool>();
        get_ui_items_pools_from_slottables(new List<Slottable>(manager.Slottables), ref found_pools);

        /* if (log_get_pools)
        {
            string log_msg = $"(UI_ItemMover) found {found_pools.Count} ui_item_pools from slottables : \n";
            log_msg += "\n - " + $"manager has {manager.Slottables.Count} root slottables \n";
            for (int i = 0; i < found_pools.Count; i++)
            {
                UI_ItemPool pool = found_pools[i];
                log_msg += $"    - {pool.name} ({pool.GetType()}) - linked to - {pool.pool?.name ?? "null"} \n";
            }
            Debug.Log(log_msg);
        } */


        found_pools = found_pools.Where(ui_pool => ui_pool.pool != null).ToList();
        return found_pools;
    }
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