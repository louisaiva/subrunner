using UnityEngine;

public class UI_HUD : UI_Pool
{
    [Header("Perso Quick Inventory")]
    public UI_Inventory perso_quick_inventory;
    [SerializeField] private bool show_perso_quick_inventory = true;
    private HUD_PersoItemPool perso_quick_inventory_pool;

    [Header("Components")]
    [SerializeField] private UI_Inventory ui_chest;

    protected void Awake()
    {
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

    // SHOW / HIDE
    public override async Awaitable Show(float duration)
    {
        // on affiche le perso_quick_inventory
        if (show_perso_quick_inventory || ui_chest != null)
        {
            perso_quick_inventory.Show();
        }
        await base.Show(duration);

        if (ui_chest != null)
        {
            // ça veut dire qu'on interagit avec un chest,
            // on doit les activer du xbox navigator
            UI_XboxNavigator.Instance.Enable(ui_chest,true);

            // UI_XboxNavigator.Instance.angle_threshold = base.angle_threshold;
        }
    }
    public override async Awaitable Hide(float duration)
    {
        if (ui_chest != null)
        {
            // ça veut dire qu'on interagit avec un chest,
            // on doit les désactiver du xbox navigator
            UI_XboxNavigator.Instance.Disable(ui_chest);
            UI_XboxNavigator.Instance.Disable(perso_quick_inventory);
        }

        perso_quick_inventory.Hide();
        await base.Hide(duration);
    }

    // REGISTER CHEST
    public async void RegisterChest(UI_Inventory ui_chest)
    {
        // on ajoute le chest au pool
        RegisterToPool(ui_chest.gameObject);
        this.ui_chest = ui_chest;

        if (Showed)
        {
            // on active le perso_quick_inventory si on doit l'afficher
            if (!perso_quick_inventory.gameObject.activeSelf)
            {
                perso_quick_inventory.Show();
            }

            // on active seulement les ui_items qui matche la rule du chest !
            perso_quick_inventory_pool.EnableItemsByRule(ui_chest.ItemRule);

            // on active le ui_navigator pour le perso_quick_inventory
            await System.Threading.Tasks.Task.Yield(); // wait for the next frame to ensure the UI is active
            UI_XboxNavigator.Instance.Enable(perso_quick_inventory, true);
        }
    }
    public void RemoveChest(UI_Inventory ui_chest)
    {
        // on enlève le chest du pool
        QuitPool(ui_chest.gameObject);
        this.ui_chest = null;
        if (Showed)
        {
            UI_XboxNavigator.Instance.Disable(perso_quick_inventory);

            // on desactive le perso_quick_inventory si on doit l'afficher
            if (!show_perso_quick_inventory)
            {
                perso_quick_inventory.Hide();
            }
            // on active tous les ui_items
            perso_quick_inventory_pool.EnableAllItems();
        }
    }
}