using UnityEngine;
using UnityEngine.InputSystem;

public interface Interactable
{

    public string name { get; }
    public InteractCapacity Interactor { get; } // there is only ONE because it's the one that is Controlled
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

public interface TurnableIntoItem
{
    public ItemData ItemDataInfo { get; }
    public DropParameters DropParameters { get; }
}

public interface Chestable : Interactable
{
    public Inventory Inventory { get; }

    // for capable that acts pretty much like a chest, which means
    // they show the UI_ChestPool
    public void ExitHover();
    public string ChestType { get; }
}

public interface Sittable : Interactable
{
    public Vector2 WorldSittingPosition { get; }
    public Vector2 WorldStandingPosition { get; }
}