using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Mother class of all the Capables in the game.
/// A Capable is a gameObject that can have Capacities.
/// Each Capacity is linked to an animation.
/// So every animated element in the game is a Capable.
/// </summary>
[RequireComponent(typeof(AnimPlayer))]
public class Capable : MonoBehaviour, Debuggable
{

    // NEW CAPACITY SYSTEM

    [Header("Capable data")]
    public CapableData data;
    public bool Loaded { get { return data != null; } }


    // LOAD / UNLOAD
    public virtual void LoadData(CapableData data)
    {
        if (CapableSystem.Instance.log_loading_extended) { Debug.Log($"(Capable - LoadData) Loading capable {data.id} \n\n{data.GetDetails()}"); }
        this.data = data;
        this.name = data.id;
        this.transform.position = data.position;

        // we set the orientation
        this.Orientation = data.orientation;

        // ! the colliders & anim data are loaded directly from CapableBank since we pool them

        // we load the inventory (and so the items)
        Inventory?.LoadInventoryData(data.inventory);

        // we add the effects
        for (int i = 0; i < data.effects.Count; i++)
        {
            AddEffect(data.effects[i], data.effects_ttl[i]);
        }

        // we load the capacities
        if (CapableSystem.Instance.log_loading_extended) { Debug.Log($"(Capable - LoadData) Calling CapacitySystem loading for capacities : {string.Join(" ", data.capacities_ids)}"); }
        this.capacities = CapacityEngine.Instance.LoadCapacities(data.capacities_ids, this);
        /* for (int i = 0; i < capacities.Count; i++)
        {
            Capacity capa = capacities[i];
            capa.transform.parent = transform;
            capa.transform.localPosition = capa.data.local_position;
        } */

    }
    public virtual void UnloadData()
    {
        // here we need to unload all the capacities that we hold
        // -> interacts with CapacityEngine

        if (CapableSystem.Instance.log_loading_extended) { Debug.Log($"(Capable - UnloadData) Calling CapacitySystem unloading for capacities : {string.Join(" ", data.capacities_ids)}"); }
        CapacityEngine.Instance.UnloadCapacities(data.capacities_ids, this);
        // this.capacities

        // we unload the inventory (and so the items)
        Inventory?.SaveAndUnloadInventoryData();


        // we save some data
        this.data.position = this.transform.position;

        this.data = null;
    }




    // GET CURRENT STATIC DATA
    /// <summary>
    /// this method is made for saving data from a prefab THAT IS NOT LOADED.
    /// it means it should run ONLY inside the editor and it may run when 
    /// the game is not started. This means we should get the data through the hierarchy only
    /// since all the lists will be null or empty
    /// </summary>
    /// <returns>CapableData the data that describes this capable</returns>
    public virtual ICapableData GetStaticData()
    {
        

        CapableData static_data = new CapableData
        {
            // set base data things
            id = get_static_id(),
            position = this.transform.position,

            // we set the kind
            kind = GetType().Name,

            // we set the anim data
            anim_data = anim_player.GetStaticAnimData(),

            // we set the body data
            body_data = get_static_body_data(),

            // we set the orientation
            orientation = this.orientation,

            // we set the inventory
            inventory = Inventory?.GetStaticInventoryData(),

            // we set the capacities
            capacities_ids = get_static_capacity_ids(),

            // we set the effects
            effects = new List<Effect>(effects),
            effects_ttl = new List<float>(effects_timetolive)
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
    protected List<string> get_static_capacity_ids()
    {
        List<string> capacities_ids = new List<string>();
        
        // we go through all children and check if we have capacities
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Capacity capa = child.GetComponent<Capacity>();
            if (capa == null) { continue; }
            if (capa.data.id == "") { capa.data.id = capa.name; }
            capacities_ids.Add(capa.data.id);
        }

        return capacities_ids;
    }
    protected BodyData get_static_body_data()
    {
        if (body == null) { return null; }

        BodyData body_data = new BodyData
        {
            box_colliders = new List<BoxData>(),
            circle_colliders = new List<CircleData>()
        };


        // we go through all colliders in the body and save their data
        for (int i = 0; i < body.childCount; i++)
        {
            Transform collider_transform = body.GetChild(i);
            Collider2D collider = collider_transform.GetComponent<Collider2D>();
            if (collider == null) { continue; }

            if (collider is BoxCollider2D box_collider)
            {
                BoxData box_data = get_static_box_data(box_collider);
                body_data.box_colliders.Add(box_data);
            }
            else if (collider is CircleCollider2D circle_collider)
            {
                CircleData circle_data = get_static_circle_data(circle_collider);
                body_data.circle_colliders.Add(circle_data);
            }
        }

        return body_data;
    }
    protected BoxData get_static_box_data(BoxCollider2D collider)
    {
        
        if (log_static_data) { Debug.Log($"(Capable - GetStaticData - {name}) BoxCollider2D found with offset {collider.offset} and size {collider.size} and is_trigger = {collider.isTrigger}"); }
        return new BoxData
        {
            // set base gameobject data
            local_position = collider.transform.localPosition,
            layerID = collider.gameObject.layer,

            // set base collider data
            offset = collider.offset,
            is_trigger = collider.isTrigger,
            size = collider.size,

            // set navmesh use
            used_for_pathfinding = is_used_for_pathfinding(collider)
        };
    }
    protected CircleData get_static_circle_data(CircleCollider2D collider)
    {
        if (log_static_data) { Debug.Log($"(Capable - GetStaticData - {name}) CircleCollider2D found with offset {collider.offset} and radius {collider.radius} and is_trigger = {collider.isTrigger}"); }
        return new CircleData
        {
            radius = collider.radius,
            local_position = collider.transform.localPosition,
            layerID = collider.gameObject.layer,
            offset = collider.offset,
            is_trigger = collider.isTrigger,
            used_for_pathfinding = is_used_for_pathfinding(collider)
        };
    }
    protected bool is_used_for_pathfinding(Collider2D collider)
    {
        NavMeshPlus.Components.NavMeshModifier modifier = collider.GetComponent<NavMeshPlus.Components.NavMeshModifier>();
        if (modifier != null && modifier.enabled) { return true; }
        return false;
    }











    // OLD AREA


    // un Capable est un gameObject qui possède des capacités
    // et donc des animations (les capacités peuvent être reliées à une animation)

    [Header("CAPABLE")]
    // the analog equivalent of the anim_player.orientation which is numerical
    [SerializeField] protected Vector2 inputs; // inputs can be at 0,0
    [SerializeField] protected Vector2 orientation = new Vector2(0,-1); // orientation can't be at 0,0 -> always normalized & remember last orientation
    public Vector2 Orientation
    {
        get { return orientation; }
        set
        {
            inputs = value;

            if (value != Vector2.zero)
            {
                orientation = value.normalized;
                anim_player.SetOrientation(orientation);
            }
        }
    }
    public void ClearInputs() { inputs = Vector2.zero; } // does the same than Orientation = Vector2.zero; but more optimized
    public string Skin => (anim_player == null) ? "none" : anim_player.Skin;

    [Header("Capacities")]
    [SerializeField] protected List<Capacity> capacities = new List<Capacity>();

    [Header("Effects")]
    [SerializeField] protected List<Effect> effects = new List<Effect>();
    [SerializeField] protected List<float> effects_timetolive = new List<float>();


    protected bool going_to_be_destroyed = false;

    // anim player
    private AnimPlayer _anim_player = null;
    public AnimPlayer anim_player { get
        {
            if (_anim_player == null) { _anim_player = GetComponent<AnimPlayer>(); }
            return _anim_player;
        } private set { _anim_player = value; } }

    // body
    private Transform _body = null;
    public Transform body { get
        {
            if (_body == null) { _body = transform.Find("body"); }
            return _body;
        } }


    // un capable peut aussi avoir un inventaire & un hover
    private Inventory _inventory = null;
    public Inventory Inventory
    {
        get
        {
            if (_inventory == null)
            {
                Transform inventory_transform = transform.Find("inventory");
                if (inventory_transform == null) { return null; }
                _inventory = inventory_transform.GetComponent<Inventory>();
            }
            return _inventory;
        }
    }
    private HoverCapacity _hover = null;
    public HoverCapacity Hover
    {
        get
        {
            if (_hover == null) { _hover = GetCapacity<HoverCapacity>(); }
            return _hover;
        }
    }



    // et des capacités electroniques
    public virtual ConnectCapacity Connector
    {
        // ? réellement logique que ça soit là ça ???
        // todo on peut pas le mettre dans Hacker/Vulnerable ou simplement utiliser Device ?
        // -> +1 pour Device
        get
        {
            // we check if we have a ConnectCapacity directly
            if (HasCapacity<ConnectCapacity>()) { return GetCapacity<ConnectCapacity>(); }

            // or a connectable item
            else if (Inventory != null)
            {
                // checks if one of our items is a device
                Device device = Inventory.GetDeviceItem();
                if (device != null) { return device.Connector; }
            }

            return null;
        }
    }


    [Header("Logs")]
    public bool log = false;
    public bool activate_all_capacities_logs_on_awake = false;
    public bool log_static_data = false;

    // START
    protected virtual void OnEnable()
    {
        // we get the anim player
        Orientation = orientation;
        if (!AppManager.Instance.IsQuitting) { DebugManager.Instance?.transform.GetComponentInChildren<EntitiesDebug>()?.AddEntity(this); }


        // we register all the capacities that are on this capable ONLY if we are not part of the BSOD pattern systems
        if (CapableSystem.Instance != null && CapableSystem.Instance.HasCapable(this) && CapacityEngine.Instance != null) { return; }
        
        capacities.Clear();
        foreach (Transform child in transform)
        {
            Capacity capa = child.GetComponent<Capacity>();
            if (!capa) { continue; }
            RegisterCapacity(capa);

            // we check if the log is true then we force debug to be true
            if (activate_all_capacities_logs_on_awake) { capa.log = true; }
        }
    }
    protected virtual void OnDisable()
    {
        // this.capacities.Clear();
        if (!AppManager.Instance.IsQuitting) { DebugManager.Instance?.transform.GetComponentInChildren<EntitiesDebug>()?.RemoveEntity(this); }
    }


    // CAPACITIES REGISTERING
    public void RegisterCapacity(Capacity capa)
    {
        if (capacities.Contains(capa)) { return; }

        capacities.Add(capa);

        if (log) { Debug.Log($"(Capable - {this.name}) Registered capacity {capa.name}"); }
    }
    public void UnregisterCapacity(Capacity capa)
    {
        if (!capacities.Contains(capa)) { return; }

        capacities.Remove(capa);

        if (log) { Debug.Log($"(Capable - {this.name}) Unregistered capacity {capa.name}"); }
    }


    // UPDATES
    protected virtual void Update()
    {
        // we update the effects
        updateEffects();
    }
    protected virtual void updateEffects()
    {
        int effects_count = effects.Count;
        int i = 0;
        while (i < effects_count)
        {
            if (effects_timetolive[i] == -888f) // -888f is the value for infinite time to live so we don't need to update it
            { i++; continue; }

            if (effects_timetolive[i] <= 0)
            {
                effects.RemoveAt(i);
                effects_timetolive.RemoveAt(i);
                effects_count--;
            }
            else
            {
                effects_timetolive[i] -= Time.deltaTime;
                i++;
            }
        }
    }

    // SETTERS
    public void OrientTowards(Vector3 target_position)
    {
        // we set the orientation
        Orientation = target_position - transform.position;
    }
    public void Orient(string direction)
    {
        // we set the orientation
        switch (direction)
        {
            case "U": Orientation = Vector2.up; break;
            case "D": Orientation = Vector2.down; break;
            case "L": Orientation = Vector2.left; break;
            case "R": Orientation = Vector2.right; break;
            case "UL": Orientation = (Vector2.up + Vector2.left).normalized; break;
            case "UR": Orientation = (Vector2.up + Vector2.right).normalized; break;
            case "DL": Orientation = (Vector2.down + Vector2.left).normalized; break;
            case "DR": Orientation = (Vector2.down + Vector2.right).normalized; break;
            default: Debug.LogWarning("Orientation " + direction + " not recognized"); break;
        }
    }

    // CAPACITIES
    [Obsolete("Use GetCapacity<T>().Use() instead")] public void Do(string name)
    {
        // get the capacity
        Capacity capacity = GetCapacity(name);

        capacity.Use(this);
    }
    [Obsolete("Use RegisterCapacity() instead")] public Capacity AddCapacity(string name)
    {
        // we check if the capacity is already in the list
        if (HasCapacity(name)) { return GetCapacity(name); }

        // get the capacity instance
        GameObject capa_instance = CapacityBank.Instance?.InstantiateCapacity(name);

        // we put it as a child of the capable & we rename it
        capa_instance.transform.parent = transform;
        capa_instance.name = name;
        capa_instance.transform.localPosition = Vector3.zero;

        // we put the capacity in the list
        Capacity capa = capa_instance.GetComponent<Capacity>();
        capacities.Add(capa);
        if (log) { Debug.Log("(Capable) " + this.name + " : capacity " + name + " added"); }

        return capa;
    }
    [Obsolete("Use UnregisterCapacity() instead")] public void RemoveCapacity(string name)
    {
        foreach (Capacity capa in capacities)
        {
            if (capa.name == name)
            {
                capacities.Remove(capa);
                Destroy(capa.gameObject);
                if (log) { Debug.Log("(Capable) " + this.name + " : capacity " + name + " removed"); }
                return;
            }
        }
    }


    // CAPACITIES GETTERS
    public virtual bool Can(string name)
    {
        foreach (Capacity capacity in capacities)
        {
            if (capacity.name == name)
            {
                return capacity.Able;
            }
        }
        return false;
    }
    public bool HasCapacity(string name)
    {
        foreach (Capacity capacity in capacities)
        {
            if (capacity.name == name)
            {
                return true;
            }
        }
        return false;
    }
    public bool HasCapacity<T>() where T : Capacity
    {
        return GetCapacity<T>() != null;
    }
    public Capacity GetCapacity(string name)
    {
        foreach (Capacity capacity in capacities)
        {
            if (capacity.name == name)
            {
                return capacity;
            }
        }
        return null;
    }
    public T GetCapacity<T>() where T : Capacity
    {
        foreach (Capacity capacity in capacities)
        {
            if (capacity is T)
            {
                return (T)capacity;
            }
        }
        return null;
    }
    public List<Capacity> GetCapacities()
    {
        return capacities;
    }

    // EFFECTS

    /// <summary>
    /// Add an effect to the capable with a time to live in seconds. If the effect is already present, it won't be added again.
    /// If the time to live is -888f, the effect will be infinite.
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="timetolive"></param>
    public virtual void AddEffect(Effect effect, float timetolive)
    {
        if (HasEffect(effect)) { return; }
        effects.Add(effect);
        effects_timetolive.Add(timetolive);
    }
    public virtual void RemoveEffect(Effect effect)
    {
        if (!HasEffect(effect)) { return; }

        effects_timetolive.RemoveAt(effects.IndexOf(effect));
        effects.Remove(effect);
    }
    public bool HasEffect(Effect effect)
    {
        foreach (Effect e in effects)
        {
            if (e == effect)
            {
                return true;
            }
        }
        return false;
    }


    // ITEMS MANAGEMENT
    public void DestroyAllItems()
    {
        if (Inventory == null || Inventory.Count == 0) { return; }

        // we drop all items on thr ground
        List<Item> items = Inventory.Items;
        for (int i = items.Count - 1; i >= 0; i--)
        {
            Item item = items[i];
            Inventory.Remove(item);
            Destroy(item.gameObject);
        }
    }
    public async Awaitable DropAllItems()
    {
        if (Inventory == null || Inventory.Count == 0) { return; }

        // we get the drop capacity
        DropCapacity dropper = GetCapacity<DropCapacity>();
        if (dropper == null)
        {
            // we add it if not present
            AddCapacity("drop");

            // we wait a frame
            await System.Threading.Tasks.Task.Yield();

            // we get the dropper
            dropper = GetCapacity<DropCapacity>();
        }
        dropper.random_direction = true;
        dropper.lock_magnitude = false;

        // we drop all items on thr ground
        List<Item> items = Inventory.Items;
        for (int i = items.Count - 1; i >= 0; i--)
        {
            // we drop the item
            dropper.Select(items[i]);
            dropper.Use(this);
        }
    }
    public async Awaitable DropItem(Item item)
    {
        if (Inventory == null || !Inventory.Items.Contains(item)) { return; }

        // we get the drop capacity
        DropCapacity dropper = GetCapacity<DropCapacity>();
        if (dropper == null)
        {
            // we add it if not present
            AddCapacity("drop");

            // we wait a frame
            await System.Threading.Tasks.Task.Yield();

            // we get the dropper
            dropper = GetCapacity<DropCapacity>();
        }

        // we drop the item
        dropper.Select(item);
        dropper.Use(this);
    }

    // DEBUG
    protected virtual void OnDestroy()
    {
        going_to_be_destroyed = true;
    }
    public string GetDebugText()
    {
        string text = "name : " + name +"\n";
        text += "type : " + GetType().Name.ToLower() + "\n";
        text += "skin : " + Skin + "\n\n";
        text += $"position :\n>>> x : {transform.position.x.ToString("F2")}\n>>> y : {transform.position.y.ToString("F2")}\n";
        text += "orientation : " + anim_player.orientation + $"\n>>> x : {orientation.x.ToString("F2")}\n>>> y : {orientation.y.ToString("F2")}\n";

        text += "\ncapacities : " + capacities.Count + "\n";
        List<string> capa_names = capacities.ConvertAll(c => c.name);
        text += ">>> " + string.Join(", ", capa_names) + "\n";

        if (Inventory != null)
        {
            List<Usable> usables = Inventory.Items.Where(i => i is Usable).Cast<Usable>().ToList();
            if (usables.Count > 0)
            {
                List<string> capa_from_items_names = usables.ConvertAll(i => i.UseLabel).Distinct().ToList();
                text += "capacities from items : " + capa_from_items_names.Count + "\n";
                text += ">>> " + string.Join(", ", capa_from_items_names) + "\n";
            }
        }

        return text;
    }


}

[Serializable]
public enum Effect
{
    // an effect is a temporary state that can be applied to a capable
    // it can be a buff, a debuff, a status, etc.
    SemiGhost, // allow a Movable to walk through other Beings
    Ghost, // a Movable can walk through other Beings & walls & objects (everything)
    Invisible, // a Being can't be seen -> change its body collider to Ghosts layer
    Invincible, // a Being can't be hurt
    Stunned, // a Being can't attack
    RegenLife, // a Being regenerates life
    Immobile, // a Movable can't move
    Unstoppable, // a Movable can't be stopped in its attack
    BeingCarried, // a Movable is being carried (bypass all movement updates)
    Boiling, // boiling, when the water is RILLY HOT -> deals damage to beings
    Burning // litteraly in FIRE -> deals damage mainly, also transmit heat
}