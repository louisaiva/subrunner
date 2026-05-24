using UnityEngine;

public class Shoes : Item, Usable
{
    // USABLE
    public string UseLabel { get; set; } = "dodge";
    public void Use(Capable user)
    {
        // we check if the item is grabbed
        if (!Grabbed) { return; }

        // we check if we have a dodge capacity
        DodgeCapacity dodge_capacity = GetCapacity<DodgeCapacity>();
        if (dodge_capacity == null) { return; }
        if (!dodge_capacity.Able) { return; }

        // we use the dodge capacity
        dodge_capacity.Use(user);
    }
}