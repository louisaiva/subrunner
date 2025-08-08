
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

    [Header("Close parameters")]
    public float closing_duration = 0.5f;

    [Header("Sibling Open Capacity")]
    public OpenCapacity open_capacity;

    [Header("Components")]
    private UI_HUD hud;

    // START
    private void Start()
    {
        hud = UI_Manager.Instance.GetPool("hud") as UI_HUD;
        close(false);
    }

    // USE
    public override void Use(Capable capable)
    {
        close();
    }

    // OPENING
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

        // on fait les vérifications pour les portes
        if (capable is Door)
        {
            capable.GetComponent<SpriteRenderer>().sortingLayerName = "fg";
            capable.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
        else if (capable is Chest && capable.Inventory != null && capable.Inventory.ui != null)
        {
            capable.Inventory.ui.Hide();
            if (hud != null) { hud.RemoveChest(capable.Inventory.ui); }
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

        // on fait les vérifications pour les portes
        if (capable is Door)
        {
            capable.GetComponent<SpriteRenderer>().sortingLayerName = "main";
            capable.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }

        // verifications pour les lootable meat
        if (capable is LootableMeat lootableMeat && lootableMeat.Inventory.Count == 0)
        {
            lootableMeat.TurnToMeat();
        }
                                                        
        if (debug) { Debug.Log(capable.name + " is closed !"); }
    }


    // CancelInvoke
    public void CancelCloseInvoke()
    {
        if (debug) { Debug.Log("(CloseCapacity) " + capable.name + " CancelInvoke success_close"); }
        CancelInvoke("success_close");
    }
}