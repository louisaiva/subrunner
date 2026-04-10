using System;
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

        // initialize the pools of capables & anim layers
        pooled_capables = new Dictionary<string,Stack<Capable>>();
        pooled_anim_layers = new Stack<AnimLayer>();
    }

    // CAPABLE LOADING
    [Header("Loaded capables")]
    [SerializeField] protected List<Capable> loaded_capables;
    // [SerializeField] protected Transform capables_parent;
    
    [Header("Prefabs")]
    [SerializeField] protected GameObject capable_prefab; // with no kind at all : when instantiating we need to add component to it
    [SerializeField] protected GameObject feet_prefab;

    [Header("Sleeping capables")]
    [SerializeField] protected Dictionary<string,Stack<Capable>> pooled_capables;
    protected HashSet<Capable> capables_in_bank = new HashSet<Capable>(); // stores all capables from instantiation (loaded & pooled ones)


    // ANIM PLAYER POOLING
    [Header("AnimPlayer pooling")]
    [SerializeField] protected GameObject anim_layer_prefab;
    [SerializeField] protected Stack<AnimLayer> pooled_anim_layers;


    [Header("Logs")]
    public bool log_types = false;
    public bool log_anim_player = false;
    public bool log_anim_layers = false;
    public bool log_inventory_build = false;

    // ACTIONS
    public Action<CapableData> OnCapableLoading = delegate { }; // fired BEFORE the data is loaded
    public Action<CapableData> OnCapableUnloaded = delegate { }; // fired AFTER the data is unloaded


    // LOAD CAPABLES
    public Capable Load(CapableData data)
    {
        // we first try to extract a capable of the right kind from the pool
        Capable capable = extractFromPool(data.kind);

        // we successfully extracted a capable from the pooled ones !
        if (capable != null)
        {
            // then we load the anim data inside the capable
            load_anim_data(capable.AnimPlayer, data.anim_data);

            // and its body
            load_feet_data(capable, data.feet_data);

            // set the good parent for the capable based on its kind
            capable.transform.SetParent(get_parent_based_on_kind(data.kind));

            // fire the callback
            OnCapableLoading?.Invoke(data);

            // we load its data
            capable.LoadData(data);
            capable.gameObject.SetActive(true);
            loaded_capables.Add(capable);
            return capable;
        }

        // if we have no pooled capable we need to instantiate one
        GameObject go = Instantiate(capable_prefab);

        // we need to add the kind of the capable to the object
        // if (log_types) { Debug.Log($"(CapableBank) Instantiated prefab, now adding component : {data.kind}"); }
        Type kind = Type.GetType(data.kind);
        if (kind == null)
        {
            Debug.LogError($"(CapableBank) Type not found for kind: {data.kind}");
            return null;
        }

        // set the good parent for the capable based on its kind
        go.transform.SetParent(get_parent_based_on_kind(data.kind));

        // then we add some few things we need, related to the capable kind
        capable = add_components_based_on_kind(go, kind);

        // we add the capable to the bank hashset
        capables_in_bank.Add(capable);

        // then we load the anim data inside the capable
        load_anim_data(capable.AnimPlayer, data.anim_data);

        // and its body
        load_feet_data(capable, data.feet_data);

        // and its inventory
        build_inventory_item_pools(capable.Inventory, data.inventory);

        // fire the callback
        OnCapableLoading?.Invoke(data);

        // then we can load the data
        capable.LoadData(data);
        loaded_capables.Add(capable);
        return capable;
    }
    private Capable add_components_based_on_kind(GameObject go, Type kind)
    {
        // movable
        // ! TODO this is temporary because Movable & Being will become MoveCapacity
        if (GameManager.IsKind(kind, typeof(Movable)))
        {
            Rigidbody2D rb = go.gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        // we add the component corresponding to the capable kind
        Capable capable = go.gameObject.AddComponent(kind) as Capable;





        return capable;
    }
    private Transform get_parent_based_on_kind(string kind)
    {
        if (GameManager.IsKind(kind, "Item")) { return World.Instance.ItemsParent; }
        else if (GameManager.IsKind(kind, "Movable")) { return World.Instance.MovablesParent; }
        else { return World.Instance.CapablesParent; }
    }

    // INVENTORY ITEM POOL BUILDING
    private void build_inventory_item_pools(Inventory inv, InventoryData data)
    {
        // we check if we have no inv or no data, no need to pull up those item pools
        if (log_inventory_build) { Debug.Log($"(CapableBank - build_inventory_item_pools) building item pools for { (inv == null ? "(inventory is null)" : inv.Capable.name ) } with { (data == null ? "(data is null)" : data.item_pools_data.Count + " items pools")}"); }
        if (inv == null) { return; }
        if (data == null || data.item_pools_data.Count == 0) { return; }

        // we get the number of pools we need to add
        for (int i=0; i < data.item_pools_data.Count; i++)
        {
            // we add an item pool component to the inv gameobject
            inv.gameObject.AddComponent<ItemPool>();
        }
    }

    // ANIM PLAYER & COLLIDERS
    private void load_anim_data(AnimPlayer player, AnimData anim_data)
    {
        // we get the layers parent
        Transform layer_parent = player.transform;

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

        // we load the main anim data in the player
        if (log_anim_player) { player.log = true; }
        player.LoadPlayerData(anim_data); // will play the last anim by default
    }
    private void load_feet_data(Capable capable, FeetData feet_data)
    {
        // checks if body data is null it means we have no colliders, we do nothing then
        if (feet_data == null) { return; }
        Transform feet = capable.Feet;

        // load box colliders
        for (int i = 0; i < feet_data.box_colliders.Count; i++)
        {
            ColliderBank.Instance.LoadCollider(feet_data.box_colliders[i], feet);
        }

        // load circle colliders
        for (int i = 0; i < feet_data.circle_colliders.Count; i++)
        {
            ColliderBank.Instance.LoadCollider(feet_data.circle_colliders[i], feet);
        }
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

    // UNLOAD CAPABLES
    public Capable Unload(CapableData data)
    {
        // get capable
        Capable capable = GetLoadedCapable(data);
        if (capable == null) { return null; }
        unload_capable(capable);

        // fire the callback
        OnCapableUnloaded?.Invoke(data);

        return capable;
    }
    private void unload_capable(Capable capable)
    {

        // unload anim layers
        List<AnimLayer> anim_layers = capable.AnimPlayer.GetAnimLayers();
        if (log_anim_layers) { Debug.Log($"(CapableBank) Unloading capable {capable.data.id}, unloading {anim_layers.Count} anim layers"); }
        // for (int i = 0; i < anim_layers.Count; i++)
        while (anim_layers.Count > 0)
        {
            AnimLayer anim_layer = anim_layers[0];
            anim_layer.UnassignLeader();
            pooled_anim_layers.Push(anim_layer);
            anim_layers.RemoveAt(0);
        }

        // unload feet colliders
        Transform feet = capable.Feet;
        Collider2D[] colliders = feet.GetComponentsInChildren<Collider2D>(includeInactive: true);
        for (int i = 0; i < colliders.Length; i++)
        {
            ColliderBank.Instance.UnloadCollider(colliders[i].gameObject);
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


    // GETTERS
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
    public bool HasCapable(Capable capable)
    {
        // we check EVERYWHERE if we have this capable, loaded or in sleeping pools
        // -> means we check capables_in_bank because it contains all capables instantiated ever !
        return capables_in_bank.Contains(capable);
    }
}
