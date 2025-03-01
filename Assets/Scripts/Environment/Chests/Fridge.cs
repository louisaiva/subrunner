using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class Fridge : InteractChest
{

    // unity functions
    protected override void Awake()
    {
        anims = new FridgeAnims();
        // randomize_cat = "food_drink";
        base.Awake();
    }

}

public class FridgeAnims : ChestAnims
{
    // constructor
    public FridgeAnims()
    {
        idle_open = "fridge_idle_open";
        idle_closed = "fridge_idle_closed";
        openin = "fridge_openin";
        closin = "fridge_closin";
    }

}