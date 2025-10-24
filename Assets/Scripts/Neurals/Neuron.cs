using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class Neuron
{
    // Neuron implementation
    public string name;
    public Vector3 position;
    public bool is_goal;

    // connected synapses
    public List<Synapse> synapses;

    // events
    public Action<Neuron> OnNeuronMoved;

    // CONSTRUCTOR
    public Neuron(string name, Vector3 position)
    {
        this.name = name;
        this.position = position;
        this.is_goal = false;
        this.synapses = new List<Synapse>();
        this.OnNeuronMoved = null;
    }

    // SETTERS
    public void SetPosition(Vector3 newPosition)
    {
        position = newPosition;
        OnNeuronMoved?.Invoke(this);
    }
}


[Serializable]
public class Synapse
{
    [SerializeReference] public Neuron neuronA;
    [SerializeReference] public Neuron neuronB;
    public int weightA; // weight for going into the synapse via A (and then going to B)
    public int weightB; // weight for going into the synapse via B (and then going to A)

    public System.Action<Synapse> OnWeightChanged;


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

    // WEIGHT CHANGER
    public void ChangeWeight(Neuron fromNeuron, int delta)
    {
        if (fromNeuron == neuronA)
        {
            weightA += delta;
        }
        else if (fromNeuron == neuronB)
        {
            weightB += delta;
        }
        if (weightA < 0 || weightB < 0)
        {
            // we delete the synapse
            NeuralBrain.Instance.DeleteSynapse(this);
            return;
        }
        OnWeightChanged?.Invoke(this);
    }
}


public class Sensation
{
    public string type;
    public float intensity;
    public Neuron initiator;
    public Neuron receiver;
}