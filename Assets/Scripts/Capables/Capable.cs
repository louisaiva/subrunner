using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Mother class of all the Capables in the game.
/// A Capable is a gameObject that can have Capacities.
/// Each Capacity is linked to an animation.
/// So every animated element in the game is a Capable.
/// </summary>
[RequireComponent(typeof(AnimPlayer))]
public class Capable : MonoBehaviour, Debuggable
{
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


    // PROPERTIES
    public AnimPlayer anim_player { get; private set; }
    public CapacityBank bank { get; private set; }

    // un capable peut aussi avoir un inventaire
    public Inventory Inventory
    {
        get
        {
            Transform inventory_transform = transform.Find("inventory");
            if (inventory_transform == null) { return null; }
            return inventory_transform.GetComponent<Inventory>();
        }
    }

    // et des capacités electroniques
    public virtual ConnectCapacity Connector
    {
        // ? réellement logique que ça soit là ça ???
        // todo on peut pas le mettre dans Hacker/Vulnerable ou simplement utiliser Device ?
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
    public bool debug = false;
    public bool activate_all_capacities_logs_on_awake = false;

    // START
    protected virtual void Awake()
    {
        // we get the anim player
        anim_player = GetComponent<AnimPlayer>();
        Orientation = orientation;

        // we get the capacity bank
        bank = GameObject.Find("/utils/bank").GetComponent<CapacityBank>();

        // we add all the capacities that are in the gameObject
        foreach (Transform child in transform)
        {
            Capacity capa = child.GetComponent<Capacity>();
            if (!capa) { continue; } // if no capacity, we skip

            // we add it to the list
            capacities.Add(capa);
            if (debug) { Debug.Log("(Capable) " + name + " : capacity " + capa.name + " found on awake"); }

            // we check if the debug is true then we force debug to be true
            if (activate_all_capacities_logs_on_awake) { capa.debug = true; }
        }
        DebugManager.Instance.transform.GetComponentInChildren<EntitiesDebug>()?.AddEntity(this);
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
    public void Do(string name)
    {
        // get the capacity
        Capacity capacity = GetCapacity(name);

        capacity.Use(this);
    }
    public Capacity AddCapacity(string name)
    {
        // we check if the capacity is already in the list
        if (HasCapacity(name)) { return GetCapacity(name); }

        // get the capacity instance
        GameObject capa_instance = bank.GetCapacityInstance(name);

        // we put it as a child of the capable & we rename it
        capa_instance.transform.parent = transform;
        capa_instance.name = name;
        capa_instance.transform.localPosition = Vector3.zero;

        // we put the capacity in the list
        Capacity capa = capa_instance.GetComponent<Capacity>();
        capacities.Add(capa);
        if (debug) { Debug.Log("(Capable) " + this.name + " : capacity " + name + " added"); }

        return capa;
    }
    public void RemoveCapacity(string name)
    {
        foreach (Capacity capa in capacities)
        {
            if (capa.name == name)
            {
                capacities.Remove(capa);
                Destroy(capa.gameObject);
                if (debug) { Debug.Log("(Capable) " + this.name + " : capacity " + name + " removed"); }
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
    public virtual void AddEffect(Effect effect, float timetolive)
    {
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


    // DEBUG
    protected virtual void OnDestroy()
    {
        if (DebugManager.Instance == null) { return; } // this happens when the scene is destroyed when we quit the scene
        DebugManager.Instance.transform.GetComponentInChildren<EntitiesDebug>()?.RemoveEntity(this);
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

[Serializable] public enum Effect
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
    BeingCarried, // a Movable is being carried (bypass all movement updates)
}