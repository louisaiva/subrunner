using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Laptop : Item
{
    [Header("Laptop parameters")]
    [SerializeField] protected int max_cores = 4;
    [SerializeField] protected int free_cores = 4;

    // CORES MANAGEMENTS
    public bool HasFreeCores(int amount = 1)
    {
        return free_cores >= amount;
    }
    public async void UseCores(int amount, float duration)
    {
        free_cores -= amount;
        if (free_cores < 0)
        {
            Debug.LogWarning($"(Laptop) {name} has a core overflow !!!");
            return;
        }

        // simulate core usage over time
        await Task.Delay((int)(duration * 1000));
        free_cores += amount;
    }

    // KEYS MANAGEMENT
    public bool HasKeyFor(Hackable target)
    {
        List<Key> keys = get_keys();
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

    // USING ITEM
    public override void Use(Capable user)
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