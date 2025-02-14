using UnityEngine;

public class Spawner : Capable, Interactable
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
        if (Can("spawn")) { Do("spawn"); }
        else { Debug.Log("Spawner has been interacted !"); }
    }

}