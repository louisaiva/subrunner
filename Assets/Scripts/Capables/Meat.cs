using UnityEngine;

public class Meat : Movable
{
    [Header("MEAT")]
    [SerializeField] private int meat_amount = 10; // amount of meat on the body that being can eat (if they want of course)
    [SerializeField] private int random_meat_modifier_at_start = 5; // meat_amount += random.range(-5,5) in the start method if this modifier = 5
    public bool Eatable { get { return meat_amount > 0; } }

    // START
    protected override void Start()
    {
        base.Start();

        // random meat
        meat_amount += Random.Range(-random_meat_modifier_at_start, random_meat_modifier_at_start);
    }

    // BEING EATEN
    public bool BeingEaten(Being eater)
    {
        if (meat_amount <= 0) { return false; } // no meat left to eat

        // we lose one meat
        meat_amount--;

        if (debug) { Debug.Log("(Meat) eaten by " + eater.name + ". Meat amount left: " + meat_amount); }

        if (meat_amount <= 0)
        {
            become_bones();
        }

        return true; // we successfully ate the meat
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