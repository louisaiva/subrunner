
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

/// <summary>
/// CloseCapacity is a capacity that allows to close itself. can be used for chests or doors for example.
/// </summary>

// todo : maybe re-unify the both open & close capacity since it's a one only system it would make sense..
// todo : and we should stop using Do("") and instead call GetCapacity<>().Open() / .Close() it would be better ahah
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

    [Header("Close parameters")]
    public float closing_duration = 0.5f;
    public string hover_close_anim = "hover";

    [Header("Sibling Open Capacity")]
    public OpenCapacity open_capacity;

    // START
    private void Start()
    {
        close(false);
    }

    // USE
    public override void Use(Capable capable)
    {
        close();
    }

    // CLOSING
    protected virtual void close(bool play_anim = true)
    {
        // on supprime les invokes de l'ouverture si il y en a
        open_capacity?.CancelOpenInvoke();

        // on ouvre le coffre
        (capable as Openable).is_moving = true;

        // on joue l'animation
        if (play_anim)
        {
            capable.anim_player.StopPlaying("idle_open");
            capable.anim_player.Play("close", duration_override: closing_duration);
        }
        Invoke("success_close", closing_duration);
        capable.GetCapacity<HoverCapacity>()?.ChangeAnimation(hover_close_anim);

        // on joue le son
        AudioEngine.Instance.Play("close", capable.Skin, capable.gameObject);

        // on fait les vérifications pour les portes
        if (capable is Door door && !door.DontTouchSortingLayer)
        {
            capable.GetComponent<SpriteRenderer>().sortingLayerName = "fg";
            capable.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }

        if (log) { Debug.Log(capable.name + " is closing..."); }
    }
    protected virtual void success_close()
    {
        // on ouvre le coffre
        (capable as Openable).is_open = false;
        (capable as Openable).is_moving = false;

        // on joue l'animation
        capable.anim_player.Play("idle");

        // on fait les vérifications pour les portes
        if (capable is Door door && !door.DontTouchSortingLayer)
        {
            capable.GetComponent<SpriteRenderer>().sortingLayerName = "main";
            capable.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }

        if (log) { Debug.Log(capable.name + " is closed !"); }
    }


    // CancelInvoke
    public void CancelCloseInvoke()
    {
        if (log) { Debug.Log("(CloseCapacity) " + capable.name + " CancelInvoke success_close"); }
        CancelInvoke("success_close");
    }
}