using UnityEngine;

public class Spawner : Capable, Interactable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we react to the interaction
        if (Can("spawn")) { Do("spawn"); }
        else { Debug.Log("Spawner has been interacted !"); }
    }

}