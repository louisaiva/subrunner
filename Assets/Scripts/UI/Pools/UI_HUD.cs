using UnityEngine;

public class UI_HUD : UI_Pool
{
    [Header("Perso Quick Inventory")]
    public UI_Inventory perso_quick_inventory;
    private HUD_PersoItemPool perso_quick_inventory_pool;

    [Header("Components")]
    [SerializeField] private UI_Inventory ui_chest;
    public UI_Notifier Notifier;
}