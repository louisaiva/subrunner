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

        // todo implement a new UI_Slot type which is kind of a item receiver slot
        // todo that is linked to a specific inventory / item pool that will receive and handles the itemstack


        // checks if the current slot is an ui_item and not the same as the moving item one
        UI_ItemStack destination = manager.CurrentSlot as UI_ItemStack;

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

        // on vérifie que les ItemPool reliés aux ItemStack des 2 UI_ItemStack ne sont pas null sinon on fait r
        if (moving_ui_item.Stack.Storer == null || destination.Stack.Storer == null)
        {
            if (log) { Debug.Log($"(UI_ItemMover) cannot move items because one of the stack has no storer : moving_ui_item storer = {moving_ui_item.Stack.Storer}, destination storer = {destination.Stack.Storer}"); }
            return;
        }


        // on choisit en fonction des différentes situations :
        if (destination != moving_ui_item && destination.Stack.CanAdd(moving_ui_item.Stack))
        {
            merge_items(moving_ui_item, destination); // ce sont les mêmes items, on peut alors les merge ensemble
        }
        else { switch_items(moving_ui_item, destination); }
    }
    private void switch_items(UI_ItemStack stack1, UI_ItemStack stack2)
    {
        // we need at least one stack.storer to be an ItemPool
        if (stack1 == null || stack2 == null) { return; }
        ItemPool swapper = null;
        if (stack1.Stack.Storer is ItemPool pool1) { swapper = pool1; }
        else if (stack2.Stack.Storer is ItemPool pool2) { swapper = pool2; }
        if (swapper == null) { return; }

        // on échange les items entre les deux stacks
        if (log_moving_items) { Debug.Log($"(UI_ItemMover) switching items between {stack1.gameObject.name} and {stack2.gameObject.name}"); }

        swapper.SwapStacks(stack1.Stack,stack2.Stack);
    }
    private void merge_items(UI_ItemStack from, UI_ItemStack to)
    {
        // we need the to stack.Storer to be ItemPool
        if (from == null || to == null || to.Stack.Storer == null) { return; }
        ItemPool merger = to.Stack.Storer as ItemPool;
        if (merger == null) { return; }

        if (log_moving_items) { Debug.Log($"(UI_ItemMover) merging items from {from.gameObject.name} into {to.gameObject.name}"); }
        // on merge les items de from dans to
        merger.MergeIntoStack(from.Stack, to.Stack);
    }


    // ENABLE / DISABLE UI_ITEM_POOL SLOTS FOR MOVING
    private void enable_only_recevable_slots(UI_ItemStack moving_ui)
    {
        UI_ItemSlottable moving_pool = moving_ui.UI_ItemSlottable;
        Item moving_item = moving_ui.Stack.Item;
        if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) enabling only receivable slots for item {moving_item.name} in pool {moving_pool.name}"); }

        // on récup toutes les ui_item_pools qui sont affichées
        List<UI_ItemSlottable> ui_item_pools = get_all_valid_ui_item_pools();


        // log preparatiob
        string log_slottables = "(UI_ItemMover) got " + ui_item_pools.Count + " ui_item_pools to enable/disable slots from \n";
        List<UI_Slot> slots = new List<UI_Slot>();

        // on parcourt tous les ui item pools
        for (int i = 0; i < ui_item_pools.Count; i++)
        {
            UI_ItemSlottable ui_item_pool = ui_item_pools[i];
            ItemStorer storer = ui_item_pool.Storer;

            log_slottables += $"\n - {ui_item_pool.name} ({ui_item_pool.GetType()}) ";

            // si le UI_ItemSlottable est un UI_FilteredItemPool alors ses ItemStacks n'ont pas de Pool
            // et on désactive tous les slots sauf si c le moving ui_itemstack
            if (ui_item_pool is UI_CompactItemPool)
            {
                // on récupère les slots du inventory
                slots.Clear();
                slots.AddRange(ui_item_pool.GetAllSlots());
                log_slottables += $"   --> {ui_item_pool.name} is a UI_FilteredItemPool, disabling all slots except the moving one \n";
                for (int j = 0; j < slots.Count; j++)
                {
                    UI_ItemStack ui_item = slots[j] as UI_ItemStack;
                    if (ui_item == null) { continue; }
                    if (ui_item == moving_ui) { continue; }
                    ui_item.Disable();
                }
                continue;
            }

            // si le moving item ne matche pas la rule de la ui_item_pool on désactive toute la ui_item_pool
            if (!storer.ValidateRule(moving_item))
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
                storer is ItemPool pool &&
                pool.Scalable &&
                (ui_item_pool.EmptyCount == 0))
            {
                pool.AddEmptyStack();
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
                if (ui_item.Stack.Item != null && !moving_pool.Storer.ValidateRule(ui_item.Stack.Item))
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

        UI_Manager.Instance.RefreshInventoryMenu();
    }
    private void disable_only_empty_slots(bool except_modules = false)
    {
        // on sauvegarde les item pools qu'on trouve
        List<UI_ItemSlottable> ui_item_pools = get_all_valid_ui_item_pools();
        if (log_receivable_slots) { Debug.Log($"(UI_ItemMover) disabling empty slots in {ui_item_pools.Count} item pools"); }

        for (int i = 0; i < ui_item_pools.Count; i++)
        {
            UI_ItemSlottable ui_pool = ui_item_pools[i];
            ItemStorer storer = ui_pool.Storer;

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

            if (storer is ItemPool pool && pool.Scalable) { pool.DestroyEmptyStacks(); }
        }
        
        manager.UpdateSlots();

        UI_Manager.Instance.RefreshInventoryMenu();
    }

    /// <summary>
    /// this method find all ui_item_pools that have a linked item_pool. It searches for them from
    /// the ui_navigator slottables, and uses recursive method below to track down all slottables in these slottables / slottables mixer
    /// </summary>
    /// <returns>a list containing the found & valid ui_item_pools</returns>
    private List<UI_ItemSlottable> get_all_valid_ui_item_pools()
    {
        List<UI_ItemSlottable> found_pools = new List<UI_ItemSlottable>();
        get_ui_items_pools_from_slottables(new List<Slottable>(manager.Slottables), ref found_pools);

        /* if (log_get_pools)
        {
            string log_msg = $"(UI_ItemMover) found {found_pools.Count} ui_item_pools from slottables : \n";
            log_msg += "\n - " + $"manager has {manager.Slottables.Count} root slottables \n";
            for (int i = 0; i < found_pools.Count; i++)
            {
                UI_ItemSlottable pool = found_pools[i];
                log_msg += $"    - {pool.name} ({pool.GetType()}) - linked to - {pool.Storer?.name ?? "null"} \n";
            }
            Debug.Log(log_msg);
        } */


        found_pools = found_pools.Where(ui_pool => ui_pool.Storer != null).ToList();
        return found_pools;
    }
    private void get_ui_items_pools_from_slottables(List<Slottable> slottables, ref List<UI_ItemSlottable> found_pools)
    {
        // List<UI_ItemSlottable> item_pools = new List<UI_ItemSlottable>();

        // convert slottables into ui_item_pools
        while (slottables.Count > 0)
        {
            Slottable slottable = slottables[0];
            slottables.RemoveAt(0);

            // if slottable is directly an item pool it s perfect
            if (slottable is UI_ItemSlottable item_pool && !found_pools.Contains(item_pool)) { found_pools.Add(item_pool); }

            if (slottable is not UI_SlottableMixer mixer) { continue; }

            // if this is a slottable mixer we go through their slottables too
            List<Slottable> mixer_slottables = mixer.Slottables.Cast<Slottable>().ToList();
            get_ui_items_pools_from_slottables(mixer_slottables, ref found_pools);
        }
    }
}