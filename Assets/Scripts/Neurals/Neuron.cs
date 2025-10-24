using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable] public class Neuron
{
    // Neuron implementation
    public string name;
    public Vector3 position;
    public bool is_goal = false;

    // connected synapses
    [SerializeReference] public List<Synapse> synapses = new List<Synapse>();

    // CONSTRUCTOR
    public Neuron(string name, Vector3 position)
    {
        this.name = name;
        this.position = position;
    }
}


[Serializable] public class Synapse
{
    public Neuron neuronA;
    public Neuron neuronB;
    public int weightA; // weight for going into the synapse via A (and then going to B)
    public int weightB; // weight for going into the synapse via B (and then going to A)


    // CONSTRUCTOR
    public Synapse(Neuron a, Neuron b)
    {
        neuronA = a;
        neuronB = b;
        weightA = 1;
        weightB = 1;
    }

    public void Init()
    {
        neuronA.synapses.Add(this);
        neuronB.synapses.Add(this);
    }
}


public class Sensation
{
    public string type;
    public float intensity;
    public Neuron initiator;
    public Neuron receiver;
}