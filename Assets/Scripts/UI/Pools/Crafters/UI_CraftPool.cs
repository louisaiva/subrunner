using System.Collections;
using UnityEngine;
using TMPro;

public class UI_CraftPool : UI_SlottablePool
{

    [Header("Connected crafter")]
    public Crafter crafter;
    
    [Header("UI_Inventory & Labels")]
    public UI_CompactItemPool PersoUI_Inventory; // the UI_Inventory of the perso, used to show the items of the chest when we open it
    [SerializeField] private TextMeshProUGUI no_item_label;


    // MAIN SHOW / HIDE
    public void ShowCraftPool(Crafter crafter)
    {
        // attach the chest inventory & perso inventory to the ui chest pool
        AttachCrafter(crafter);
        AttachPerso(crafter.Interactor?.Capable.Inventory);

        // then we show the ui_chest
        UI_Manager.Instance.SwitchTo(Reference);
    }
    public void HideCraftPool()
    {
        // we detach the chest inventory & perso inventory from the ui chest pool
        DetachCrafter();
        DetachPerso();

        // then we hide the ui_chest
        UI_Manager.Instance.UnstackPool(Reference);
    }
    public bool IsShown(Crafter crafter) { return crafter == this.crafter; }



    // PERSO ATTACH / DETACH
    public void AttachPerso(Inventory perso_inv)
    {
        // check if we have a chest already, if yes we extract their rule to give it to the ui_compact item pool
        string rule = "";
        if (crafter != null) { rule = crafter.ItemRule; }

        // on met les items du chest dans le UI_Inventory du perso
        PersoUI_Inventory.AttachStorer(perso_inv, rule);
    }
    public void DetachPerso() { PersoUI_Inventory.DetachStorer(); }


    // crafter ATTACH / DETACH
    public virtual void AttachCrafter(Crafter crafter)
    {
        this.crafter = crafter;
        no_item_label.text = crafter.EmptyInventoryDesc;
    }
    public virtual void DetachCrafter() { crafter = null; }

    // CLOSING crafter (HAPPENS WHEN SWITCHING TO ANOTHER UI_POOL)
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();

        // we force the crafter to interrupt interaction
        crafter?.ExitHover();

        yield break;
    }

}