using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_OrdererPool : UI_CraftPool, Descriptable
{
    public List<UI_ItemPool> meal_pools;

    public string Name => "Pasta cooking machine";
    public string Description => (crafter == null || crafter is not Orderer orderer ? "crafter is null or not orderer".AddColor(Color.red) : orderer.GetCurrentMealDescription()) + "\n\nPut your ingredients in the machine to create your pasta meal !";


    // crafter ATTACH / DETACH
    public override void AttachCrafter(Crafter crafter)
    {
        base.AttachCrafter(crafter);
        if (crafter is not Orderer orderer) { return; }

        foreach (UI_ItemPool ui_pool in meal_pools)
        {
            ItemPool pool = orderer.Inventory.GetItemPool(ui_pool.PoolID);
            if (pool == null) { Debug.LogError($"(UI_OrdererPool) Can't connect ui_pool '{ui_pool.PoolID}' to '{orderer.ID}'s inventory bcz no ItemPool with corresponding pool_id was found"); continue; }
            ui_pool.AttachToPool(pool);
        }
    }
    public override void DetachCrafter()
    {
        base.DetachCrafter();
        foreach (UI_ItemPool ui_pool in meal_pools) { ui_pool.DetachFromPool(); }
    }


    // BUTTON CALLERS
    public void CookMeal()
    {
        if (crafter == null) { return; }
        if (log) { Debug.Log($"(UI_OrdererPool) Cooking {crafter.ID} meal !"); }
        if (crafter is not Orderer orderer) { return; }
        orderer.Order();
    }
}