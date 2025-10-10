using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this UI_ItemPool is a special one that can dynamically enable/disable
/// ui_items based on a given item rule. useful for the Perso quick inventory
/// shown in the HUD pool. When we open a fridge we don't want our katana
/// to be shown bcz we can't put it inside lol
/// </summary>
public class HUD_PersoItemPool : UI_ItemPool
{
    [Header("Perso Item Pool")]
    [SerializeField] private string dynamic_rule = "";

    public void EnableAllItems()
    {
        foreach (UI_Item ui_item in ui_items)
        {
            if (ui_item.Item == null) { continue; }
            ui_item.gameObject.SetActive(true);
        }
        dynamic_rule = "";
    }
    public void EnableItemsByRule(string rule)
    {
        foreach (UI_Item ui_item in ui_items)
        {
            if (ui_item.Item == null) { continue; }

            if (ui_item.Item.ValidateRule(rule)) { ui_item.gameObject.SetActive(true); }
            else { ui_item.gameObject.SetActive(false); }
        }
        dynamic_rule = rule;
    }
}