using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DistributorPool : UI_CraftPool
{
    public UI_ItemPool TicketUI_ItemPool;

    // crafter ATTACH / DETACH
    public override void AttachCrafter(Crafter crafter)
    {
        base.AttachCrafter(crafter);
        if (crafter is not Distributor distrib) { return; }
        TicketUI_ItemPool.AttachToPool(distrib.Inventory.GetItemPool("tickets"));
    }
    public override void DetachCrafter()
    {
        base.DetachCrafter();
        TicketUI_ItemPool.DetachFromPool();
    }


    // BUTTON CALLERS
    public void TryInsertTicket()
    {
        if (crafter == null) { return; }
        List<Item> tickets = Controller.Capable.Inventory.GetItemsByRule("other:ticket");
        if (tickets.Count == 0)
        {
            if (log) { Debug.LogWarning("(UI_DistributorPool) Could not insert ticket since perso has no ticket !!!"); }
            return;
        }
        if (!crafter.Inventory.Grab(tickets[0]))
        {
            Debug.LogError("(UI_DistributorPool) Could not insert ticket bcz crafter did not want to :///".AddColor(Color.magenta));
            return;
        }
        if (log) { Debug.Log("(UI_DistributorPool) YAAAAAY we inserted a ticket into the distributor !"); }
    }
}