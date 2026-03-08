using System;
using UnityEngine;

public class Capacity : MonoBehaviour
{

    // NEW CAPACITY SYSTEM

    [Header("Capacity data")]
    public CapacityData data;
    public bool Loaded { get { return data != null; } }


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






    // OLD AREA


    public virtual bool Able
    {
        get
        {
            // if there is no cooldown, we return true
            if (cooldown == 0) { return true; }

            // if the cooldown is done, we return true
            return cooldown_timer <= 0;
        }
    }
    private Capable _capable = null;
    public Capable capable
    {
        get
        {
            if (_capable == null) { _capable = transform.parent.GetComponent<Capable>(); }
            return _capable;
        }
    }



    // todo some capacity don't have a cooldown (walk, run, grab, hover), so make this an interface
    [Header("Cooldown")]
    [SerializeField] protected float cooldown = 0;
    protected float cooldown_timer;

    [Header("Logs")]
    public bool log = false;


    protected virtual void Update()
    {
        // if the cooldown is not set, we return
        if (cooldown == 0) { return; }

        // if the cooldown is running, we update it
        if (cooldown_timer > 0)
        {
            cooldown_timer -= Time.deltaTime;
        }
    }

    protected void startCooldown(float? custom_cooldown = null)
    {
        // we set the cooldown timer if there is one
        if (custom_cooldown != null)
        {
            cooldown = (float) custom_cooldown;
        }
        
        if (cooldown > 0)
        {
            cooldown_timer = cooldown;
        }
    }

    public virtual void Use(Capable capable)
    {
        // we play the animation
        capable.anim_player.Play(name);
    }
}