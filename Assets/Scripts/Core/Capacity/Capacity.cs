using UnityEngine;

public class Capacity : MonoBehaviour
{

    public virtual bool Able { get { return true; } }
    private Capable _capable = null;
    public Capable Capable
    {
        get
        {
            if (_capable == null) { _capable = transform.parent.GetComponent<Capable>(); }
            return _capable;
        }
    }
    public AnimPlayer AnimPlayer
    {
        get
        {
            if (Capable == null) { return null; }
            return Capable.AnimPlayer;
        }
    }

    [Header("Capacity data")]
    public CapacityData data;
    public bool Loaded { get { return data is not null; } }
    public string ID
    {
        get
        {
            if (!Loaded) { return "unloaded_capacity"; }
            if (string.IsNullOrEmpty(data.id)) { return name; }
            return data.id;
        }
    }
    public string OwnerID
    {
        get
        {
            if (!Loaded) { return "unloaded_capacity"; }
            return data.owner_id;
        }
    }





    [Header("Logs")]
    public bool log = false;


    // LOAD / UNLAOD
    public virtual void LoadData(CapacityData data, CapableData capable_data)
    {
        this.data = data;
        this.name = data.id;

        // set the local pos if different than zero
        if (data.local_position != Vector2.zero) { transform.localPosition = data.local_position; }

        // we set the layer & tag
        gameObject.layer = data.layer;
        if (!string.IsNullOrEmpty(data.tag)) { gameObject.tag = data.tag; }
    }
    public virtual void UnloadData()
    {
        // save dynamic data
        SaveDynamicData();

        this.data = null;
        this._capable = null;

        // stop all coroutines
        StopAllCoroutines();
    }

    // SAVE DYNAMIC DATA
    public virtual void SaveDynamicData()
    {
        // this method is made for saving data that changes during the game (dynamic data).
        // it means it should run when the game is running and the data is loaded.
        // This method is called in UnloadData, but it can be called anywhere else when we want to save the dynamic data.
    }




    // GET CURRENT STATIC DATA
    /// <summary>
    /// this method is made for saving data from a prefab THAT IS NOT LOADED.
    /// it means it should run ONLY inside the editor and it may run when 
    /// the game is not started. This means we should get the data through the hierarchy only
    /// since all the lists will be null or empty
    /// </summary>
    /// <returns>CapacityData the data that describes this capable</returns>
    public virtual CapacityData GetStaticData()
    {
        CapacityData static_data = new CapacityData
        {
            // set base data things
            id = get_static_id(),
            owner_id = get_static_owner_id(),
            local_position = this.transform.localPosition,

            // set the layer & tag
            layer = gameObject.layer,
            tag = gameObject.tag,
            
            // we set the kind
            kind = GetType().Name
        };

        return static_data;
    }
    protected string get_static_id()
    {
        string id = this.name;
        if (this.data == null) { return id; }
        if (string.IsNullOrEmpty(this.data.id)) { return id; }
        return this.data.id;
    }
    protected string get_static_owner_id()
    {
        // needs to check in the parent' capable bcz we are static
        // so the game is not running -> Capable = null
        if (transform.parent == null) { return "no_parent"; }
        Capable parent_capable = transform.parent.GetComponent<Capable>();
        if (parent_capable == null) { return "no_capable"; }
        if (parent_capable.data == null) { return "no_capable_data"; }
        if (string.IsNullOrEmpty(parent_capable.data.id)) { return "no_capable_id"; }
        return parent_capable.data.id;
    }

    // USE
    // ? do we need all Capacities to have a Use method ?
    public virtual void Use(Capable capable)
    {
        // we play the animation
        capable.AnimPlayer.Play(name);
    }


    // GETTERS
    public T GetSiblingCapacity<T>() where T : Capacity
    {
        if (Capable == null) { return null; }
        if (!Capable.TryGetCapacity(out T sibling_capacity)) { return null; }
        return sibling_capacity;
    }
    public bool TryGetSiblingCapacity<T>(out T sibling_capacity) where T : Capacity
    {
        sibling_capacity = null;
        if (Capable == null) { return false; }
        return Capable.TryGetCapacity<T>(out sibling_capacity);
    }
}