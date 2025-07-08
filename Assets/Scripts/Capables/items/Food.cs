using System.Collections;
using UnityEngine;

public class Food : Item
{
    [Header("Food parameters")]
    public float life_regen_per_bite = 10f; // life regen of the food per bite
    [SerializeField] protected int bites_left = 0; // number of bites left on the food
    public bool Eatable { get { return bites_left > 0; } }

    // BEING BITTEN
    public void BeingBitten(Being being, float duration)
    {
        if (debug) { Debug.Log("(Food) " + being.name + " is eating " + name); }

        StartCoroutine(being_bitten(being, duration));
    }
    protected virtual IEnumerator being_bitten(Being eater, float duration)
    {
        // we wait for the eat animation to finish
        yield return new WaitForSeconds(duration);

        // we regen the life of the eater
        if (debug) { Debug.Log("(Food) " + eater.name + " is eating one bite of " + name + " for " + life_regen_per_bite + " hp"); }
        eater.AddLife(life_regen_per_bite);
        if (eater is IA ia) { ia.hunger -= life_regen_per_bite; } // if the eater is an IA, we reduce its hunger

        // check if there is still some bites left
        bites_left--;
        if (bites_left > 0) { yield break; }

        // we call the being fully eaten
        beingFullyEaten(eater);
    }
    protected virtual void beingFullyEaten(Being eater) { Destroy(gameObject); }
}