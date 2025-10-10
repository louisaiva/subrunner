using UnityEngine;
using UnityEngine.InputSystem;

public interface Interactable
{
    
    public InteractCapacity Interactor { get; } // there is only ONE because it's the one that is Controlled
    public void OnInteract(Capable interactor);
    public bool AuthorizeEndlessInteraction { get; }
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