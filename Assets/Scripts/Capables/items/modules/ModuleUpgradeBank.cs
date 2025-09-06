using System.Collections.Generic;
using UnityEngine;

public class ModuleUpgradeBank : Singleton<ModuleUpgradeBank>
{
    [Header("Available Upgrades per Module")]
    [SerializeField] private List<ModuleUpgrade> hdd_upgrades = new List<ModuleUpgrade>();
    [SerializeField] private List<ModuleUpgrade> cpu_upgrades = new List<ModuleUpgrade>();
    [SerializeField] private List<ModuleUpgrade> bug_os_upgrades = new List<ModuleUpgrade>();
    [SerializeField] private List<ModuleUpgrade> usb_upgrades = new List<ModuleUpgrade>();

    [Header("Log")]
    public bool log = false;


    public ModuleUpgrade GetRandomUpgrade(Module module)
    {
        switch (module.Reference)
        {
            case "module:hdd":
                return hdd_upgrades[Random.Range(0, hdd_upgrades.Count)];
            case "module:cpu":
                return cpu_upgrades[Random.Range(0, cpu_upgrades.Count)];
            case "module:bug_os":
                return bug_os_upgrades[Random.Range(0, bug_os_upgrades.Count)];
            case "module:usb":
                return usb_upgrades[Random.Range(0, usb_upgrades.Count)];
            default:
                if (log) { Debug.LogWarning($"(ModuleUpgradeBank) No upgrade available for module {module.name} with reference {module.Reference}"); }
                return null;
        }
    }
}