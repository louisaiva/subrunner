using System;
using System.Collections.Generic;
using UnityEngine;

public class ExpCapacity : Capacity
{
    /* // on s'enregistre en tant que trigger dans l'XPProvider particle system
    var trigger_particle_module = XPProvider.Instance.GetComponent<ParticleSystem>().trigger;
    trigger_particle_module.SetCollider(0, GetCapacity<HealthCapacity>().HealthCollider); */
}

[Serializable] public class CapacityUpgrade<T, U> where T : Capacity where U : struct
{
    public T capacity_type; // todo : très peu serializable ça mdr
    public string variable_name;
    public List<U> levels; // todo ça aussi
    public int current_level = 0; 
}