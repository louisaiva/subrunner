using UnityEngine;

public class Spawner : Capable, Interactable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public void OnInteract()
    {
        // if (!input_actions.perso.enabled) { return; }
        if (Can("spawn")) { Do("spawn"); }
        else { Debug.Log("Spawner has been interacted !"); }
    }

}