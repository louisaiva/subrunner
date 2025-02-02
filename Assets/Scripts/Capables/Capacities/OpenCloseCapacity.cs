
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

/// <summary>
/// OpenCloseCapacity is a capacity that allows to open/close itself. can be used for chests or doors for example.
/// </summary>

public class OpenCloseCapacity : Capacity
{
    public override bool Able
    {
        get
        {
            return !is_moving;
        }
    }

    [Header("Open Close parameters")]
    public bool is_open = false;
    public bool is_moving = false;
    public float opening_duration = 0.5f;
    public float closing_duration = 0.5f;
    
    [Header("Components")]
    protected Capable capable;

    [Header("Debug")]
    public bool debug = false;  

    // USE
    public override void Use(Capable capable)
    {
        this.capable = capable;
        if (!is_open)
        {
            open();
        }
        else
        {
            close();
        }
    }

    
    // OPENING
    protected virtual void open()
    {
        // on joue l'animation
        capable.anim_player.Play("open",priority_override:2,duration_override: opening_duration);
        Invoke("success_open", opening_duration);

        // on ouvre le coffre
        is_moving = true;

        if (debug) { Debug.Log("Chest is opening..."); }
    }

    protected virtual void success_open()
    {
        // on ouvre le coffre
        is_open = true;
        is_moving = false;

        // on joue l'animation
        capable.anim_player.Play("idle_open",priority_override:0);

        if (debug) { Debug.Log("Chest is open !"); }
    }

    protected virtual void close()
    {
        // on ferme le coffre
        is_moving = true;

        // on joue l'animation
        capable.anim_player.Play("close",priority_override:2,duration_override: closing_duration);
        Invoke("success_close", closing_duration);

        if (debug) { Debug.Log("Chest is closing..."); }
    }

    protected virtual void success_close()
    {
        // on ferme le coffre
        is_open = false;
        is_moving = false;

        // on joue l'animation
        capable.anim_player.Play("idle");

        if (debug) { Debug.Log("Chest is closed !"); }
    }
}