using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Capable : MonoBehaviour
{
    // un Capable est un gameObject qui possède des capacités
    // et donc des animations (les capacités peuvent être reliées à une animation)

    [Header("Components")]
    public AnimPlayer anim_player;
    public CapacityBank bank;

    [Header("Orientation")]
    // the analog equivalent of the anim_player.orientation which is numerical
    [SerializeField] protected Vector2 inputs; // inputs can be at 0,0
    [SerializeField] protected Vector2 orientation; // orientation can't be at 0,0 -> always normalized & remember last orientation
    public Vector2 Orientation {
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

    [Header("Capacities")]
    [SerializeField] protected List<Capacity> capacities = new List<Capacity>();

    [Header("Effects")]
    [SerializeField] protected List<Effect> effects = new List<Effect>();
    [SerializeField] protected List<float> effects_timetolive = new List<float>();

    // START
    protected virtual void Start()
    {
        // we get the anim player
        anim_player = GetComponent<AnimPlayer>();

        // we get the capacity bank
        bank = GameObject.Find("/utils/bank").GetComponent<CapacityBank>();

        // we add all the capacities that are in the gameObject
        foreach (Transform child in transform)
        {
            Capacity capa = child.GetComponent<Capacity>();
            if (capa != null)
            {
                add_capacity(capa);
            }
        }
    }

    // EVENTS
    /* private void Events()
    {
        // walk
        if (Can("run"))
        {
            // we get the inputs
            Vector2 inputs = new Vector2(0, 0);
            if (Input.GetKey(KeyCode.W)) { inputs.y += 1;}
            if (Input.GetKey(KeyCode.S)) { inputs.y -= 1;}
            if (Input.GetKey(KeyCode.A)) { inputs.x -= 1;}
            if (Input.GetKey(KeyCode.D)) { inputs.x += 1;}

            // we check if the player is running
            bool running = false;
            if (inputs.x != 0 || inputs.y != 0) { running = true; }

            // we update the orientation
            Orientation = inputs;

            // we play the animation
            if (running) { Do("run"); }
            else { anim_player.StopPlaying("run"); }
        }

        // ATTACK & HURT
        if (Input.GetKey(KeyCode.Space))
        {
            if (Can("attack")) { Do("attack"); }
        }
        if (Input.GetKey(KeyCode.H))
        {
            if (Can("hurted")) { Do("hurted"); }
        }

        // DIE
        if (Input.GetKey(KeyCode.F))
        {
            Do("die");
        }
    } */

    // UPDATES
    protected virtual void Update()
    {
        // we get the inputs
        // Events();

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
    
    // CAPACITIES
    public void Do(string name)
    {
        foreach (Capacity capacity in capacities)
        {
            if (capacity.name == name)
            {
                // we use the capacity
                capacity.Use(this);
            }
        }
    }
    public void ShowCapacities()
    {
        string capacities_str = "(Capable - " + gameObject.name + ") capacities : \n";
        foreach (Capacity capa in capacities)
        {
            capacities_str += capa.name + " : " + capa.Able + "\n";
        }
        Debug.Log(capacities_str);
    }
    private void add_capacity(Capacity capacity)
    {
        if (hasCapacity(capacity)) {return;}
        capacities.Add(capacity);
    }
    public void AddCapacity(string capa_name)
    {
        // we check if the capacity is already in the list
        if (hasCapacity(capa_name)) { return; }

        // get the capacity instance
        GameObject capa_instance = bank.GetCapacityInstance(capa_name);

        // we put it as a child of the capable & we rename it
        capa_instance.transform.parent = transform;
        capa_instance.name = capa_name;
        capa_instance.transform.localPosition = Vector3.zero;

        // we put the capacity in the list
        add_capacity(capa_instance.GetComponent<Capacity>());
    }
    public void RemoveCapacity(string capa_name)
    {
        foreach (Capacity capa in capacities)
        {
            if (capa.name == capa_name)
            {
                capacities.Remove(capa);
                Destroy(capa.gameObject);
                return;
            }
        }
    }


    // EFFECTS
    public virtual void AddEffect(Effect effect, float timetolive)
    {
        effects.Add(effect);
        effects_timetolive.Add(timetolive);
    }

    // GETTERS
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
    protected bool hasCapacity(string capa_name)
    {
        foreach (Capacity capacity in capacities)
        {
            if (capacity.name == capa_name)
            {
                return true;
            }
        }
        return false;
    }
    protected bool hasCapacity(Capacity capa)
    {
        return hasCapacity(capa.name);
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
    protected Capacity getCapacity(string name)
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
}

[Serializable] public enum Effect
{
    // an effect is a temporary state that can be applied to a capable
    // it can be a buff, a debuff, a status, etc.
    Ghost,
    Invincible,
    Stunned,
    RegenLife,
    Immobile,
}