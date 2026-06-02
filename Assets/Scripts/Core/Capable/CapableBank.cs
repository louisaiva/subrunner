using System;
using System.Collections.Generic;
using UnityEngine;

public class CapableBank : MonoBehaviour
{

    // AWAKE & SINGLETON LOGIC
    public static CapableBank Instance { get; private set; }
    public static CapableBank LazyInstance
    {
        get
        {
            if (Instance != null) { return Instance; }
            Instance = FindFirstObjectByType<CapableBank>();
            if (Instance == null) { Debug.LogError($"No instance of CapableBank found in the scene."); }
            return Instance;
        }
    }
    public void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // initialize the pools of capables & anim layers
        pooled_capables = new Dictionary<string,Stack<Capable>>();
    }


    // CAPABLE LOADING
    [Header("Loaded capables")]
    [SerializeField] protected List<Capable> loaded_capables;
    
    [Header("Prefabs")]
    [SerializeField] protected GameObject capable_prefab; // with no kind at all : when instantiating we need to add component to it
    [SerializeField] protected GameObject feet_prefab;
    [SerializeField] protected GameObject item_pool_prefab;

    [Header("Sleeping capables")]
    [SerializeField] protected Dictionary<string,Stack<Capable>> pooled_capables;
    protected HashSet<Capable> capables_in_bank = new HashSet<Capable>(); // stores all capables from instantiation (loaded & pooled ones)


    // SUB SYSTEMS
    private AnimLayerBank _anim_layer_bank;
    public AnimLayerBank LayerBank
    {
        get
        {
            if (_anim_layer_bank == null) { _anim_layer_bank = GetComponentInChildren<AnimLayerBank>(includeInactive: true); }
            return _anim_layer_bank;
        }
    }

    private MaterialBank _material_bank;
    public MaterialBank MaterialBank
    {
        get
        {
            if (_material_bank == null)
            {
                _material_bank = GetComponentInChildren<MaterialBank>(includeInactive: true);
                if (_material_bank == null) { Debug.LogError("CapableEngine: MaterialBank component not found in children."); }
            }
            return _material_bank;
        }
    }



    [Header("Logs")]
    public bool log_types = false;
    public bool log_inventory_build = false;

    // ACTIONS
    public Action<CapableData> OnCapableLoading = delegate { }; // fired BEFORE the data is loaded
    public Action<Capable> OnCapableLoaded = delegate { }; // fired AFTER the data is loaded
    public Action<Capable> OnCapableUnloading = delegate { }; // fired BEFORE the capable is unloaded
    public Action<CapableData> OnCapableUnloaded = delegate { }; // fired AFTER the data is unloaded


    // LOAD CAPABLES
    public Capable Load(CapableData data)
    {
        // we first try to extract a capable of the right kind from the pool
        Capable capable = extractFromPool(data.kind);

        if (capable != null) // we successfully extracted a capable from the pooled ones !
        {
            // then we load the anim data inside the capable
            // LayerBank.LoadAnimData(capable.AnimPlayer, data.anim_data);

            // and its feet
            bool force_is_trigger2 = data is ItemData idata2 && idata2.is_grabbed;
            load_feet_data(capable, data.feet_data, force_is_trigger2);

            // set the good parent for the capable based on its kind
            capable.transform.SetParent(get_parent_based_on_kind(data.kind));

            // fire the loading callback
            OnCapableLoading?.Invoke(data);

            // we load its data
            capable.LoadData(data);
            capable.gameObject.SetActive(true);
            loaded_capables.Add(capable);

            // fire the loaded callback
            OnCapableLoaded?.Invoke(capable);
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
        // LayerBank.LoadAnimData(capable.AnimPlayer, data.anim_data);

        // and its feet
        bool force_is_trigger = data is ItemData idata && idata.is_grabbed;
        load_feet_data(capable, data.feet_data, force_is_trigger);

        // and its inventory
        build_inventory_item_pools(capable.Inventory, data.inventory);

        // fire the callback
        OnCapableLoading?.Invoke(data);

        // then we can load the data
        capable.LoadData(data);
        loaded_capables.Add(capable);
        OnCapableLoaded?.Invoke(capable);
        return capable;
    }
    private Capable add_components_based_on_kind(GameObject go, Type kind)
    {
        // movable
        // ! TODO this is temporary because Movable & Being will become MoveCapacity & HealthCapacity
        if (GameManager.IsKind(kind, typeof(Movable)))
        {
            Rigidbody2D rb = go.gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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
        if (log_inventory_build) { Debug.Log($"(CapableBank - build_inventory_item_pools) building item pools for { (inv == null ? "(inventory is null)" : inv.Capable.ID ) } with { (data == null ? "(data is null)" : data.item_pools_data.Count + " items pools")}"); }
        if (inv == null) { return; }
        if (data == null || data.item_pools_data.Count == 0) { return; }
        if (data.item_pools_data.Count == 1)
        {
            if (log_inventory_build) { Debug.Log($"(CapableBank - build_inventory_item_pools) only one item pool, adding one ItemPool component to the inventory gameobject"); }
            inv.gameObject.AddComponent<ItemPool>();
            return;
        }

        // if we have more than 1 item pool, we build multiple children gameObjects otherwise
        // it will have weird issues when calling Capable.GetStaticData()

        // we get the number of pools we need to add
        for (int i=0; i < data.item_pools_data.Count; i++)
        {
            GameObject go = Instantiate(item_pool_prefab, inv.transform);
            go.name = data.item_pools_data[i].pool_id;
            if (log_inventory_build) { Debug.Log($"(CapableBank - build_inventory_item_pools) built item pool '{go.name}' (n°{i}) for inventory {inv.Capable.ID}"); }
        }
    }

    // ANIM PLAYER & COLLIDERS
    private void load_feet_data(Capable capable, FeetData feet_data, bool force_is_trigger = false)
    {
        // checks if body data is null it means we have no colliders, we do nothing then
        if (feet_data == null) { return; }
        Transform feet = capable.Feet;

        // load box colliders
        for (int i = 0; i < feet_data.box_colliders.Count; i++)
        {
            if (force_is_trigger) { feet_data.box_colliders[i].is_trigger = true; }
            ColliderBank.Instance.LoadCollider(feet_data.box_colliders[i], feet);
        }

        // load circle colliders
        for (int i = 0; i < feet_data.circle_colliders.Count; i++)
        {
            if (force_is_trigger) { feet_data.circle_colliders[i].is_trigger = true; }
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

    // UNLOAD CAPABLES
    public Capable Unload(CapableData data)
    {
        // get capable
        Capable capable = GetLoadedCapable(data);
        if (capable == null) { return null; }

        // fire the callback
        OnCapableUnloading?.Invoke(capable);

        // unload the capable
        unload_capable(capable);

        // fire the callback
        OnCapableUnloaded?.Invoke(data);

        return capable;
    }
    private void unload_capable(Capable capable)
    {
        // LayerBank.UnloadAnimData(capable.AnimPlayer);

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
        insertInPool(capable, kind);

        // remove the capable from the loaded capables list
        loaded_capables.Remove(capable);

        // disable the gameObject
        capable.gameObject.SetActive(false);
    }


    // DESTROY CAPABLES
    public void DestroyAllCapablesInstantly(bool log)
    {
        destroy_all_loaded_capables(log);
        destroy_all_pooled_capables(log);
        capables_in_bank.Clear();
        if (log) { Debug.Log($"(CapableBank) All capables were successfully destroyed"); }
    }
    private void destroy_all_loaded_capables(bool log)
    {
        if (log) { Debug.Log($"(CapableBank) Removing null loaded capables"); }
        loaded_capables.RemoveAll(c => c == null);
        if (log) { Debug.Log($"(CapableBank) Destroying all LOADED capables, count : {loaded_capables.Count}"); }
        for (int i = 0; i < loaded_capables.Count; i++)
        {
            Destroy(loaded_capables[i].gameObject);
        }
        loaded_capables.Clear();
        if (log) { Debug.Log($"(CapableBank) All loaded capables are now destroyed and loaded_capables is cleared"); }
    }
    private void destroy_all_pooled_capables(bool log)
    {
        if (log) { Debug.Log($"(CapableBank) Destroying all POOLED capables"); }
        // we clear the pool of pooled capables to be sure to destroy all capable gameobjects in the bank
        foreach (KeyValuePair<string, Stack<Capable>> entry in pooled_capables)
        {
            Stack<Capable> stack = entry.Value;
            if (log) { Debug.Log($"(CapableBank) Destroying pooled capables in stack : {entry.Key}"); }
            while (stack.Count > 0)
            {
                Capable capable = stack.Pop();
                if (capable == null) { continue; }
                Destroy(capable.gameObject);
            }
        }
        pooled_capables.Clear();
        if (log) { Debug.Log($"(CapableBank) All pooled capables are now destroyed and pooled_capables is cleared"); }
    }
    public void ClearSubSystemsCache(bool log)
    {
        // we clear the sub systems caches to be sure to destroy all capable gameobjects in the bank
        if (log) { Debug.Log($"(CapableBank) clearing anim layer bank"); }
        LayerBank.ClearCache(log);

        if (log) { Debug.Log($"(CapableBank) clearing collider bank"); }
        ColliderBank.Instance.ClearCache(log);
    }


    // GETTERS
    public bool TryGetLoadedCapable(string id, out Capable capable)
    {
        capable = GetLoadedCapable(id);
        return capable != null;
    }
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
    public List<Capable> GetAllLoadedCapables() { return loaded_capables; }
}
