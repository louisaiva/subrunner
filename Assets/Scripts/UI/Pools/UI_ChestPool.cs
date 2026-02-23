using System.Collections;
using UnityEngine;

public class UI_ChestPool : UI_SlottablePool
{

    [Header("UI_Inventory")]
    public UI_CompactItemPool PersoUI_Inventory; // the UI_Inventory of the perso, used to show the items of the chest when we open it
    public UI_ItemPool ChestUI_ItemPool; // the UI_ItemPool of the chest, used to show the items of the chest when we open it
    private Chest chest; // todo for now it only works with Chest but should it rather work with HoverCapacity ?

    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        // we add item pools to our ui_elements
        if (!ui_elements.Contains(PersoUI_Inventory.transform.parent.gameObject)) { ui_elements.Add(PersoUI_Inventory.transform.parent.gameObject); }
        if (!ui_elements.Contains(ChestUI_ItemPool.transform.parent.gameObject)) { ui_elements.Add(ChestUI_ItemPool.transform.parent.gameObject); }

        // we add item pools to slottable mixer
        if (!(slottable is UI_SlottableMixer mixer)) { if (log) { Debug.LogError($"(UI_ChestPool) slottable on {name} is not a UI_SlottableMixer"); } return; }

        // we add all the UI_ItemPools in the ui_elements as slottables inside our UI_SlottableMixer
        mixer.AddSlottable(ChestUI_ItemPool);
        mixer.AddSlottable(PersoUI_Inventory);
    }

    // PERSO ATTACH / DETACH
    public void AttachPerso(Inventory perso_inv)
    {
        // on met les items du chest dans le UI_Inventory du perso
        PersoUI_Inventory.AttachToStorer(perso_inv);
    }
    public void DetachPerso()
    {
        // on met les items du chest dans le UI_Inventory du perso
        PersoUI_Inventory.DetachFromStorer();
    }


    // CHEST ATTACH / DETACH
    public void AttachChest(Inventory chest_inv)
    {
        // on met les items du chest dans
        ChestUI_ItemPool.AttachToPool(chest_inv);

        if (chest_inv.capable is Chest chest) { this.chest = chest; }

        // on s'assure que le persoquickinventory n'affiche que les bons items
        // if (ui_chest is UI_Inventory ui_inv) { perso_quick_inventory_pool.EnableItemsByRule(ui_inv.ItemRule); }
        // else if (ui_chest is UI_SlottableMixer ui_mixer) { perso_quick_inventory_pool.EnableItemsByRule(ui_mixer.GetItemRule()); }
    }
    public void DetachChest()
    {
        // on met les items du chest dans
        ChestUI_ItemPool.DetachFromPool();

        this.chest = null;

        // on s'assure que le persoquickinventory n'affiche que les bons items
        // perso_quick_inventory_pool.EnableItemsByRule(null);
    }

    // CLOSING CHEST (HAPPENS WHEN SWITCHING TO ANOTHER UI_POOL)
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();

        // we force the chest to interrupt interaction
        chest?.ExitHover();

        yield break;
    }

}