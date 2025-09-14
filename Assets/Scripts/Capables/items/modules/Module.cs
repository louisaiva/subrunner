using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Hardware item that can be installed on a motherboard
/// can be upgraded and merged with other same reference modules
/// at the beginning every module has all its upgrades, but they are on tier 0 (or 1?)
/// </summary>
public class Module : Item
{

    [Header("Upgrades")]
    [SerializeField] private List<ModuleUpgrade> upgrades = new List<ModuleUpgrade>();
    public List<ModuleUpgrade> Upgrades => upgrades;

    // UPGRADING
    public bool HasSameUpgrades(Module other)
    {
        if (other == null) { return false; }
        if (other.Upgrades.Count != Upgrades.Count) { return false; }

        for (int i = 0; i < upgrades.Count; i++)
        {
            if (other.Upgrades[i].tier != Upgrades[i].tier) { return false; }
        }

        return true;
    }
    public void MergeWith(Module other)
    {
        // check if we can merge
        if (other == null) { return; }
        if (other.Reference != Reference) { return; }
        if (other.Upgrades.Count != Upgrades.Count) { return; }

        // we merge the upgrades
        for (int i = 0; i < upgrades.Count; i++)
        {
            upgrades[i].tier += other.Upgrades[i].tier;
        }

        // apply random upgrade
        Upgrade();
        Destroy(other.gameObject);
    }
    public void Upgrade()
    {
        // we get a random upgrade
        ModuleUpgrade upgrade = upgrades[UnityEngine.Random.Range(0, upgrades.Count)];
        upgrade.Upgrade();
        if (debug) { Debug.Log($"(Module) {name} upgraded {upgrade.name} to tier {upgrade.tier}"); }

        // we apply the upgrade
        apply_upgrade();
    }

    // LOW UPGRADE
    protected virtual void apply_upgrade() { }
    protected float get_upgrade_effect(string upgrade_name)
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade.name == upgrade_name)
            {
                return upgrade.effect;
            }
        }
        return 0;
    }
}

[Serializable]
public class ModuleUpgrade
{
    public string name = "upgrade";
    public int tier = 1;
    public float effect = 1; // generic effect value
    public string precision = "F0";
    public string effect_unit = ""; // unit of the effect (%, MB, units, etc)

    public virtual void Upgrade()
    {
        tier++;

        if (name == "storage capacity")
        {
            effect = 4.096f + 1.024f * tier; // in MB
            if (tier >= 3) { effect = 8.192f + 2.048f * (tier - 3); }
        }
        else if (name == "usb cable")
        {
            effect = tier + 1; // in number of unity units
        }
        else if (name == "cores")
        {
            effect = 2 * (tier + 1); // in number of cores
        }
        else if (name == "process speed")
        {
            effect = 100;
            if (tier == 1) { effect = 110; }
            else if (tier == 2) { effect = 125; }
            else if (tier == 3) { effect = 145; }
            else if (tier > 3) { effect = 145 + 20 * (tier - 3); }
        }
        else if (name == "bruteforce speed"
                || name == "overheat damage"
                || name == "ddos army speed")
        {
            effect = 100 + 25 * tier; // in percentage
            if (tier >= 3) { effect = 200 + 50 * (tier - 3); }
        }
    }
}