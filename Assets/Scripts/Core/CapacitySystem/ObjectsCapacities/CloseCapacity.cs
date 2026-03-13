
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
            if ((Capable as Openable).is_moving) { return false; }
            if (!(Capable as Openable).is_open) { return false; }
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
    public override void Use(Capable capable) => close();

    // CLOSING
    protected virtual void close(bool play_anim = true)
    {
        // on supprime les invokes de l'ouverture si il y en a
        open_capacity?.CancelOpenInvoke();

        // on ouvre le coffre
        (Capable as Openable).is_moving = true;

        // on joue l'animation
        if (play_anim)
        {
            Capable.AnimPlayer.StopPlaying("idle_open");
            Capable.AnimPlayer.Play("close", duration_override: closing_duration);
        }
        Invoke("success_close", closing_duration);
        Capable.GetCapacity<HoverCapacity>()?.ChangeAnimation(hover_close_anim);

        // on joue le son
        AudioEngine.Instance.Play("close", Capable.Skin, Capable.gameObject);

        // on fait les vérifications pour les portes
        if (Capable is Door door && !door.DontTouchSortingLayer)
        {
            Capable.AnimPlayer.Renderer.sortingLayerName = "fg";
            Capable.AnimPlayer.Renderer.sortingOrder = 1;
        }

        if (log) { Debug.Log(Capable.name + " is closing..."); }
    }
    protected virtual void success_close()
    {
        // on ouvre le coffre
        (Capable as Openable).is_open = false;
        (Capable as Openable).is_moving = false;

        // on joue l'animation
        Capable.AnimPlayer.Play("idle");

        // on fait les vérifications pour les portes
        if (Capable is Door door && !door.DontTouchSortingLayer)
        {
            Capable.AnimPlayer.Renderer.sortingLayerName = "main";
            Capable.AnimPlayer.Renderer.sortingOrder = 0;
        }

        if (log) { Debug.Log(Capable.name + " is closed !"); }
    }


    // CancelInvoke
    public void CancelCloseInvoke()
    {
        if (log) { Debug.Log("(CloseCapacity) " + Capable.name + " CancelInvoke success_close"); }
        CancelInvoke("success_close");
    }
}