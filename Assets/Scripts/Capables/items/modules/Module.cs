using System;
using System.Collections.Generic;
using UnityEngine;

public class Module : Item
{

    [SerializeField] private List<ModuleUpgrade> upgrades = new List<ModuleUpgrade>();
    public List<ModuleUpgrade> Upgrades => upgrades;

    public void MergeWith(Module other)
    {
        // check if we can merge
        if (other == null) { return; }
        if (other.Reference != Reference) { return; }

        // we merge the upgrades
        add_upgrades(other.Upgrades);
        upgrade(); // apply random upgrade
        Destroy(other.gameObject);
    }

    // UPGRADING
    protected void upgrade()
    {
        // we get a random upgrade from the bank
        ModuleUpgrade upgrade = ModuleUpgradeBank.Instance.GetRandomUpgrade(this);
        if (upgrade == null) { return; }

        // we check if we already have a same name upgrade
        foreach (ModuleUpgrade u in upgrades)
        {
            if (u.name != upgrade.name) { continue; }

            // we upgrade the current upgrade by one tier
            u.tier += upgrade.tier;
            if (debug) { Debug.Log($"(Module) {name} upgraded {u.name} to tier {u.tier}"); }
            return;
        }

        // we add the new upgrade
        upgrades.Add(upgrade);
        if (debug) { Debug.Log($"(Module) {name} added new upgrade {upgrade.name}"); }
    }

    protected void add_upgrades(List<ModuleUpgrade> new_upgrades)
    {
        foreach (ModuleUpgrade new_upgrade in new_upgrades)
        {
            bool found = false;
            foreach (ModuleUpgrade u in upgrades)
            {
                if (u.name != new_upgrade.name) { continue; }

                // we upgrade the current upgrade by one tier
                u.tier += new_upgrade.tier;
                found = true;
                if (debug) { Debug.Log($"(Module) {name} merged upgrade {u.name} to tier {u.tier}"); }
                break;
            }
            if (!found)
            {
                upgrades.Add(new_upgrade);
                if (debug) { Debug.Log($"(Module) {name} added new upgrade {new_upgrade.name}"); }
            }
        }
    }

}

[Serializable] public class ModuleUpgrade
{
    public string name = "upgrade";
    public int tier = 1;
}