using UnityEngine;
using UnityEngine.InputSystem;

public interface Interactable
{

    public string ID { get; }
    public string name { get; }
    public InteractCapacity Interactor { get; } // there is only ONE because it's the one that is Controlled // todo : maybe make this from HoverBasedInteractCapacity
    public InteractType InteractionType { get; }
    public void OnInteract(Capable interactor);
}
public interface EndlessInteractable : Interactable
{
    public void OnEndlessInteract(Capable interactor);
}

public interface Openable
{
    public bool is_moving { get; set; }
    public bool is_open { get; set; }
}

public interface Onnable
{
    public bool IsMoving { get; set; }
    public bool IsOn { get; set; }
}

public interface Holdable : Onnable
{
    public bool IsHolding { get; set; }
}

public interface TurnableIntoSomething
{
    public DropParameters DropParameters { get; }
}
public interface TurnableIntoItem : TurnableIntoSomething
{
    public ItemData ItemDataInfo { get; }
}

public interface Chestable : Interactable
{
    public Inventory Inventory { get; }

    // for capable that acts pretty much like a chest, which means
    // they can receive ingame near item drop, and also they show an
    // UI_Pool when interacted. basically it is Chest + Crafter for now.
    // this is mainly used to get the interacting inventory in Inventory.GetInteractingInventory()
    public void ExitHover();
    public string ChestType { get; }
}

public interface Sittable : Interactable
{
    public Vector2 WorldSittingPosition { get; }
    public Vector2 WorldStandingPosition { get; }
}