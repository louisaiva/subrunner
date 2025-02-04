
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

/// <summary>
/// OpenCapacity is a capacity that allows to open itself. can be used for chests or doors for example.
/// </summary>

public class OpenCapacity : Capacity
{
    public override bool Able
    {
        get
        {
            if ((capable as Openable).is_moving) { return false; }
            if ((capable as Openable).is_open) { return false; }
            return true;
        }
    }

    [Header("Open parameters")]
    public float opening_duration = 0.5f;

    [Header("Debug")]
    public bool debug = false;  

    // USE
    public override void Use(Capable capable)
    {
        open();
    }

    
    // OPENING
    protected virtual void open()
    {
        // on joue l'animation
        capable.anim_player.Play("open",priority_override:3,duration_override: opening_duration);
        Invoke("success_open", opening_duration);

        // on ouvre le coffre
        (capable as Openable).is_moving = true;

        if (capable is Door)
        {
            // si on est une Door on désactive le collider
            (capable as Door).door_collider.enabled = false;
            // si on est une Door on désactive le ceiling
            (capable as Door).ceiling.gameObject.SetActive(false);
        }

        if (debug) { Debug.Log(capable.name + " is opening..."); }
    }

    protected virtual void success_open()
    {
        // on ouvre le coffre
        (capable as Openable).is_open = true;
        (capable as Openable).is_moving = false;

        // on joue l'animation
        capable.anim_player.Play("idle_open");

        if (capable is Door)
        {
            // et on set le order in layer a -1
            if (!(capable as Door).is_vertical)
            {
                capable.GetComponent<SpriteRenderer>().sortingOrder = -1;
            }
        }

        if (debug) { Debug.Log(capable.name + " is open !"); }
    }
}