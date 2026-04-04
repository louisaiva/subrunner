using System.Collections;
using UnityEngine;

public class Food : Item, Usable
{
    [Header("Food parameters")]
    public float life_regen = 10f; // life regen of the food
    public event System.Action<HealthCapacity> OnBeingBitten = delegate { };

    // BEING EATEN
    public void BeEaten(HealthCapacity eater)
    {
        OnBeingBitten?.Invoke(eater);
        // Destroy(gameObject);
        CapableSystem.Instance.DespawnCapable(this.data);
        if (Holder != null) { Holder.Inventory.Remove(this); } // we remove the item from the holder's inventory
    }
    public string UseLabel { get; } = "eat";
    public void Use(Capable user)
    {
        // we get the user's eat capacity
        EatCapacity eat_capacity = user.GetCapacity<EatCapacity>();
        if (eat_capacity == null) { return; }

        // we eat the food
        eat_capacity.SetFoodTarget(this);
        eat_capacity.Use(user);
    }

    // todo : make this more with multiple food portions
}