using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapableBank : MonoBehaviour
{

    // AWAKE & SINGLETON LOGIC
    public static CapableBank Instance { get; private set; }
    public void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // initialize the pool of rooms
        pooled_capables = new Hashtable();
        pooled_anim_layers = new Stack<AnimLayer>();
    }

    // CAPABLE LOADING

    [Header("Loaded capables")]
    [SerializeField] protected List<Capable> loaded_capables;
    [SerializeField] protected Transform capables_parent;
    
    [Header("Prefabs")]
    [SerializeField] protected GameObject capable_prefab; // with no kind at all : when instantiating we need to add component to it
    [SerializeField] protected GameObject feet_prefab;
    [SerializeField] protected GameObject body_prefab;

    [Header("Sleeping capables")]
    [SerializeField] protected Hashtable/* <string, Stack<Capable>> */ pooled_capables;


    // ANIM PLAYER POOLING
    [Header("AnimPlayer pooling")]
    [SerializeField] protected GameObject anim_layer_prefab;
    [SerializeField] protected Stack<AnimLayer> pooled_anim_layers;


    [Header("Logs")]
    public bool log_types = false;
    public bool log_anim_layers = false;

    // LOAD CAPABLES
    public Capable Load(CapableData data)
    {
        // we first try to extract a capable of the right kind from the pool
        Capable capable = extractFromPool(data.kind);

        // we successfully extracted a capable from the pooled ones !
        if (capable != null)
        {
            // then we load the anim data inside the capable
            load_anim_data(capable.anim_player, data.anim_data);
            
            // we load its data
            capable.LoadData(data);
            capable.gameObject.SetActive(true);
            loaded_capables.Add(capable);
            return capable;
        }

        // if we have no pooled capable we need to instantiate one
        GameObject go = Instantiate(capable_prefab, capables_parent);

        // we need to add the kind of the capable to the object
        // if (log_types) { Debug.Log($"(CapableBank) Instantiated prefab, now adding component : {data.kind}"); }
        Type kind = Type.GetType(data.kind);
        if (kind == null)
        {
            Debug.LogError($"(CapableBank) Type not found for kind: {data.kind}");
            return null;
        }

        // then we add some few things we need, related to the capable kind
        capable = add_components_based_on_kind(go, kind);

        // then we load the anim data inside the capable
        load_anim_data(capable.anim_player, data.anim_data);

        // then we can load the data
        capable.LoadData(data);
        loaded_capables.Add(capable);
        return capable;
    }
    private Capable add_components_based_on_kind(GameObject go, Type kind)
    {
        // ! TODO this is temporary because Movable & Being will become MoveCapacity & HealthCapacity
        // movable
        if (is_kind(kind, typeof(Movable)))
        {
            Rigidbody2D rb = go.gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;

            // add a feet gameobject to it
            GameObject feet = Instantiate(feet_prefab, go.transform);
            feet.name = "feet";
        }

        // being
        if (is_kind(kind, typeof(Being)))
        {
            // add a body collider to it
            GameObject body = Instantiate(body_prefab, go.transform);
            body.name = "body";
        }

        // then we add the component corresponding to the capable kind
        Capable capable = go.gameObject.AddComponent(kind) as Capable;
        return capable;
    }
    private bool is_kind(Type capable_kind, Type ref_kind)
    {
        bool is_same_or_subclass = capable_kind == ref_kind || capable_kind.IsSubclassOf(ref_kind);
        return is_same_or_subclass;
    }


    // ANIM PLAYER
    private void load_anim_data(AnimPlayer player, AnimData anim_data)
    {
        // we load the main anim data in the player
        player.LoadPlayerData(anim_data);

        // we get the layers parent
        Transform layer_parent = player.layers_parent;

        // check that we do have some layers / layer_parent
        if (layer_parent == null || anim_data.layers == null) { return; }
        if (log_anim_layers) { Debug.Log($"(CapableBank - Load) Loading anim data for {anim_data.skin}, loading {anim_data.layers.Count} anim layers"); }

        // we go through all the layers inside anim_data and we load a layer for each
        for (int i = 0; i < anim_data.layers.Count; i++)
        {
            // we extract an anim layer from pooled ones
            AnimLayer anim_layer = extractAnimLayerFromPool(layer_parent);

            // then we load the data in the anim layer
            AnimLayerData layer_data = anim_data.layers[i];
            anim_layer.LoadData(layer_data);
            anim_layer.AssignLeader(player);
        }
    }


    // UNLOAD CAPABLES
    public Capable Unload(CapableData data)
    {
        // get capable
        Capable capable = GetLoadedCapable(data);
        if (capable == null) { return null; }
        Unload(capable);
        return capable;
    }
    public void Unload(Capable capable)
    {

        // unload anim layers
        List<AnimLayer> anim_layers = capable.anim_player.GetAnimLayers();
        if (log_anim_layers) { Debug.Log($"(CapableBank) Unloading capable {capable.data.id}, unloading {anim_layers.Count} anim layers"); }
        // for (int i = 0; i < anim_layers.Count; i++)
        while (anim_layers.Count > 0)
        {
            AnimLayer anim_layer = anim_layers[0];
            anim_layer.UnassignLeader();
            pooled_anim_layers.Push(anim_layer);
            anim_layers.RemoveAt(0);
        }

        // unload the capable's data and put it back in the pool
        string kind = capable.data.kind;
        capable.UnloadData();
        // pooled_capables.Push(capable);
        insertInPool(capable, kind);

        // remove the capable from the loaded capables list
        loaded_capables.Remove(capable);

        // disable the gameObject
        capable.gameObject.SetActive(false);
    }

    // low level pool management
    private Capable extractFromPool(string kind)
    {
        // we look for the capable with the given kind in the pool of pooled capables
        if (pooled_capables.ContainsKey(kind) && pooled_capables[kind] is Stack<Capable> stack && stack.Count > 0)
        {
            return stack.Pop();
        }
        return null;
    }
    private void insertInPool(Capable capable, string kind)
    {
        if (!pooled_capables.ContainsKey(kind))
        {
            pooled_capables[kind] = new Stack<Capable>();
        }
        if (pooled_capables[kind] is Stack<Capable> stack)
        {
            stack.Push(capable);
        }
    }
    private AnimLayer extractAnimLayerFromPool(Transform layer_parent)
    {
        // we first try to extract an anim layer from the pool
        AnimLayer anim_layer = null;
        if (pooled_anim_layers != null && pooled_anim_layers.Count > 0)
        {
            anim_layer = pooled_anim_layers.Pop();
            anim_layer.gameObject.SetActive(true);
            anim_layer.transform.SetParent(layer_parent);
        }
        else
        {
            // if we have no pooled anim layer we need to instantiate one
            anim_layer = Instantiate(anim_layer_prefab, layer_parent).GetComponent<AnimLayer>();
        }
        return anim_layer;
    }

    // CAPABLE GETTING
    public Capable GetLoadedCapable(string id)
    {
        // we look for the capable with the given id in the pool of loaded capables
        for (int i = 0; i < loaded_capables.Count; i++)
        {
            Capable capable = loaded_capables[i];
            if (capable.data != null && capable.data.id == id)
            {
                return capable;
            }
        }
        return null;
    }
    public Capable GetLoadedCapable(CapableData data)
    {
        return GetLoadedCapable(data.id);
    }
}
