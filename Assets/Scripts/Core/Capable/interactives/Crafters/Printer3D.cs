using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer3D : Container, Interactable
{
    public InteractType InteractionType => InteractType.InteractableByMobsOnly;
    public InteractCapacity Interactor => null;
    public void OnInteract(Capable interactor) { }




    // PRINT MAIN METHOD
    public bool Print(PastaMeal meal)
    {
        if (print_coroutine != null) { return false; }

        // first we grab all the items of the meal, so they are removed from orderer's inventory
        List<Item> items = meal.GetItems();
        foreach(Item item in items) { Inventory.Grab(item); }

        // starts the coroutine
        print_coroutine = StartCoroutine(print_meal());
        return true;
    }

    // print coroutine
    public Vector2 plate_position = new Vector2(0f, 0.6666667f);
    private Coroutine print_coroutine = null;
    private IEnumerator print_meal()
    {
        // we destroy all our items
        List<Item> items = Inventory.GetAllItems();
        foreach (Item item in items) { CapableEngine.Instance.DespawnCapable(item.data); }

        AnimPlayer.Play("open");
        while (AnimPlayer.IsPlaying("open")) { yield return null; }
        AnimPlayer.Play("craft");
        while (AnimPlayer.IsPlaying("craft")) { yield return null; }

        Pasta pastas = CapableEngine.Instance.LoadCapableInstantly("plate_full") as Pasta;
        if (TryGetCapacity(out SortingCapacity sc)) { sc.ReceiveMovable(pastas, transform.position + (Vector3)plate_position); }
        pastas.LoadHover();

        AnimPlayer.Play("close");
        while (AnimPlayer.IsPlaying("close")) { yield return null; }

        print_coroutine = null;
    }


    public override void LoadCapablesAccordingly(Capable capable)
    {
        if (capable is not Item item) { return; }
        if (TryGetCapacity(out SortingCapacity sc)) { sc.ReceiveMovable(item, plate_position); }
    }
}