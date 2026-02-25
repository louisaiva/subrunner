using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Laptop : Item, Usable, Device
{
    [Header("Device")]
    public ProcessCapacity Processor
    {
        get
        {
            if (processor == null) { processor = GetCapacity<ProcessCapacity>(); }
            return processor;
        }
    }
    private ProcessCapacity processor;
    public HackCapacity Hacker
    {
        get
        {
            if (hacker == null) { hacker = GetCapacity<HackCapacity>(); }
            return hacker;
        }
    }
    private HackCapacity hacker;
    public Color MB_Color { get => this.Color; }

    [Header("Disks")]
    [SerializeField] private List<StoreCapacity> disks;
    public event System.Action<List<StoreCapacity>> OnDisksChanged = delegate { };

    [Header("Logs")]
    [SerializeField] protected bool log_keys = false;

    // USABLE
    public string UseLabel { get; } = "hack";
    public void Use(Capable user)
    {
        // we check if we have a hack capacity
        HackCapacity hack_capacity = GetCapacity<HackCapacity>();
        if (hack_capacity == null) { return; }

        // we use the hack capacity
        hack_capacity.Use(user);
    }

    // MODULES MANAGEMENT
    public void OnNetworkModuleChanged()
    {
        // we check how many network modules we have in our inventory
        List<Module_Network> network_modules = Inventory.GetItemsByRule("module:network")
                                                    .Select(item => item as Module_Network)
                                                    .Where(module => module != null)
                                                    .ToList();

        // we check if we have a connect capacity
        if (network_modules.Count == 0)
        {
            Connector.Radius = 0;
            return;
        }

        // we update the radius of the connect capacity
        Connector.Radius = network_modules.Max(module => module.USB_Range);
    }
    public void OnHDD_Changed()
    {
        // we check how many hdd do we have in our inventory
        disks = Inventory.GetItemsByRule("module:hdd")
                         .Select(item => item.GetCapacity<StoreCapacity>())
                         .Where(capacity => capacity != null)
                         .ToList();

        // we call the event
        OnDisksChanged?.Invoke(disks);

        if (debug) { Debug.Log($"(Laptop) {name} HDD changed. New disks count: {disks.Count}"); }
    }

    // FILES MANAGEMENT
    public System.Action<File> OnFileWritten { get; set; } = delegate { };
    public bool WriteFile(File file)
    {
        // we try to write the file to the first disk that has enough space
        foreach (StoreCapacity disk in disks)
        {
            if (disk.CanStore(file))
            {
                disk.Store(file);
                OnFileWritten?.Invoke(file);
                return true;
            }
        }
        return false;
    }
    public List<StoreCapacity> GetDisks()
    {
        // we return the disks
        return disks;
    }
    public List<Exploit> GetExploits()
    {
        // we get all exploits from all disks
        List<Exploit> exploits = new List<Exploit>();
        List<string> exploit_refs = new List<string>();
        for (int i=0; i<disks.Count; i++)
        {
            List<Exploit> disk_exploits = disks[i].GetExploits();
            for (int j = 0; j < disk_exploits.Count; j++)
            {
                // if we already have the name, we keep the one with the highest security level
                if (exploit_refs.Contains(disk_exploits[j].name))
                {
                    int index = exploit_refs.IndexOf(disk_exploits[j].name);
                    if (disk_exploits[j].security_level > exploits[index].security_level)
                    {
                        exploits[index] = disk_exploits[j];
                    }
                    continue;
                }

                // else we add it
                exploits.Add(disk_exploits[j]);
                exploit_refs.Add(disk_exploits[j].name);
            }
        }
        exploits.Add(FileBank.Instance.Nmap);
        exploits.Add(FileBank.Instance.TypePassword);
        return exploits;
    }
    public List<File> GetFiles()
    {
        // we get all files from all disks
        List<File> files = new List<File>();
        foreach (StoreCapacity disk in disks)
        {
            files.AddRange(disk.Files);
        }
        return files;
    }

    // KEYS MANAGEMENT
    public bool HasKeyFor(Lockable target)
    {
        return GetKeyFor(target) != null;
    }
    public Key GetKeyFor(Lockable target)
    {
        List<Key> keys = get_keys();
        if (log_keys)
        {
            string s = $"(Laptop) {name} checking if has key for {target.Key}. Keys found: {keys.Count}";
            foreach (Key key in keys)
            {
                s += $"\n - {key.data}";
            }
            Debug.Log(s);
        }
        foreach (Key key in keys)
        {
            if (key.Matches(target.Password))
            {
                return key;
            }
        }
        if (log_keys) { Debug.Log($"(Laptop) {name} has no key for {target.Key}"); }
        return null;
    }
    private List<Key> get_keys()
    {
        // we get all keys from all disks
        List<Key> keys = new List<Key>();
        foreach (StoreCapacity disk in disks)
        {
            keys.AddRange(disk.GetKeys());
        }
        return keys;
    }

    // UI
    public List<WindowType> WindowsTypes => new List<WindowType>() { WindowType.Device, WindowType.Connection, WindowType.Explorer };
}

