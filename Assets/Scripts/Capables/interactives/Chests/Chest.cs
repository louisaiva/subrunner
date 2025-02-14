using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : Capable, Interactable
{
    // inputs actions
    /*public PlayerInputActions input_actions
    {
        get => GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;
    }*/

    // ON INTERACT
    public void OnInteract()
    {
        // if (!input_actions.perso.enabled) { return; }

        if (Can("open_close"))
        {
            Do("open_close");
        }
    }

}