using System.Collections;
using UnityEngine;

public class UI_ChestPool : UI_SlottablePool
{

    [Header("Perso Quick Inventory")]
    public UI_Inventory UI;
    private HUD_PersoItemPool perso_quick_inventory_pool;
    private UI_SlottableMixer slottable_mixer => slottable as UI_SlottableMixer;

    // START & RJOY VERIF
    protected void Start()
    {
        // on met les callbacks pour vérifier que le select_hackable se désactive bien
        // InputManager.Instance.OnPersoInputsToggled += verify_right_joy_is_disabled;
        perso_quick_inventory_pool = UI.GetComponent<HUD_PersoItemPool>();
    }

    /* private void verify_right_joy_is_disabled(bool perso_inputs_enabled)
    {
        // todo : is there a better way to do this ? looks schlag. maybe it's better to bring back enhanced_perso map ? or hacking map ?

        if (!perso_inputs_enabled) { return; }
        if (!Showed) { return; }
        if (slottable_mixer.Count == 1) { return; } // si on a qu'un seul slottable, pas besoin de désactiver le joystick

        // on doit s'assurer que le right joystick est désactivé
        InputManager.Instance.inputs.perso.select_hackable.Disable();
    } */

    // ENABLE - DISABLE ROUTINES
    /* protected override IEnumerator enable_coroutine()
    {
        // on s'assure que le right joystick est désactivé
        InputManager.Instance.inputs.perso.select_hackable.Disable();
        yield return base.enable_coroutine();
    }
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();

        // on remet l'input de right joystick
        InputManager.Instance.inputs.perso.select_hackable.Enable();
    } */

    // REGISTER CHEST
    public void RegisterChest(Inventory chest_inv)
    {
        // on met les items dans le UI_Inventory du chest


        // on s'assure que le persoquickinventory n'affiche que les bons items
        // if (ui_chest is UI_Inventory ui_inv) { perso_quick_inventory_pool.EnableItemsByRule(ui_inv.ItemRule); }
        // else if (ui_chest is UI_SlottableMixer ui_mixer) { perso_quick_inventory_pool.EnableItemsByRule(ui_mixer.GetItemRule()); }
    }
}