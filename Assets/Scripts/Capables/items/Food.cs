using System.Collections;
using UnityEngine;

public class Food : Item
{
    [Header("Food parameters")]
    public float life_regen_per_bite = 10f; // life regen of the food per bite
    [SerializeField] protected int bites_left = 0; // number of bites left on the food
    public bool Eatable { get { return bites_left > 0; } }

    // BEING EATEN
    public void RemoveOneBite()
    {
        bites_left--;
        if (bites_left > 0)
        {
            if (debug) { Debug.Log("(Food) " + name + " has " + bites_left + " bites left"); }
            return;
        }

        // else we being fully eaten
        beingFullyEaten();
    }
    protected virtual void beingFullyEaten() { Destroy(gameObject); }
}