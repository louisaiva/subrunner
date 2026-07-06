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
        string random_module_template = module_references[UnityEngine.Random.Range(0, module_references.Count)];
        // Module module = ItemBank.Instance.CreateModule(random_module_template); // we don't do this anymore cause everything is template based now
        spawner.Spawn(random_module_template);
        if (log) { Debug.Log("(ModuleSpawner) " + name + " spawned module " + random_module_template); }
    }

    public void OnEndlessInteract(Capable interactor) { if (authorize_interact_endlessly) { OnInteract(interactor); } }
}