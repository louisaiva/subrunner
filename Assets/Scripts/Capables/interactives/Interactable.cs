using UnityEngine;
using UnityEngine.InputSystem;

public interface Interactable
{
    // PlayerInputActions input_actions { get; }
    public InteractCapacity Interactor { get; set; }
    void OnInteract() {}
}

public interface Openable
{
    public bool is_moving { get; set; }
    public bool is_open { get; set; }
}