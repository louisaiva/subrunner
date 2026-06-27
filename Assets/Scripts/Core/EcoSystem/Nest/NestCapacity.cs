using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NestCapacity : Capacity
{

    [Header("Static Data")]
    [SerializeField] private string species;
    [SerializeField] private int max_entity_stored = 1;
    [SerializeField] private NestStoreMode store_mode;
    [SerializeField] private NestSpawnMode spawn_mode;
    public NestData ndata => (NestData)data;
    private Collider2D trigger;



    ///
    //
    /// SPAWNING LOGIC
    //
    ///

    // ! readme please
    // for now we do the stuff in the update of the NestCapacity
    // but this is only available when the capacity is loaded, which is a problem
    // later we will want to have the spawning logic directly inside NestData
    private void Update()
    {
        if (!Loaded) { return; }
        if (ndata.entities_to_spawn.Count == 0) { return; }
        if (!TryGetSiblingCapacity(out SpawnCapacity spawner)) { return; }
        if (spawner.IsSpawning) { return; }
        string entity_id = ndata.ExtractSpawnableEntity();
        spawner.Spawn(entity_id);
    }



    ///
    //
    /// TRIGGERS
    //
    ///

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"(NestCapacity) {other.gameObject.name} just entered trigger of nest {ID} !!".AddColor(Color.blueViolet));

        if (ndata.spawn_mode != NestSpawnMode.Trigger) { return; }
        
        // we spawn them all !
        ndata.SpawnThemAll();
    }







    ///
    //
    /// DATA MANAGEMENT
    //
    ///

    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        base.LoadData(data, capable_data);

        if (data is not NestData ndata) { return; }
        if (ndata.trigger == null) { return; }
        this.trigger = ColliderBank.Instance.LoadCollider(ndata.trigger, this.transform);
    }
    public override void UnloadData()
    {
        // unload custom data here
        if (trigger != null)
        {
            ColliderBank.Instance.UnloadCollider(trigger.gameObject);
            trigger = null;
        }

        base.UnloadData();
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        NestData static_data = new NestData(base.GetStaticData())
        {
            species = this.species,
            capacity = this.max_entity_stored,
            store_mode = this.store_mode,
            spawn_mode = this.spawn_mode
        };
        
        Collider2D collider = GetComponentInChildren<Collider2D>(includeInactive: true);
        if (collider != null) { static_data.trigger = (CircleData) ColliderBank.GetColliderData(collider); }
        else { static_data.trigger = null; }

        return static_data;
    }
}
[Serializable] public class NestData : CapacityData
{
    public string species;
    public int capacity; // capacity of the burrow in count of max entity it can store at the same time
    public NestStoreMode store_mode;
    public int received_entities_since_last_reset;
    public NestSpawnMode spawn_mode;
    public CircleData trigger;
    public List<string> stored_entities = new List<string>();

    // CONSTRUCTOR
    public NestData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        NestData new_data = new NestData(base.Duplicate() as CapacityData)
        {
            species = this.species,
            capacity = this.capacity,
            store_mode = this.store_mode,
            received_entities_since_last_reset = this.received_entities_since_last_reset,
            spawn_mode = this.spawn_mode,
            stored_entities = new List<string>(this.stored_entities)
        };
        new_data.trigger = (trigger == null) ? null : (CircleData) this.trigger.Duplicate();
        return new_data;
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        // add details to the string here
        details += $"  - species : {species}\n";
        details += $"  - capacity : {capacity}\n";
        details += $"  - store_mode : {store_mode}\n";
        details += $"  - received_entities_since_last_reset : {received_entities_since_last_reset}\n";
        details += $"  - spawn_mode : {spawn_mode}\n";
        details += $"  - stored_entities : {stored_entities.Count}\n";
        foreach (string id in stored_entities)
        {
            details += $"    - {id}\n";
        }
        if (trigger == null) { details += $"  - trigger : null\n"; }
        else { details += $"  - trigger : {trigger.GetDetails()}\n"; }
        return base.GetDetails() + details;
    }






    ///
    //
    /// MAIN ENTRY POINTS
    //
    ///

    // public properties helper
    [RuntimeOnly] public int EntityCount => stored_entities.Count;
    public bool CanReceiveEntity()
    {
        if (store_mode == NestStoreMode.Endless) { return capacity > EntityCount; }
        if (store_mode == NestStoreMode.Once)
        {
            return capacity > received_entities_since_last_reset;
        }
        return false; // unknown store mode
    }

    // RECEIVE ENTITY
    public void ReceiveEntity(string id, bool manually = false)
    {
        if (!CanReceiveEntity())
        {
            Debug.LogError($"(NestData - {this.id}) Received entity '{id}' but the burrow is already full !!");
            return;
        }
        stored_entities.Add(id);
        
        // this happens when an entity come back by itself to hide inside the nest ! in this case we want to return early
        // because : 1. we don't want to spawn entity directly, and 2 we don't want to increase received_entities since it was
        // a manual entity receiving :)
        if (manually) { return; }

        received_entities_since_last_reset++;

        // here we check the spawn mode to see if we need to register to the spawning thing
        if (spawn_mode == NestSpawnMode.Direct) { entities_to_spawn.Add(id); }
    }

    // SPAWN ENTITIES
    [RuntimeOnly] public List<string> entities_to_spawn = new List<string>();
    public void SpawnRandomEntity()
    {
        if (EntityCount == 0) { return; }

        // add the entity to pending spawning entities
        string random_id = stored_entities[UnityEngine.Random.Range(0, stored_entities.Count)];
        entities_to_spawn.Add(random_id);

        // the entity is now registered to spawning list, which will (FOR NOW) be spawned
        // from the NestCapacity. in [mid-term] we will want this to happen in a NestData.Update() method
        // so we can handle both unloaded & loaded Nests, but for now we stay easy
    }
    public void SpawnThemAll()
    {
        entities_to_spawn = new List<string>(stored_entities);
    }
    public string ExtractSpawnableEntity()
    {
        if (entities_to_spawn.Count == 0) { return null; }
        string id = entities_to_spawn[0];
        entities_to_spawn.RemoveAt(0);
        stored_entities.Remove(id);
        return id;
    }









}

public enum NestSpawnMode
{
    Direct,
    Trigger,
    Interaction, // a stored entity is spawned when the Spawner is interacted by someone
    Manual // means entities are choosing when then want out
}

public enum NestStoreMode
{
    Endless,
    Once
}