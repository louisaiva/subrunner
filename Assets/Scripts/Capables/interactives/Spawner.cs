using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : Capable, EndlessInteractable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }

    public InteractType InteractionType => InteractType.Spawner;

    [Header("Endless interaction")]
    public bool authorize_interact_endlessly = true;



    [Header("Spawner Parameters")]
    public List<GameObject> spawnables = new List<GameObject>();
    public List<int> probabilities = new List<int>();
    private SpawnCapacity spawner;


    // ON INTERACT
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we get the spawner
        if (spawner == null) { spawner = GetCapacity<SpawnCapacity>(); }

        // we get a random module from the bank
        GameObject entity = spawner.entity_prefab;
        if (spawnables.Count > 0) { entity = ChooseRandomEntity(); }

        // we spawn the entity
        entity = Instantiate(entity, Vector3.zero, Quaternion.identity);
        spawner.Spawn(entity);
        if (debug) { Debug.Log("(Spawner) " + name + " spawned entity " + entity.name); }
    }
    public void OnEndlessInteract(Capable interactor) { if (authorize_interact_endlessly) { OnInteract(interactor); } }


    // choose random entity
    private GameObject ChooseRandomEntity()
    {
        if (spawnables.Count == 1) { return spawnables[0]; }

        int totalProbability = probabilities.Sum();

        int randomValue = Random.Range(0, totalProbability);

        for (int i = 0; i < spawnables.Count; i++)
        {
            if (randomValue < probabilities[i])
            {
                return spawnables[i];
            }
            randomValue -= probabilities[i];
        }

        return spawnables[0]; // Fallback in case of an error
    }
}