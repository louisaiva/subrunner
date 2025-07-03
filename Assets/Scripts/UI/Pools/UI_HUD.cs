using UnityEngine;

public class UI_HUD : UI_Pool
{
    [Header("Inventory Menu Components")]
    [SerializeField] private UI_Inventory perso_quick_inventory;
    [SerializeField] private UI_Inventory ui_chest;
    [SerializeField] private UI_XboxNavigator navigator;

    protected override void Awake()
    {
        // on récupère le navigator
        navigator = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        // on récupère les composants
        perso_quick_inventory = transform.Find("perso_quick_inventory").GetComponent<UI_Inventory>();
        if (perso_quick_inventory == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing perso_quick_inventory on " + name);
        }
        
        base.Awake();
    }

    // SHOW / HIDE
    public override void Show()
    {
        base.Show();

        if (ui_chest != null)
        {
            // ça veut dire qu'on interagit avec un chest,
            // on doit les activer du xbox navigator
            navigator.Enable(ui_chest,true);
            // navigator.Enable(perso_quick_inventory,true);

            // on désactive les perso useconso
            inputs.perso.useConso.Disable();

            navigator.angle_threshold = base.angle_threshold;
        }
    }
    public override void Hide()
    {
        base.Hide();

        if (ui_chest != null)
        {
            // ça veut dire qu'on interagit avec un chest,
            // on doit les désactiver du xbox navigator
            navigator.Disable(ui_chest);
            navigator.Disable(perso_quick_inventory);

            // on réactive les perso useconso
            inputs.perso.useConso.Enable();
        }
    }

    // REGISTER CHEST
    public void RegisterChest(UI_Inventory ui_chest)
    {
        // on ajoute le chest au pool
        RegisterToPool(ui_chest.gameObject);
        this.ui_chest = ui_chest;

        // on désactive les perso useconso si on est showed
        if (Showed) { inputs.perso.useConso.Disable(); }
    }
    public void RemoveChest(UI_Inventory ui_chest)
    {
        // on enlève le chest du pool
        QuitPool(ui_chest.gameObject);
        this.ui_chest = null;
        if (Showed) { inputs.perso.useConso.Enable(); }
    }
}