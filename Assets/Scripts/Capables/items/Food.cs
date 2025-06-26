using System.Collections;
using UnityEngine;

public class Food : Item
{
    [Header("Food parameters")]
    public float life_regen = 10f; // life regen of the food
    [SerializeField] private int bites = 0; // number of bites during the eating (1 will make the life_regen goes 100% in one bite)

    public void BeingEat(float duration)
    {
        // we check if the item is grabbed
        if (!Grabbed) { return; }
        if (Holder == null) { return; }
        if (Holder is not Being) { return; }

        if (debug) { Debug.Log("(Food) " + Holder.name + " is eating " + name); }

        StartCoroutine(BeingEaten(Holder as Being, duration));
    }

    IEnumerator BeingEaten(Being eater, float duration)
    {
        // we split the duration in x bites
        float bite_duration = duration / bites;
        for (int i = 0; i < bites; i++)
        {
            // we wait the bite duration
            yield return new WaitForSeconds(bite_duration);

            // we regen the life of the eater
            if (debug) { Debug.Log("(Food) " + eater.name + " is eating one bite of " + name + " for " + (life_regen / bites) + " hp"); }
            eater.AddLife(life_regen / bites);
        }

        // then we destroy the item
        if (Holder != null)
        {
            // we remove the item from the holder's inventory
            Holder.inventory.Remove(this);
        }
        Destroy(gameObject);
    }
}