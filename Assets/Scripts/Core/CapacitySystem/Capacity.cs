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

    [Header("Capacity data")]
    public CapacityData data;
    public bool Loaded { get { return data != null; } }

    [Header("Logs")]
    public bool log = false;


    // LOAD / UNLAOD
    public virtual void LoadData(CapacityData data)
    {
        this.data = data;
        this.name = data.id;
    }
    public virtual void UnloadData()
    {
        this.data = null;
        this._capable = null;
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
            local_position = this.transform.localPosition,

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


    // USE
    // ? do we need all Capacities to have a Use method ?
    public virtual void Use(Capable capable)
    {
        // we play the animation
        capable.AnimPlayer.Play(name);
    }
}