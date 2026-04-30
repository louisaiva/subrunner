using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Stocks the path of prefabs of the capacities
/// can instance them and give them to any capable
/// </summary>

public class CapacityBank : MonoBehaviour
{

    // AWAKE & SINGLETON LOGIC
    public static CapacityBank Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // load capacities prefabs
        load_capacities_prefabs();

        // initialize pools
        pooled_capacities = new Dictionary<string,Stack<Capacity>>();
    }



    // GETTERS
    [Obsolete("Use Load + data instead")]
    public GameObject InstantiateCapacity(string capacity_name)
    {
        // check if there is a prefab with this name in the prefab path
        GameObject capacity = Resources.Load<GameObject>(capacities_prefabs_path + capacity_name);
        if (capacity == null)
        {
            // if not, we return an empty capacity
            capacity = Resources.Load<GameObject>(capacities_prefabs_path + "empty_capacity");
            GameObject capacity_instance = Instantiate(capacity);
            capacity_instance.name = capacity_name;

            // we log the error
            if (log) { Debug.LogWarning("(CapacityBank) No capacity prefab found so we returned empty capacity instead of " + capacities_prefabs_path + "/" + capacity_name); }
            return capacity_instance;
        }

        if (log)
        {
            string s = "(CapacityBank) Instanciating " + capacity_name + " capacity prefab !!";
            Debug.Log(s);
        }

        // instance the prefab
        return Instantiate(capacity);
    }


    [Header("Prefabs")]
    // todo for now we can only have 1 single folder for storing the capacities prefabs which is not the best
    // since we have a LOT of capacities (and will have more)
    // , so implement something like this : https://stackoverflow.com/questions/57094126/searching-through-all-subfolders-when-using-resources-load-in-unity
    public string capacities_prefabs_path = "prefabs/capacities/";
    private Dictionary<Type, GameObject> capacities_prefabs = new Dictionary<Type, GameObject>();


    [Header("Logs")]
    public bool log = false;
    public bool log_prefabs_loading = false;

    // PREFABS LOADING
    private void load_capacities_prefabs()
    {
        capacities_prefabs = new Dictionary<Type, GameObject>();
        string log_prefabs = "";

        // we load all the prefabs in the prefab path and stock them in the dictionary
        GameObject[] prefabs = Resources.LoadAll<GameObject>(capacities_prefabs_path);
        foreach (GameObject prefab in prefabs)
        {
            Capacity capacity = prefab.GetComponent<Capacity>();
            if (capacity == null) { continue; }

            // we add the go to the prefab dic so we don't have to ever call Resources.Load<>()
            Type capacity_type = capacity.GetType();
            capacities_prefabs.Add(capacity_type, prefab);

            // and add its name to the log
            log_prefabs += "  - " + capacity_type + "\n";
        }

        if (log_prefabs_loading) { Debug.Log($"(CapacityBank) CAPACITIES in the bank - Total {capacities_prefabs.Count} capacities\n\n" +log_prefabs); }
    }



    [Header("Loaded capacities")]
    [SerializeField] protected List<Capacity> loaded_capacities;

    [Header("Sleeping capacities")]
    [SerializeField] protected Transform sleeping_capacities_parent;
    [SerializeField] protected Dictionary<string,Stack<Capacity>> pooled_capacities;

    // low level pool management
    private Capacity extractFromPool(string kind)
    {
        // we look for the given kind of capacity within the pool of pooled capacities
        if (pooled_capacities.ContainsKey(kind) && pooled_capacities[kind] is Stack<Capacity> stack && stack.Count > 0)
        {
            return stack.Pop();
        }
        return null;
    }
    private void insertInPool(Capacity capa, string kind)
    {
        if (!pooled_capacities.ContainsKey(kind))
        {
            pooled_capacities[kind] = new Stack<Capacity>();
        }
        if (pooled_capacities[kind] is Stack<Capacity> stack)
        {
            stack.Push(capa);
        }
    }


    // LOAD CAPACITIES
    public Capacity Load(CapacityData data)
    {
        // we first try to extract of the right kind from the pool
        Capacity capacity = extractFromPool(data.kind);

        // we successfully extracted from the pooled ones !
        if (capacity != null)
        {
            // we load its data
            capacity.LoadData(data);
            capacity.gameObject.SetActive(true);
            loaded_capacities.Add(capacity);
            return capacity;
        }

        // if we have no pooled capacity we need to instantiate one

        // we check if we have a corresponding prefab
        Type capa_type = Type.GetType(data.kind);
        if (!capacities_prefabs.ContainsKey(capa_type))
        {
            Debug.LogError("(CapacityBank) No prefab found for capacity kind: " + data.kind);
            return null;
        }
        GameObject go = Instantiate(capacities_prefabs[capa_type]);
        capacity = go.GetComponent<Capacity>();

        // then we can load the data
        capacity.LoadData(data);
        loaded_capacities.Add(capacity);
        return capacity;
    }

    // UNLOAD CAPACITIES
    public Capacity Unload(CapacityData data)
    {
        // get capacity
        Capacity capacity = GetLoadedCapacity(data);
        if (capacity == null) { return null; }
        Unload(capacity);
        return capacity;
    }
    public void Unload(Capacity capacity)
    {
        // unload the capacity's data and put it back in the pool
        string kind = capacity.data.kind;
        capacity.UnloadData();
        insertInPool(capacity, kind);

        // remove the capacity from the loaded capacitys list
        loaded_capacities.Remove(capacity);

        // disable the gameObject
        capacity.gameObject.SetActive(false);

        // and set the capacity parent to the sleeping one
        capacity.transform.SetParent(sleeping_capacities_parent);
    }



    // DESTROY CAPACITIES
    public void DestroyAllCapacitiesInstantly()
    {
        destroy_all_loaded_capacities();
        destroy_all_pooled_capacities();
    }
    private void destroy_all_loaded_capacities()
    {
        for (int i = 0; i < loaded_capacities.Count; i++)
        {
            Destroy(loaded_capacities[i].gameObject);
        }
        loaded_capacities.Clear();
    }
    private void destroy_all_pooled_capacities()
    {
        // we clear the pool of pooled capables to be sure to destroy all capable gameobjects in the bank
        foreach (KeyValuePair<string, Stack<Capacity>> entry in pooled_capacities)
        {
            Stack<Capacity> stack = entry.Value;
            while (stack.Count > 0)
            {
                Capacity capacity = stack.Pop();
                Destroy(capacity.gameObject);
            }
        }
        pooled_capacities.Clear();
    }




    // CAPA GETTING
    public Capacity GetLoadedCapacity(string id)
    {
        // we look for the capacity with the given id in the pool of loaded capacitys
        for (int i = 0; i < loaded_capacities.Count; i++)
        {
            Capacity capacity = loaded_capacities[i];
            if (capacity.data != null && capacity.data.id == id)
            {
                return capacity;
            }
        }
        return null;
    }
    public Capacity GetLoadedCapacity(CapacityData data)
    {
        return GetLoadedCapacity(data.id);
    }
}