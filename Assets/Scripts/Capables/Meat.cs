using System.Collections;
using UnityEngine;

public class Meat : Food
{
    [Header("MEAT")]
    [SerializeField] private int random_meat_modifier_at_start = 5; // meat_amount += random.range(-5,5) in the start method if this modifier = 5

    // INIT
    public void Initialize(Being being)
    {
        // random meat
        bites_left = 9;
        bites_left += Random.Range(-random_meat_modifier_at_start, random_meat_modifier_at_start);
        life_regen_per_bite = 1f; // 1 hp per bite

        // set the item reference name
        Reference = "food:meat";
        ItemDescription = "meat./. mmh by bad it's just a dead body/./l do not eat PLEASE";
        if (being is Zombo)
        {
            Reference = "food:zombo_meat";
            ItemDescription = "meat./. mmh by bad it's just a dead zombo/./l better go vegan";
        }
        MaxQty = 10;
    }

    // BEING FULLY EATEN
    protected override void beingFullyEaten() { become_bones(); }

    // BECOME BONES
    private void become_bones()
    {
        StopAllCoroutines(); // we stop all coroutines to avoid any issues
        if (anim_player.Skin == "bones") { return; } // if we are already bones, we do nothing

        if (debug) { Debug.Log("(Meat) " + name + " has become bones!"); }

        // we destroy the body child
        Destroy(transform.Find("body").gameObject);

        // and change our skin to bones
        anim_player.Skin = "bones";
        anim_player.ClearPile();
        Reference = "food:bones";
        ItemDescription = "just some bones./. nothing special here.";
    }
}