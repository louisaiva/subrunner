using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Stocks the path of prefabs of the capacities
/// can instance them and give them to any capable
/// </summary>

public class CapacityBank : MonoBehaviour
{
    [Header("Prefab path")]
    public string capacities_prefabs_path = "capacities";

    [Header("Debug")]
    public bool debug = false;


    // GETTERS
    public GameObject GetCapacityInstance(string capacity_name)
    {
        // check if there is a prefab with this name in the prefab path
        GameObject capacity = Resources.Load<GameObject>(capacities_prefabs_path+capacity_name);
        if (capacity == null)
        {
            // if not, we return an empty capacity
            capacity = Resources.Load<GameObject>(capacities_prefabs_path + "empty_capacity");
            GameObject capacity_instance = Instantiate(capacity);
            capacity_instance.name = capacity_name;

            // we log the error
            if (debug) { Debug.LogWarning("(CapacityBank) No capacity prefab found so we returned empty capacity instead of " + capacities_prefabs_path + "/" + capacity_name); }
            return capacity_instance;
        }

        if (debug)
        {
            string s = "(CapacityBank) Instanciating " + capacity_name + " capacity prefab !!";
            Debug.Log(s);
        }

        // instance the prefab
        return Instantiate(capacity);
    }
}