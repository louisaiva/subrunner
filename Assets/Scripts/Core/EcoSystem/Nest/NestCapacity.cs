using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NestCapacity : Capacity
{

    [Tooltip("These fields are NOT real fields. These are only visible debug fields but the working ones exist only in the data NestData. Conclusion : DON T TRUST THESE FIELDS THEY ARE FULL OF LIES BERK")]
    [Header("Static Data")]
    [SerializeField] private string species;
    [SerializeField] private int max_entity_stored = 1;
    [SerializeField] private NestStoreMode store_mode;
    [SerializeField] private NestSpawnMode spawn_mode;
    
    [Header("Runtime Only")]
    [SerializeField] private Collider2D trigger;
    public NestData ndata => (NestData)data;



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
        if (ndata.spawn_mode == NestSpawnMode.Trigger) { update_potential_triggers(); }

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

    private Capable potential_trigger = null;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ndata.spawn_mode != NestSpawnMode.Trigger) { return; }

        // check if the trigger has a capable
        Capable capable = other.GetComponentInParent<Capable>();
        if (capable == null || !capable.Loaded) { return; }
        if (capable.data.room != Capable.data.room)
        {
            // here we are in different rooms, we don't want to trigger it right away
            // BUT we register the capable as potential trigger so asas it gets to the same room we
            // spawn them all :D
            potential_trigger = capable;
            return;
        }

        // we spawn them all !
        ndata.SpawnThemAll();
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we remove the potential trigger if this is the potential trigger
        if (ndata.spawn_mode != NestSpawnMode.Trigger) { return; }
        if (potential_trigger == null) { return; }
        Capable capable = other.GetComponentInParent<Capable>();
        if (capable == null || capable != potential_trigger) { return; }
        potential_trigger = null;
    }
    private void update_potential_triggers()
    {
        if (potential_trigger == null) { return; }
        if (!potential_trigger.Loaded) { potential_trigger = null; return; }
        if (potential_trigger.data.room != Capable.data.room) { return; }

        // here we are finally in the same room !
        ndata.SpawnThemAll();
        potential_trigger = null;
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

        // load debug static data
        this.species = ndata.species;
        this.max_entity_stored = ndata.capacity;
        this.store_mode = ndata.store_mode;
        this.spawn_mode = ndata.spawn_mode;

        // load the trigger
        if (ndata.trigger == null) { return; }
        if (ndata.trigger.radius == 0f) { return; }
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
            spawn_mode = this.spawn_mode,
            spawned_once = false,
            stored_entities = this.max_entity_stored // we full up the nest
        };
        
        Collider2D collider = GetComponentInChildren<Collider2D>(includeInactive: true);
        if (collider != null)
        {
            static_data.trigger = (CircleData) ColliderBank.GetColliderData(collider);
            if (static_data.trigger.radius == 0f) { static_data.trigger = null;}
        }
        else { static_data.trigger = null; }

        return static_data;
    }
}
[Serializable] public class NestData : CapacityData
{
    public string species;
    public int capacity; // capacity of the burrow in count of max entity it can store at the same time
    public NestStoreMode store_mode;
    // public int received_entities_since_last_reset;
    public bool spawned_once;
    public NestSpawnMode spawn_mode;
    public CircleData trigger;
    // public List<string> stored_entities = new List<string>();
    public int stored_entities;

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
            spawned_once = false,
            spawn_mode = this.spawn_mode,
            stored_entities = stored_entities
        };
        new_data.trigger = (trigger == null) ? null : (CircleData) this.trigger.Duplicate();
        if (store_mode == NestStoreMode.Once) { stored_entities = spawned_once ? 0 : capacity; }

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
        details += $"  - spawned_once : {spawned_once}\n";
        details += $"  - spawn_mode : {spawn_mode}\n";
        details += $"  - stored_entities : {stored_entities}\n";
        if (trigger == null) { details += $"  - trigger : null\n"; }
        else { details += $"  - trigger : {trigger.GetDetails()}\n"; }
        return base.GetDetails() + details;
    }
    public string StoreDetails(Color a, Color b)
    {
        return $"{stored_entities} / {capacity}".AddColor(Color.Lerp(a,b, FullPercentage));
    }





    ///
    //
    /// MAIN ENTRY POINTS
    //
    ///

    // public properties helper
    [RuntimeOnly] public int EntityCount => stored_entities;
    [RuntimeOnly] private float FullPercentage => EntityCount / capacity;
    public bool CanReceiveEntity()
    {
        if (store_mode == NestStoreMode.Endless) { return capacity > EntityCount; }
        if (store_mode == NestStoreMode.Once)
        {
            if (capacity <= EntityCount) { return false; }
            return !spawned_once;
        }
        return false; // unknown store mode
    }
    public bool CanHostEntity(string id)
    {
        if (capacity <= EntityCount) { return false; } // no space left anymore

        // check species type
        string spec_template = CapableEngine.EcoEngine.GetTemplateOf(species);
        if (string.IsNullOrEmpty(spec_template)) { return false; }
        if (id.GetPrefix() != spec_template) { return false; }
        return true;
    }

    // RECEIVE ENTITY
    public void ReceiveEntity(bool manually = false)
    {
        if (!CanReceiveEntity())
        {
            Debug.LogError($"(NestData - {this.id}) Received an entity but the nest is already full !!");
            return;
        }
        stored_entities++;

        if (manually) { return; }

        // here we check the spawn mode to see if we need to register to the spawning thing
        if (spawn_mode == NestSpawnMode.Direct) { entities_to_spawn.Add(Template); }
    }

    // SPAWN ENTITIES
    [RuntimeOnly] public List<string> entities_to_spawn = new List<string>();
    public void SpawnThemAll()
    {
        if (EntityCount == 0) { return; }
        entities_to_spawn = new List<string>();
        for (int i=0; i<stored_entities; i++) { entities_to_spawn.Add(Template); }
        spawned_once = true;
    }
    public void SpawnOne()
    {
        if (EntityCount == 0) { return; }
        entities_to_spawn.Add(Template);
        spawned_once = true;
    }
    public string ExtractSpawnableEntity()
    {
        if (entities_to_spawn.Count == 0) { return null; }
        string id = entities_to_spawn[0];
        stored_entities--;
        entities_to_spawn.RemoveAt(0);
        return id;
    }
    public string Template => CapableEngine.EcoEngine.GetTemplateOf(species);









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