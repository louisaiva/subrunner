using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : Capable, Interactable, Openable
{
    // OPENABLE
    public bool is_open { get; set; }
    public bool is_moving { get; set; }

    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public void OnInteract()
    {
        if (Can("open")) { Do("open"); }
        else if (Can("close")) { Do("close"); }
    }

}