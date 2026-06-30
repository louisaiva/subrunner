using System.Collections;
using TMPro;
using UnityEngine;

public class UI_ChestPool : UI_SlottablePool
{

    [Header("UI_Inventory")]
    public UI_CompactItemPool PersoUI_Inventory; // the UI_Inventory of the perso, used to show the items of the chest when we open it
    public UI_ItemPool ChestUI_ItemPool; // the UI_ItemPool of the chest, used to show the items of the chest when we open it
    private Chestable chestable;

    [Header("UI_ChestPool title")]
    [SerializeField] private TextMeshProUGUI title_text;



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


    // MAIN SHOW / HIDE
    public void ShowChest(Chestable chestable)
    {
        // attach the chest inventory & perso inventory to the ui chest pool
        AttachChest(chestable.Inventory);
        AttachPerso(chestable.Interactor?.Capable.Inventory);

        // set the title text
        if (title_text != null) { title_text.text = chestable.ChestType; }

        // then we show the ui_chest
        UI_Manager.Instance.SwitchTo(Reference);
    }
    public void HideChest(/* Chestable chestable */)
    {
        // we detach the chest inventory & perso inventory from the ui chest pool
        DetachChest();
        DetachPerso();

        // then we hide the ui_chest
        UI_Manager.Instance.UnstackPool(Reference);
    }
    public bool IsShown(Chestable chestable)
    {
        return chestable == this.chestable;
    }



    // PERSO ATTACH / DETACH
    public void AttachPerso(Inventory perso_inv)
    {
        // check if we have a chest already, if yes we extract their rule to give it to the ui_compact item pool
        string rule = "";
        if (chestable != null) { rule = chestable.Inventory.ItemRule; }

        // on met les items du chest dans le UI_Inventory du perso
        PersoUI_Inventory.AttachStorer(perso_inv, rule);
    }
    public void DetachPerso()
    {
        // on met les items du chest dans le UI_Inventory du perso
        PersoUI_Inventory.DetachStorer();
    }


    // CHEST ATTACH / DETACH
    public void AttachChest(Inventory chest_inv)
    {
        // on met les items du chest dans
        ChestUI_ItemPool.AttachToPool(chest_inv);

        if (chest_inv.Capable is Chestable chestable) { this.chestable = chestable; }

        // on s'assure que le persoquickinventory n'affiche que les bons items
        // if (ui_chest is UI_Inventory ui_inv) { perso_quick_inventory_pool.EnableItemsByRule(ui_inv.ItemRule); }
        // else if (ui_chest is UI_SlottableMixer ui_mixer) { perso_quick_inventory_pool.EnableItemsByRule(ui_mixer.GetItemRule()); }
    }
    public void DetachChest()
    {
        // on met les items du chest dans
        ChestUI_ItemPool.DetachFromPool();

        this.chestable = null;

        // on s'assure que le persoquickinventory n'affiche que les bons items
        // perso_quick_inventory_pool.EnableItemsByRule(null);
    }

    // CLOSING CHEST (HAPPENS WHEN SWITCHING TO ANOTHER UI_POOL)
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();

        // we force the chest to interrupt interaction
        chestable?.ExitHover();

        yield break;
    }

}