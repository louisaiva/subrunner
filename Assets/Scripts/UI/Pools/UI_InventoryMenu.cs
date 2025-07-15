using UnityEngine;

public class UI_InventoryMenu : UI_Pool
{
    [Header("Inventory Menu Components")]
    [SerializeField] private UI_Inventory ui_inventory;
    [SerializeField] private UI_Inventory ui_laptop;
    [SerializeField] private UI_XboxNavigator navigator;

    protected override void Awake()
    {
        // on récupère le navigator
        navigator = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        // on récupère les composants
        ui_inventory = GetComponent<UI_Inventory>();
        if (ui_inventory == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_inventory on " + name);
        }
        ui_laptop = transform.Find("ui_laptop").GetComponent<UI_Inventory>();
        if (ui_laptop == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_laptop on " + name);
        }
        
        base.Awake();
    }

    public override void Show()
    {
        base.Show();

        // on active le navigator
        navigator.Enable(ui_inventory);
        navigator.Enable(ui_laptop);

        // on arrête le temps
        Time.timeScale = 0;

        // on met à jour l'angle treshold du navigator
        navigator.angle_threshold = base.angle_threshold;
        
    }
    public override void Hide()
    {
        base.Hide();

        // on désactive le navigator
        navigator.Disable(ui_inventory);
        navigator.Disable(ui_laptop);

        // on remet le temps
        Time.timeScale = 1;
    }

}