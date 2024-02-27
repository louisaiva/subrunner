using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class TrashContainer : InteractChest
{

    // unity functions
    protected override void Awake()
    {
        anims = new TrashContainerAnims();
        // randomize_cat = "food_drink";
        base.Awake();
    }

}

public class TrashContainerAnims : ChestAnims
{
    // constructor
    public TrashContainerAnims()
    {
        idle_open = "trash_container_idle_opened";
        idle_closed = "trash_container_idle_closed";
        openin = "trash_container_openin";
        closin = "trash_container_closin";
    }

}