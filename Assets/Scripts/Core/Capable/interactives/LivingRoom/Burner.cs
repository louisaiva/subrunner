using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Burner : Capable, EndlessInteractable
{

    [Header("Buner")]
    [SerializeField] private List<CapableData> pending_items_to_burn = new List<CapableData>();
    private string trash_rule = "corpse,leftover";


    // INTERACTABLE
    public InteractCapacity Interactor => null;
    public InteractType InteractionType => InteractType.Corpse;
    public void OnEndlessInteract(Capable interactor) { OnInteract(interactor); }
    public void OnInteract(Capable interactor)
    {
        if (!CanReceiveFrom(interactor, out List<Item> trashes)) { return; }
        if (AnimPlayer.IsPlaying("burn")) { return; } // we wait for the give corpse animation to be completed

        // we grab one item of the interactor and we burn it
        Item item = trashes[0];
        pending_items_to_burn.Add(item.data);
        interactor.Inventory.Drop(item, on_ground:false); // on ground false since we will destroy it, we don't want it to appear

        Debug.Log($"(Burner) {ID} received a new item to burn : '{item.ID}'");
    }
    public bool CanReceiveFrom(Capable interactor, out List<Item> trashes)
    {
        if (interactor.Inventory == null) { trashes = null; return false; }
        trashes = interactor.Inventory.GetItemsByRule(trash_rule);
        if (trashes.Count == 0) { return false; }
        return true;
    }
    public bool CanReceiveFrom(Capable interactor) { return CanReceiveFrom(interactor, out List<Item> trashes); }

    // UPDATE
    private float smoke_counter = 0f;
    private float smoke_duration_on_burn = 10f;
    protected override void Update()
    {
        base.Update();

        if (AnimPlayer.IsPlaying("burn")) { return; }
        if (pending_items_to_burn.Count == 0)
        {
            smoke_counter -= Time.deltaTime;
            update_smoke_layer();
            return;
        }

        // here we are not showing burn item and we have item to burn. we show
        AnimPlayer.Play("burn");
        smoke_counter = smoke_duration_on_burn;
        update_smoke_layer();

        // here we destroy the item
        burn_item(pending_items_to_burn[0]);
    }

    // smoke layer update
    private AnimLayer smoke_layer => AnimPlayer.GetLayerWithSkin("burner_smoke");
    private Color SmokeColor(float opacity=0f) => new Color(1f,.4f,.4f,opacity);
    private void update_smoke_layer()
    {
        if (smoke_layer == null) { return; }
        if (smoke_counter <= 0f)
        {
            smoke_counter = 0f;
            smoke_layer.Renderer.color = SmokeColor();
            return;
        }
        float opacity = smoke_counter/smoke_duration_on_burn;
        smoke_layer.Renderer.color = SmokeColor(opacity);
    }




    // BURN ITEM
    private void burn_item(CapableData item)
    {
        pending_items_to_burn.Remove(item);
        Debug.Log($"(Burner) {ID} Burning item : '{item.id}'");
        CapableEngine.LazyInstance.DespawnCapable(item);
    }

}