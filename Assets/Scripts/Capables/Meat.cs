using System.Collections;
using UnityEngine;

public class Meat : Food
{
    [Header("MEAT")]
    [SerializeField] private int random_meat_modifier_at_start = 5; // meat_amount += random.range(-5,5) in the start method if this modifier = 5

    // INIT
    public void Initialize()
    {
        // random meat
        bites_left = 9;
        bites_left += Random.Range(-random_meat_modifier_at_start, random_meat_modifier_at_start);
        life_regen_per_bite = 1f; // 1 hp per bite

        // set the item reference name
        Reference = "food:meat";
        ItemDescription = "meat./. mmh by bad it's just a dead body/./l do not eat PLEASE";
        MaxQty = 10;
    }

    // BEING BITTEN
    protected override IEnumerator being_bitten(Being eater, float duration)
    {
        // we wait for the eat animation to finish
        yield return new WaitForSeconds(duration);

        // we regen the life of the eater
        if (debug) { Debug.Log("(Meat) " + eater.name + " is eating one bite of " + name + " for " + life_regen_per_bite + " hp"); }
        eater.AddLife(life_regen_per_bite);

        // check if there is still some bites left
        bites_left--;
        if (bites_left > 0) { yield break; }

        // if not we destroy the item
        become_bones();
    }

    // BECOME BONES
    private void become_bones()
    {
        if (debug) { Debug.Log("(Meat) " + name + " has become bones!"); }

        // we destroy the body child
        Destroy(transform.Find("body").gameObject);

        // and change our skin to bones
        anim_player.skin = "bones";
        anim_player.ClearPile();
    }
}