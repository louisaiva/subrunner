using UnityEngine;

public class Katana : Item
{
    public override void Use(Capable user)
    {
        // we check if the item is grabbed
        if (!Grabbed) { return; }

        // we check if we have an attack capacity
        AttackCapacity attack_capacity = GetCapacity<AttackCapacity>();
        if (attack_capacity == null) { return; }
        if (attack_capacity.IsAttacking) { return; }

        // we find the holder of the item
        // Capable holder = transform.parent.GetComponent<Inventory>().capable;
        // if (holder == null) { return; }

        // we use the attack capacity
        attack_capacity.Use(user);
    }
}