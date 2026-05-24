using System.Collections.Generic;
using UnityEngine;

public class ModuleSpawner : Capable, EndlessInteractable
{
    private SpawnCapacity spawner;

    [Header("Modules Spawner")]
    public List<string> module_references = new List<string>();

    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public InteractType InteractionType { get { return InteractType.Spawner; } }

    [Header("Endless interaction")]
    public bool authorize_interact_endlessly = true;
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we get the spawner
        if (spawner == null) { spawner = GetCapacity<SpawnCapacity>(); }

        // we get a random module from the bank
        Module module = ItemBank.Instance.CreateRandomModule(module_references);
        spawner.Spawn(module.gameObject);
        if (log) { Debug.Log("(ModuleSpawner) " + name + " spawned module " + module.Reference); }
    }

    public void OnEndlessInteract(Capable interactor) { if (authorize_interact_endlessly) { OnInteract(interactor); } }
}