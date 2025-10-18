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
    public UI_Notifier Notifier;

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

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        if (ui_chest == null) { yield break; }

        // on active le perso_quick_inventory & chest navigator
        UI_XboxNavigator.Instance.Enable(ui_chest,ingame_navigation: true);
        UI_XboxNavigator.Instance.Enable(perso_quick_inventory,ingame_navigation: true);
    }
    protected override IEnumerator disable_coroutine()
    {
        if (ui_chest == null) { yield break; }

        // on active le perso_quick_inventory & chest navigator
        UI_XboxNavigator.Instance.Disable(ui_chest);
        UI_XboxNavigator.Instance.Disable(perso_quick_inventory);
    }


    // REGISTER CHEST
    public void RegisterChest(UI_Inventory ui_chest)
    {
        // on ajoute le chest au pool
        // ui_chest.Show();
        this.ui_chest = ui_chest;
        perso_quick_inventory_pool.EnableItemsByRule(ui_chest.ItemRule);

        // on ajoute le chest & persoquickinv au pool
        RegisterToPool(ui_chest.gameObject);
        RegisterToPool(perso_quick_inventory.gameObject);

        // on s'assure que le right joystick est désactivé
        InputManager.Instance.inputs.perso.select_hackable.Disable();

        if (!Showed) { return; }
        StartCoroutine(enable_coroutine());
    }
    public void RemoveChest(UI_Inventory ui_chest)
    {
        // on enlève le chest
        StartCoroutine(disable_coroutine());

        // on ajoute le chest & persoquickinv au pool
        QuitPool(ui_chest.gameObject,hide_element:true);
        QuitPool(perso_quick_inventory.gameObject,hide_element:true);

        this.ui_chest = null;

        // on active tous les ui_items
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