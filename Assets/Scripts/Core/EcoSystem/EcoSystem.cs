using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class EcoEngine : MonoBehaviour
{
    public List<Species> species;
    private Dictionary<Species, List<NestData>> nests = new Dictionary<Species, List<NestData>>();
    private Dictionary<Species, ManualSpeciesTicker> manual_tickers = new Dictionary<Species, ManualSpeciesTicker>();

    [Header("Logs")]
    public bool log_awake_details = false;
    public bool log_species_gain_entity = false;
    public bool log_species_loss_entity = false;
    public bool log_new_nest = false;
    public bool log_tick_nests_details = false;
    public bool hide_log_no_species_found = false;


    ///
    //
    /// MAIN ENTRY POINTS
    //
    ///


    // load species
    public void LoadSpecies(bool log)
    {
        initialize_alive_population_species();

        // register callbacks
        CapableEngine.LazyInstance.OnCapableSpawned += handle_capable_spawned;
        CapableEngine.LazyInstance.OnCapableDespawned += handle_capable_despawned;
        CapacityEngine.LazyInstance.OnCapacitySpawned += handle_capacity_spawned;
        CapacityEngine.LazyInstance.OnCapacityDespawned += handle_capacity_despawned;
        if (log) { Debug.Log($"(EcoEngine) Registered to CapableEngine & CapacityEngine callbacks"); }

        // now gather the manual tickers for species that require precise spawning handling
        ManualSpeciesTicker[] tickers = GetComponents<ManualSpeciesTicker>();
        string debug = "";
        foreach (ManualSpeciesTicker ticker in tickers)
        {
            Species spec = get_species_from_id(ticker.species);
            if (spec == null) { continue; }
            manual_tickers[spec] = ticker;
            if (log) { debug += " - " + spec.name + " --> " + ticker.GetType().Name + "\n"; }
        }
        if (log) { Debug.Log($"(EcoEngine) Gathered {manual_tickers.Keys.Count} manual tickers : \n" + debug); }


        if (!log) { return; }
        debug = "";
        foreach (Species spec in species)
        {
            debug += spec.GetDetails() + "\n";
        }
        Debug.Log($"(EcoEngine) Loaded {species.Count} species :\n{debug}");
    }
    private void initialize_alive_population_species()
    {
        List<CapableData> entities = new List<CapableData>();
        foreach (Species spec in species)
        {
            entities = CapableEngine.LazyInstance.GetWorldCapableMatchingTemplate(spec.template);
            spec.alive_population = entities.Count;

            if (!spec.wait_for_corpse_despawn) { continue; }

            // todo here we need to gather also corpse matching the template
        }
    }

    // gather NestDatas
    public void GatherNests(bool log)
    {
        nests.Clear();
        List<NestData> world_nests = CapacityEngine.Instance.GetCapacitiesDataOfKind<NestData>();
        foreach (NestData nest in world_nests)
        {
            if (string.IsNullOrEmpty(nest.species)) { continue; }
            Species spec = get_species_from_id(nest.species);
            if (spec == null) { continue; }

            if (!nests.ContainsKey(spec)) { nests[spec] = new List<NestData>(); }
            nests[spec].Add(nest);
            spec.waiting_population += nest.EntityCount;
        }



        finalize_species_loading();



        if (!log) { return; }
        string debug = "";
        int total_nests = 0;
        foreach (var kvp in nests)
        {
            debug += " - " + kvp.Key.name + $" : {kvp.Value.Count} nests\n";
            foreach (NestData nest in kvp.Value)
            {
                debug += "    - " + nest.id + "\n";
                total_nests++;
            }
            debug += "\n";
        }
        Debug.Log($"(EcoEngine) Gathered {total_nests} nests for {nests.Keys.Count} species :\n" + debug);
    }
    private void finalize_species_loading()
    {
        // here we have gathered :
        // - all existing entity data of template
        // - all number of entities living in nests
        // we have enough data to count current global population of the species
        foreach (Species spec in species)
        {
            spec.population = spec.alive_population + spec.waiting_population;
        }

        if (!log_awake_details) { return; }
        string debug = "";
        foreach (Species spec in species)
        {
            debug += spec.GetDetails() + "\n";
        }
        Debug.Log($"(EcoEngine) Finally loaded {species.Count} species :\n{debug}");
    }

    // clear cache
    public void ClearCache(bool log)
    {
        // unregister callbacks
        CapableEngine.LazyInstance.OnCapableSpawned -= handle_capable_spawned;
        CapableEngine.LazyInstance.OnCapableDespawned -= handle_capable_despawned;
        CapacityEngine.LazyInstance.OnCapacitySpawned -= handle_capacity_spawned;
        CapacityEngine.LazyInstance.OnCapacityDespawned -= handle_capacity_despawned;
        if (log) { Debug.Log($"(EcoEngine) Unregistered from CapableEngine & CapacityEngine callbacks"); }

        nests.Clear();
        if (log) { Debug.Log($"(EcoEngine) Cleared nests"); }
    }


    /// CALLBACKS
    private void handle_capable_spawned(CapableData cdata)
    {
        if (cdata is not IAData ia) { return; }
        Species spec = get_species_from_id(ia.id);
        if (spec == null) { return; }

        // we register the new entity !
        spec.alive_population+=1;
        // we update the current waiting pop
        if (!nests.ContainsKey(spec)) { spec.waiting_population = 0; }
        else { spec.UpdateWaitingPopulation(nests[spec]); }
        spec.UpdatePopulation();

        if (log_species_gain_entity) { Debug.Log($"(EcoEngine) Species {spec.name} welcomes '{cdata.id}' !!!!!! population is now : " + spec.PopDetails()); }
    }
    private void handle_capable_despawned(CapableData cdata)
    {
        Species spec = null;
        if (cdata is IAData ia)
        {
            spec = get_species_from_id(ia.id);
            if (spec == null) { return; }
            if (spec.wait_for_corpse_despawn) { return; } // we don't care about this ia death cause we want to wait the corpse to be despawned for re giving entities :D
        }
        else if (cdata is CorpseData corpse)
        {
            spec = get_species_from_id(corpse.species_template);
            if (spec == null) { return; }
            if (!spec.wait_for_corpse_despawn) { return; } // we don't care for this corpse since we are not corpse based, so the call was already taken when the real capable was despawned
        }
        if (spec == null) { return; }

        // now an entity of this species was dead :///
        spec.alive_population -= 1;
        if (log_species_loss_entity) { Debug.Log($"(EcoEngine) Species {spec.name} lost a member :/// population is now : " + spec.PopDetails()); }
    }
    private void handle_capacity_spawned(CapacityData cdata)
    {
        if (cdata is not NestData nest) { return; }
        if (string.IsNullOrEmpty(nest.species)) { return; }
        Species spec = get_species_from_id(nest.species);
        if (spec == null) { return; }
        if (!nests.ContainsKey(spec)) { nests[spec] = new List<NestData>(); }
        if (nests[spec].Contains(nest)) { return; }
        nests[spec].Add(nest);
        spec.population += nest.EntityCount;
        spec.waiting_population += nest.EntityCount;
        if (log_new_nest) { Debug.Log($"(EcoEngine) Species {spec.name} just earned a new nest '{nest.id}' !!"); }
    }
    private void handle_capacity_despawned(CapacityData cdata)
    {
        if (cdata is not NestData nest) { return; }
        if (string.IsNullOrEmpty(nest.species)) { return; }
        Species spec = get_species_from_id(nest.species);
        if (spec == null) { return; }
        if (!nests.ContainsKey(spec)) { return; }
        if (!nests[spec].Contains(nest)) { return; }
        nests[spec].Remove(nest);
        spec.population -= nest.EntityCount;
        spec.waiting_population -= nest.EntityCount;
        if (log_new_nest) { Debug.Log($"(EcoEngine) Species {spec.name} just lost a nest '{nest.id}' :/"); }
    }


    ///
    //
    /// UPDATE
    //
    ///

    [Header("Tick duration")]
    [SerializeField] private float tick_delay = 0.1f;
    private float counter = 0f;
    private void Update()
    {
        counter -= Time.deltaTime;
        if (counter > 0f) { return; }
        counter = tick_delay;
        Tick();
    }
    private void Tick()
    {
        // we go through all species to check if we have some entities to give
        foreach (Species spec in species)
        {
            if (manual_tickers.TryGetValue(spec, out ManualSpeciesTicker ticker) && nests.TryGetValue(spec, out List<NestData> spec_nests))
            {
                ticker.Tick(spec, spec_nests);
                if (!ticker.handle_entity_receiving_each_frame) { continue; } // we don't want to handle entity receiving, we just skip it
            }
            
            if (spec.dead_population == 0) { continue; } // no entity to give
            make_a_nest_receive_an_entity(spec);
        }
    }


    // low level update methods
    private List<NestData> tmp_nests = new List<NestData>();
    private List<NestData> tmp_nests2 = new List<NestData>();
    private void make_a_nest_receive_an_entity(Species spec)
    {
        // we can give one entity to a random nest !!!
        if (!nests.TryGetValue(spec, out tmp_nests) || tmp_nests.Count == 0)
        {
            if (log_tick_nests_details) { Debug.LogWarning($"(EcoEngine) Species {spec.name} has no nest."); }
            return;
        }
        if (log_tick_nests_details) { Debug.LogWarning($"(EcoEngine) Species {spec.name} has {tmp_nests.Count} nest before filtering those who can receive entity."); }
        tmp_nests2 = tmp_nests.Where(n => n.CanReceiveEntity()).ToList();
        if (tmp_nests2.Count == 0)
        {
            if (log_tick_nests_details) { Debug.LogWarning($"(EcoEngine) Species {spec.name} : No nest can receive entity !!!"); }
            return;
        }

        NestData nest = tmp_nests2[UnityEngine.Random.Range(0, tmp_nests2.Count)];
        nest.ReceiveEntity();
        spec.waiting_population++;
    }




    ///
    //
    /// GETTERS
    //
    ///


    private Species get_species_from_id(string id)
    {
        string template = id.GetPrefix();
        if (string.IsNullOrEmpty(template)) { return null; }
        for (int i=0; i<species.Count; i++)
        {
            if (species[i].template != template) { continue; }
            return species[i];
        }
        if (!hide_log_no_species_found) { Debug.LogWarning($"(EcoEngine) no species found for capable template : '{template}'"); }
        return null;
    }
    public string GetTemplateOf(string species)
    {
        Species spec = get_species_from_id(species);
        if (spec == null) { return null; }
        return spec.template;
    }

}

[Serializable] public class Species
{
    public string name;
    public string template;
    public int population;

    [RuntimeOnly] public int alive_population;
    [RuntimeOnly] public int waiting_population;
    [RuntimeOnly] public float WaitingOrAlivePercentage => (alive_population + waiting_population) / population;
    [RuntimeOnly] public int dead_population => population - alive_population - waiting_population;

    public bool wait_for_corpse_despawn = false;

    public string GetDetails()
    {
        string log = "";
        log += " - " + name + " :\n";
        log += "   - template : " + template + "\n";
        log += "   - population : " + population + $"   --->  ({alive_population} alive, {waiting_population} waiting, {dead_population} dead)\n";
        log += "   - wait_for_corpse_despawn : " + wait_for_corpse_despawn + "\n";
        return log;
    }
    public string PopDetails()
    {
        return $"{alive_population} or {waiting_population} / {population}".AddColor(Color.Lerp(a: Color.cyan, b: Color.magenta, WaitingOrAlivePercentage));
    }

    public void UpdateWaitingPopulation(List<NestData> nestDatas)
    {
        int waiting = 0;
        foreach (NestData nest in nestDatas) { waiting += nest.EntityCount; }
        this.waiting_population = waiting;
    }
    public void UpdatePopulation()
    {
        // if waiting + alive is bigger than population we update it as well
        if (waiting_population + alive_population > population)
        {
            population = waiting_population + alive_population;
        }
    }
}