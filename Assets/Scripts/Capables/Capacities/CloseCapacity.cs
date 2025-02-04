
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

/// <summary>
/// CloseCapacity is a capacity that allows to close itself. can be used for chests or doors for example.
/// </summary>

public class CloseCapacity : Capacity
{
    public override bool Able
    {
        get
        {
            if ((capable as Openable).is_moving) { return false; }
            if (!(capable as Openable).is_open) { return false; }
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
        close();
    }

    
    // OPENING
    protected virtual void close()
    {
        // on joue l'animation
        capable.anim_player.StopPlaying("idle_open");
        capable.anim_player.Play("close",priority_override:3,duration_override: opening_duration);
        Invoke("success_close", opening_duration);

        // on ouvre le coffre
        (capable as Openable).is_moving = true;

        // si on est une Door on désactive le collider
        if (capable is Door)
        {
            (capable as Door).door_collider.enabled = true;
            (capable as Door).ceiling.gameObject.SetActive(true);

            // et on set le order in layer a 0 si la Door est horizontale
            if (!(capable as Door).is_vertical)
            {
                capable.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }
        }

        if (debug) { Debug.Log(capable.name + " is closing..."); }
    }

    protected virtual void success_close()
    {
        // on ouvre le coffre
        (capable as Openable).is_open = false;
        (capable as Openable).is_moving = false;

        // on joue l'animation
        capable.anim_player.Play("idle");

        // si on est une Door on désactive le ceiling
        if (capable is Door)
        {
            (capable as Door).ceiling.gameObject.SetActive(true);
        }

        if (debug) { Debug.Log(capable.name + " is closed !"); }
    }
}