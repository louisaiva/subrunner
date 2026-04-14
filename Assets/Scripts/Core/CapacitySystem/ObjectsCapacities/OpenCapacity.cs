
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
            if (Capable is not Openable openable)
            {
                Debug.LogWarning("Capable " + Capable.ID + " is not Openable !");
                return false;
            }

            if (openable.is_moving) { return false; }
            if (openable.is_open) { return false; }
            return true;
        }
    }

    [Header("Open parameters")]
    public float opening_duration = 0.5f;
    public string hover_open_anim = "hover";

    [Header("Sibling Close Capacity")]
    public CloseCapacity close_capacity;

    // USE
    public override void Use(Capable capable) => open();

    
    // OPENING
    protected virtual void open()
    {
        // on supprime les invokes de l'ouverture si il y en a
        close_capacity?.CancelCloseInvoke();

        // on ouvre le coffre
        (Capable as Openable).is_moving = true;

        // on joue l'animation
        Capable.AnimPlayer.Play("open",duration_override: opening_duration);
        Invoke("success_open", opening_duration);
        Capable.GetCapacity<HoverCapacity>()?.ChangeAnimation(hover_open_anim);

        // on joue le son
        AudioEngine.Instance.Play("open", Capable.Skin, Capable.gameObject);


        // on fait les vérifications pour les portes
        if (Capable is Door door && !door.DontTouchSortingLayer)
        {
            // on reset le layer à fg & order in layer à 1
            Capable.AnimPlayer.Renderer.sortingLayerName = "fg";
            Capable.AnimPlayer.Renderer.sortingOrder = 1;
        }

        if (log) { Debug.Log(Capable.name + " is opening..."); }
    }
    protected virtual void success_open()
    {
        // on ouvre le coffre
        (Capable as Openable).is_open = true;
        (Capable as Openable).is_moving = false;

        // on joue l'animation
        Capable.AnimPlayer.AddToPile("idle_open");

        // on fait les vérifications pour les portes
        if (Capable is Door door && !door.DontTouchSortingLayer)
        {
            // on reset le layer à main & order in layer a -1
            Capable.AnimPlayer.Renderer.sortingLayerName = "main";
            Capable.AnimPlayer.Renderer.sortingOrder = -1;
        }

        if (log) { Debug.Log(Capable.name + " is open !"); }
    }


    // CancelInvoke
    public void CancelOpenInvoke()
    {
        if (log) { Debug.Log("(OpenCapacity) " + Capable.name + " CancelInvoke success_open"); }
        CancelInvoke("success_open");
    }
}