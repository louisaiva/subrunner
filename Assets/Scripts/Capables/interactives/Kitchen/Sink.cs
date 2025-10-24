using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sink : Chest, Onnable
{
    // CHEST
    public override bool is_open { get => true; }
    public override bool is_moving { get => false; }

    // START
    protected override void Start()
    {
        base.Start();

        // suscribe to Inventory Grab
        Inventory.OnItemGrabbed += (item) => handle_pot();
    }

    // INTERACTION
    public override void OnInteract(Capable interactor)
    {
        InteractCapacity old_interactor = Interactor;
        base.OnInteract(interactor);
        // we put back the interactor
        Interactor = old_interactor;

        // we check if it is a pot
        if (interactor is Pot pot) { Inventory.Grab(pot); return; }

        // we check if a being interact with the oven
        if (interactor is not Being) { return; }

        // we set the Interactor as the being finally
        Interactor = interactor.GetCapacity<InteractCapacity>();
    }
    private void handle_pot()
    {
        // check that we are flowing water
        if (!IsOn) { return; }

        // we get the pot in the hob
        Pot pot = Inventory.GetItemsByType<Pot>().FirstOrDefault();
        if (pot == null) { return; }

        // we fill it with water (will clean it if burned)
        pot.Fill();
    }


    // ONNIN / ONNOFF
    public bool IsMoving { get; set; } = false;
    public bool IsOn { get; set; } = false;
    public void PowerOn()
    {
        // we play flowing
        anim_player.Play("flowing");
        IsOn = true;

        // we try to handle pot right away
        handle_pot();
    }
    public void PowerOff()
    {
        // we stop flowing
        anim_player.StopPlaying("flowing");
        IsOn = false;
    }

    // INTERACT KEY FEEDBACK
    protected override Vector2 calculate_best_kf_position()
    {
        // we calculate the position we need to give the kf's canvas

        // 1 - we get the inventory's canvas
        Transform ui_canvas = Inventory.ui.transform.parent;
        Vector2 kf_position = ui_canvas.transform.localPosition;
        kf_position.y += 150.0f; // we move it a bit up
        return kf_position;
    }
}