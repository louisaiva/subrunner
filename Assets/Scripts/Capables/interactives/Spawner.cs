using System.Collections.Generic;
using UnityEngine;

public class Spawner : Capable, Interactable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public bool AuthorizeEndlessInteraction => authorize_interact_endlessly;
    [Header("Endless interaction")]
    public bool authorize_interact_endlessly = true;



    [Header("Spawner Parameters")]
    public List<GameObject> spawnables = new List<GameObject>();
    private SpawnCapacity spawner;


    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we get the spawner
        if (spawner == null) { spawner = GetCapacity<SpawnCapacity>(); }

        // we get a random module from the bank
        GameObject entity = spawner.entity_prefab;
        if (spawnables.Count > 0) { entity = spawnables[Random.Range(0, spawnables.Count)]; }

        // we spawn the entity
        entity = Instantiate(entity, Vector3.zero, Quaternion.identity);
        spawner.Spawn(entity);
        if (debug) { Debug.Log("(Spawner) " + name + " spawned entity " + entity.name); }
    }
}