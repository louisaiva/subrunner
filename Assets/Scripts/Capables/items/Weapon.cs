using UnityEngine;

public class Weapon : Item, Usable
{
    // USABLE
    public string UseLabel { get; set; } = "attack";
    public void Use(Capable user)
    {
        // we check if the item is grabbed
        if (!Grabbed) { return; }

        // we check if we have an attack capacity
        AttackCapacity attack_capacity = GetCapacity<AttackCapacity>();
        if (attack_capacity == null) { return; }
        if (attack_capacity.IsAttacking) { return; }

        // we use the attack capacity
        attack_capacity.Use(user);
    }
}