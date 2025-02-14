using UnityEngine;
using UnityEngine.InputSystem;

public interface Interactable
{
    // PlayerInputActions input_actions { get; }
    void OnInteract() {}
}

public interface Openable
{
    public bool is_moving { get; set; }
    public bool is_open { get; set; }
}