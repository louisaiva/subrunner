using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Laptop : Item, Usable
{
    [Header("Processor")]
    public ProcessCapacity Processor { get {
        if (processor == null) { processor = GetCapacity<ProcessCapacity>(); }
        return processor; }}
    private ProcessCapacity processor;

    [Header("Disks")]
    [SerializeField] private List<StoreCapacity> disks;
    public event System.Action<List<StoreCapacity>> OnDisksChanged = delegate { };

    [Header("Logs")]
    [SerializeField] protected bool log_keys = false;

    // KEYS MANAGEMENT
    public bool HasKeyFor(Lockable target)
    {
        return GetKeyFor(target) != null;
    }
    public Key GetKeyFor(Lockable target)
    {
        List<Key> keys = GetKeys();
        if (log_keys)
        {
            string s = $"(Laptop) {name} checking if has key for {target.Key} (security level {target.SecurityLevel})";
            foreach (Key key in keys)
            {
                s += $"\n - {key.data} ({key.key_type})";
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
        return null;
    }
    
    // USABLE
    public string UseLabel { get; } = "hack";
    public void Use(Capable user)
    {
        // we check if we have a hack capacity
        HackCapacity hack_capacity = GetCapacity<HackCapacity>();
        if (hack_capacity == null) { return; }

        // we find the holder of the item
        /* Capable holder = transform.parent.GetComponent<Inventory>().capable;
        if (holder == null) { return; } */

        // we use the hack capacity
        hack_capacity.Use(user);
    }

    // GRABBING HACK MODULE & NETWORK MODULE
    /* public void OnHackModuleChanged()
    {
        // we check how many hack modules we have in our inventory
        List<Item> hack_modules = Inventory.GetItemsByRule("module:hack");

        // remove hack capa if we don't have any hack module
        if (hack_modules.Count == 0)
        {
            if (debug) { Debug.LogWarning($"(Laptop) {name} has no hack module, removing hack capacity."); }
            if (GetCapacity<HackCapacity>() != null) { RemoveCapacity("hack"); }
            return;
        }

        // otherwise we have at least one hack module -> we ensure we have a hack capa
        HackCapacity hack_capacity = GetCapacity<HackCapacity>();
        if (hack_capacity == null)
        {
            if (debug) { Debug.LogWarning($"(Laptop) {name} has a hack module, adding hack capacity."); }
            AddCapacity("hack");
        }
    } */
    public void OnNetworkModuleChanged()
    {
        // we check how many network modules we have in our inventory
        List<Item> network_modules = Inventory.GetItemsByRule("module:network");

        // remove connect capa if we don't have any network module
        if (network_modules.Count == 0)
        {
            if (debug) { Debug.LogWarning($"(Laptop) {name} has no network module, removing connect capacity."); }
            if (GetCapacity<ConnectCapacity>() != null) { RemoveCapacity("connect"); }
            return;
        }

        // otherwise we have at least one network module -> we ensure we have a connect capa
        ConnectCapacity connect_capacity = GetCapacity<ConnectCapacity>();
        if (connect_capacity == null)
        {
            if (debug) { Debug.LogWarning($"(Laptop) {name} has a network module, adding connect capacity."); }
            AddCapacity("connect");
        }
    }

    // FILES MANAGEMENT
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
    public bool WriteFile(File file)
    {
        // we try to write the file to the first disk that has enough space
        foreach (StoreCapacity disk in disks)
        {
            if (disk.CanStore(file))
            {
                disk.Store(file);
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
        exploits.Add(Exploit.TypePassword); // we always add TypePassword as default
        foreach (StoreCapacity disk in disks)
        {
            exploits.AddRange(disk.GetExploits());
        }
        exploits.Add(Exploit.Nmap); // we always add Nmap as a default exploit
        return exploits;
    }
    public List<Key> GetKeys()
    {
        // we get all keys from all disks
        List<Key> keys = new List<Key>();
        foreach (StoreCapacity disk in disks)
        {
            keys.AddRange(disk.GetKeys());
        }
        return keys;
    }
}

[System.Serializable]
public class Key : File
{
    public string key_type; // SHA, AES, RSA

    public Key(string type, string key)
    {
        this.extension = ".key"; // default extension for keys
        key_type = type;
        this.data = key;
    }

    public bool Matches(string target_key)
    {
        return data == target_key;
    }
}