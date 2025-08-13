using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Laptop : Item, Usable
{
    [Header("Cores management")]
    [SerializeField] protected int max_cores = 0;
    private Dictionary<Hack, int> used_cores = new Dictionary<Hack, int>(); // store the nb of cores used per hack
    public int FreeCoresCount
    {
        get
        {
            int free_cores = max_cores;
            foreach (KeyValuePair<Hack, int> kvp in used_cores)
            {
                free_cores -= kvp.Value;
            }
            return free_cores;
        }
    }
    public int MaxCores => max_cores;
    public int UsedCoresCount => max_cores - FreeCoresCount;
    public event System.Action<int> OnCoresChange = delegate { };
    public event System.Action<int> OnCoresFreedOrUsed = delegate { };

    [Header("Logs")]
    [SerializeField] protected bool log_keys = false;


    // CORES MANAGEMENTS
    public bool HasFreeCores(int amount = 1)
    {
        return FreeCoresCount >= amount;
    }
    public void UseCores(Hack hack)
    {
        // we add the hack to the used cores
        used_cores[hack] = hack.exploit.cores_cost;

        OnCoresFreedOrUsed?.Invoke(hack.exploit.cores_cost);

        if (FreeCoresCount < 0)
        {
            Debug.LogWarning($"(Laptop) {name} has a core overflow !!!");
            return;
        }
    }
    public void FreeCores(Hack hack)
    {
        if (!used_cores.ContainsKey(hack))
        {
            if (debug) { Debug.LogWarning($"(Laptop) {name} tried to free cores for a hack that is not running: {hack.exploit.name}"); }
            return;
        }

        used_cores.Remove(hack);
        OnCoresFreedOrUsed?.Invoke(-hack.exploit.cores_cost);
    }

    // KEYS MANAGEMENT
    public bool HasKeyFor(Lockable target)
    {
        List<Key> keys = get_keys();
        if (log_keys)
        {
            string s = $"(Laptop) {name} checking if has key for {target.Key} (security level {target.SecurityLevel})";
            foreach (Key key in keys)
            {
                s += $"\n - {key.key} ({key.key_type})";
            }
            Debug.Log(s);
        }
        foreach (Key key in keys)
        {
            if (key.Matches(target.Key))
            {
                return true;
            }
        }
        return false;
    }
    private List<Key> get_keys()
    {
        List<Item> cards = Inventory.GetItemsByType<Card>();
        List<Key> keys = new List<Key>();
        foreach (Item item in cards)
        {
            if (item is not Card card) { continue; }
            ;
            if (card.key != null) { keys.Add(card.key); }
        }
        return keys;
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

    // GRABBING PROCESSOR MODULE
    // todo : i think it is better to have a ProcessCapacity that handles cores & etc
    public void OnCPU_Changed()
    {
        // we check how many cpu modules we have in our inventory
        List<Item> cpus = Inventory.GetItemsByRule("module:cpu");
        int new_max_cores = cpus.Count * 2; // each cpu provides 2 cores

        if (debug) { Debug.Log($"(Laptop) {name} CPU changed. New max cores: {new_max_cores} / old cores: {max_cores}"); }

        // check the difference between current and next max_cores
        if (new_max_cores >= max_cores) { set_new_max_cores(new_max_cores); return; }

        // if we have less cores, it's ok if we have have enough free cores left
        if (FreeCoresCount >= max_cores - new_max_cores) { set_new_max_cores(new_max_cores); return; }

        // otherwise we need to free some used cores
        while (FreeCoresCount < max_cores - new_max_cores)
        {
            // we free the first hack in the list
            Hack first_hack = used_cores.Keys.First();
            first_hack.Overflow();
            FreeCores(first_hack);
        }

        // finally we set the new max_cores
        set_new_max_cores(new_max_cores);
    }
    private void set_new_max_cores(int new_max_cores)
    {
        max_cores = new_max_cores;
        OnCoresChange?.Invoke(new_max_cores);
    }
}

[System.Serializable]
public class Key
{
    public string key_type; // SHA, AES, RSA
    public string key;

    public Key(string type, string key)
    {
        key_type = type;
        this.key = key;
    }

    public bool Matches(string target_key)
    {
        return key == target_key;
    }
}