using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distributor : Crafter
{

    ///
    //
    /// CRAFTER
    //
    ///
    public override string EmptyInventoryDesc => "tickets will be shown here";
    public override string ItemRule => "other:ticket";
    public override string UI_PoolName => "distributor";

    private Coroutine pasta_coroutine = null;
    private List<Item> pending_tickets = new List<Item>();
    private void handle_item_grabbed(Item item)
    {
        // Debug.Log("(Distributor) Handle item grabbed : " + item.ID);

        if (item.Reference != "other:ticket") { return; }

        // we launch the pasta craft coroutine !!!
        pending_tickets.Add(item);
        if (pasta_coroutine != null) { return; } // we already have a pasta coroutine, so it is good !
        pasta_coroutine = StartCoroutine(distribute_pasta());

        // we quit the ui
        hide_ui_pool();
    }

    [SerializeField] private DropParameters drop_param = new DropParameters()
    {
        random_direction = false,
        drop_magnitude = 70f,
        offset_drop = new Vector2(-.2f,0f),
        lock_magnitude = true,
    };

    private IEnumerator distribute_pasta()
    {
        AnimPlayer.Play("craft");
        Pasta pastas = CapableEngine.Instance.LoadCapableInstantly("dry_pastas") as Pasta;
        Inventory.Grab(pastas);
        while (AnimPlayer.IsPlaying("craft")) { yield return null; }

        // now we drop some pastas !
        DropEngine.Instance.Drop(this, pastas, drop_param);
        
        // we destroy the ticket
        Inventory.Drop(pending_tickets[0], on_ground:false);
        CapableEngine.Instance.DespawnCapable(pending_tickets[0].data);
        pending_tickets.RemoveAt(0);
        if (pending_tickets.Count > 0) { yield return distribute_pasta(); }
        pasta_coroutine = null;
    }

    ///
    //
    /// DATA MANAGEMENT
    //
    ///

    // LOAD / UNLOAD DATA
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);
        Inventory.OnItemGrabbed += handle_item_grabbed;
    }
    public override void UnloadData()
    {
        Inventory.OnItemGrabbed -= handle_item_grabbed;
        base.UnloadData();
    }
}