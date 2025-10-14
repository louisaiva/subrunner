using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_HUD : UI_Pool
{
    [Header("Perso Quick Inventory")]
    public UI_Inventory perso_quick_inventory;
    private HUD_PersoItemPool perso_quick_inventory_pool;

    [Header("Components")]
    [SerializeField] private UI_Inventory ui_chest;

    // AWAKE & START
    protected override void Awake()
    {
        base.Awake();

        // on récupère les composants
        if (perso_quick_inventory == null)
        {
            perso_quick_inventory = transform.Find("perso_quick_inventory").GetComponent<UI_Inventory>();
        }
        if (perso_quick_inventory == null)
        {
            Debug.LogWarning("(UI_HUD) missing perso_quick_inventory on " + name);
            return;
        }

        perso_quick_inventory_pool = perso_quick_inventory.GetComponent<HUD_PersoItemPool>();
    }
    protected void Start()
    {
        // on met les callbacks pour vérifier que le select_hackable se désactive bien
        InputManager.Instance.OnPersoInputsToggled += verify_right_joy_is_disabled;
    }

    // SHOW / HIDE
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null)
    {
        // on affiche le perso_quick_inventory
        if (ui_chest != null)
        {
            ui_chest.Show();
            perso_quick_inventory.Show();
        }

        // attend que la pool s'affiche
        yield return base.show_coroutine(dont_show);
    }
    protected override IEnumerator hide_coroutine(List<GameObject> dont_hide = null)
    {
        if (ui_chest != null)
        {
            perso_quick_inventory.Hide();
            ui_chest.Hide();
        }

        yield return base.hide_coroutine(dont_hide);
    }

    // REGISTER CHEST
    public void RegisterChest(UI_Inventory ui_chest)
    {
        // on ajoute le chest au pool
        ui_chest.Show();
        this.ui_chest = ui_chest;

        // on s'assure que le right joystick est désactivé
        InputManager.Instance.inputs.perso.select_hackable.Disable();

        if (!Showed) { return; }

        // on active seulement les ui_items qui matche la rule du chest !
        perso_quick_inventory.Show();
        perso_quick_inventory_pool.EnableItemsByRule(ui_chest.ItemRule);
    }
    public void RemoveChest(UI_Inventory ui_chest)
    {
        // on enlève le chest du pool
        ui_chest.Hide();
        this.ui_chest = null;
        if (!Showed) { return; }

        // on active tous les ui_items
        perso_quick_inventory.Hide();
        perso_quick_inventory_pool.EnableAllItems();

        // on remet l'input de right joystick
        InputManager.Instance.inputs.perso.select_hackable.Enable();
    }
    private void verify_right_joy_is_disabled(bool perso_inputs_enabled)
    {

        // todo : is there a better way to do this ? looks schlag. maybe it's better to bring back enhanced_perso map ? or hacking map ?

        if (!perso_inputs_enabled) { return; }
        if (!Showed) { return; }
        if (ui_chest == null) { return; }

        // on doit s'assurer que le right joystick est désactivé
        InputManager.Instance.inputs.perso.select_hackable.Disable();
    }
}