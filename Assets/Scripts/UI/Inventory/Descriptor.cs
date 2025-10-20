using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this class handles multiple description FOR ITEMS
/// and allow the inventory to show item description
/// with upgrades for example
/// </summary>
public class Descriptor : MonoBehaviour
{
    [Header("Descriptions")]
    public Description name_desc;
    public Description data_desc;

    [Header("Upgrades")]
    public List<Description> upgrade_descs = new List<Description>();
    public GameObject upgrade_prefab;
    public Color tier_1_color = new Color(0.5f, 1f, 0.5f, 1f);
    public Color tier_2_color = new Color(0.5f, 0.5f, 1f, 1f);
    public Color tier_3_color = new Color(1f, 0.5f, 0.5f, 1f);
    public Color tier_4_color = new Color(1f, 0.5f, 0.5f, 1f);
    public Color tier_X_color = new Color(1f, 0.5f, 0.5f, 1f);

    [Header("Logs")]
    public bool log = false;

    // DESCRIPTION
    public void SetDescription(UI_Item ui_item)
    {
        Item item = ui_item.Item;

        // we check if we have to updates upgrades
        updateUpgrades(item);

        // we set the name & data description
        name_desc.SetDescription(item == null ? "empty slot" : item.Reference);
        data_desc.SetDescription(item == null ? "" : item.ItemDescription);
    }


    // UPGRADES MANAGEMENT
    protected void updateUpgrades(Item item)
    {
        int upgrades = 0;
        if (item != null && item is Module module)
        {
            upgrades = module.Upgrades.Count(upgrade => upgrade.tier > 0);
        }
        if (log) { Debug.Log($"(Descriptor) updating upgrades for {item} found {upgrades} upgrades"); }

        // we check if we have no upgrade at all
        if (upgrades == 0)
        {
            // we destroy all upgrade descs
            foreach (var desc in upgrade_descs) { Destroy(desc.gameObject); }
            upgrade_descs.Clear();
            if (log) { Debug.Log($"(Descriptor) no upgrades, clearing all descs"); }
            return;
        }


        // we check if we need to adjust slightly the upgrade count
        if (upgrades != upgrade_descs.Count)
        {
            int delta = upgrades - upgrade_descs.Count;
            if (log) { Debug.Log($"(Descriptor) adjusting upgrade descs by {delta}"); }
            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                {
                    Description desc = Instantiate(upgrade_prefab, transform).GetComponent<Description>();
                    upgrade_descs.Add(desc);
                }
            }
            else
            {
                for (int i = 0; i < -delta; i++)
                {
                    Description desc = upgrade_descs[upgrade_descs.Count - 1];
                    upgrade_descs.RemoveAt(upgrade_descs.Count - 1);
                    Destroy(desc.gameObject);
                }
            }
            data_desc.transform.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            if (log) { Debug.Log($"(Descriptor) now have {upgrade_descs.Count} upgrade descs"); }
        }

        // we update the upgrade for each upgrade
        List<ModuleUpgrade> module_upgrades = (item as Module).Upgrades.Where(upgrade => upgrade.tier > 0).ToList();
        for (int i = 0; i < module_upgrades.Count; i++)
        {
            updateUpgrade(upgrade_descs[i], module_upgrades[i]);
        }
    }
    public void updateUpgrade(Description desc, ModuleUpgrade upgrade)
    {
        // we set the description
        if (log) { Debug.Log($"(Descriptor) updating upgrade {upgrade.name} to tier {upgrade.tier}"); }
        string roman = convert_to_roman(upgrade.tier);
        if (roman == "") { roman = "0"; }
        string effect = upgrade.effect.ToString(upgrade.precision) + upgrade.effect_unit;
        desc.SetDescription($"{effect} {upgrade.name} ({roman})");

        // we set the color
        switch (upgrade.tier)
        {
            case 0:
                desc.SetColor(Color.white);
                break;
            case 1:
                desc.SetColor(tier_1_color);
                break;
            case 2:
                desc.SetColor(tier_2_color);
                break;
            case 3:
                desc.SetColor(tier_3_color);
                break;
            case 4:
                desc.SetColor(tier_4_color);
                break;
            default:
                desc.SetColor(tier_X_color);
                break;
        }
    }

    private string convert_to_roman(int number)
    {
        if (number < 1) return string.Empty;
        if (number >= 10) return "X" + convert_to_roman(number - 10);
        if (number == 9) return "IX";
        if (number >= 5) return "V" + convert_to_roman(number - 5);
        if (number == 4) return "IV";
        if (number >= 1) return "I" + convert_to_roman(number - 1);
        return string.Empty;
    }
}