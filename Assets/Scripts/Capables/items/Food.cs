using System.Collections;
using UnityEngine;

public class Food : Item, Usable
{
    [Header("Food parameters")]
    public float life_regen_per_bite = 10f; // life regen of the food per bite
    [SerializeField] protected int bites_left = 0; // number of bites left on the food

    public bool Eatable { get { return bites_left > 0; } }
    public event System.Action<Being> OnBeingBitten = delegate { };

    // BEING EATEN
    public void RemoveOneBite(Being eater)
    {
        bites_left--;
        OnBeingBitten?.Invoke(eater);
        if (bites_left > 0)
        {
            if (debug) { Debug.Log("(Food) " + name + " has " + bites_left + " bites left"); }
            return;
        }

        // else we being fully eaten
        Destroy(gameObject);
        // beingFullyEaten();
    }
    // protected virtual void beingFullyEaten() { Destroy(gameObject); }


    // USABLE
    public string UseLabel { get; } = "eat";
    public void Use(Capable user)
    {
        if (!Eatable) { return; }

        // we get the user's eat capacity
        EatCapacity eat_capacity = user.GetCapacity<EatCapacity>();
        if (eat_capacity == null) { return; }

        // we eat the food
        eat_capacity.SetFoodTarget(this);
        eat_capacity.Use(user);
    }
}