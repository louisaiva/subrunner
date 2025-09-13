using UnityEngine;

public class ModuleSpawner : Capable, Interactable
{
    private SpawnCapacity spawner;

    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we get the spawner
        if (spawner == null) { spawner = GetCapacity<SpawnCapacity>(); }

        // we get a random module from the bank
        Module module = ItemBank.Instance.CreateRandomModule();
        spawner.Spawn(module.gameObject);
        if (debug) { Debug.Log("(ModuleSpawner) " + name + " spawned module " + module.Reference); }
    }
}