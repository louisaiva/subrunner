using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sink : Capable, Interactable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }

    [Header("Cooking")]
    [SerializeField] private Vector2 pot_local_position = new Vector2(0.0f, 0.0f);

    // INTERACTION
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we check the type of the interactor
        if (interactor is Pot pot) { interact_with_pot(pot); return; }

        // we toggle the oven
        if (interactor is Being)
        {
            // we check if we have a pot in our inventory and if it is filled
            pot = Inventory.GetItemsByType<Pot>().FirstOrDefault();
            if (pot == null) { return; }
            if (pot.IsFull)
            {
                // we make the interactor grab the pot
                interactor.Inventory.Grab(pot);
            }
            else
            {
                // we fill the pot
                pot.Fill();
            }
            return;
        }
    }
    private void interact_with_pot(Pot pot)
    {
        // we heat the pot
        if (debug) { Debug.Log("(Sink) placing pot " + pot.name); }

        // we grab the pot
        Inventory.Grab(pot);
        
        // then we place the pot to our capable + position it
        pot.Placed = true;
        pot.transform.localPosition = pot_local_position;
    }
}