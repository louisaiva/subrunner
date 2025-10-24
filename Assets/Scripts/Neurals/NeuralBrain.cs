using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralBrain : MonoBehaviour
{
    [SerializeField] private bool log = false;

    [Header("Neural Data")]
    [SerializeField] private List<Neuron> neurons = new List<Neuron>();
    [SerializeField] private List<Synapse> synapses = new List<Synapse>();
    public System.Action<Neuron> OnNeuronCreated;
    public System.Action<Synapse> OnSynapseCreated;

    [Header("Thinking Settings")]
    [SerializeField] private int max_thinking_steps = 100;
    [SerializeField] private float thinking_delay = 0.1f; // delay between thinking steps
    public System.Action<Thought[]> OnStartedThinking;
    public System.Action<Thought[]> OnCompletedThinking;

    // START
    private void Start()
    {
        // we call OnCreated for all neurons & synapses we already have
        for (int i = 0; i < neurons.Count; i++)
        {
            OnNeuronCreated?.Invoke(neurons[i]);
        }
        for (int i = 0; i < synapses.Count; i++)
        {
            synapses[i].Init();
            OnSynapseCreated?.Invoke(synapses[i]);
        }
    }


    // SENSE
    public void Sense(string type, float intensity, string initiator, string receiver)
    {
        // we check if we have those 3 neurons already
        Neuron sensationNeuron = GetNeuronByName(type);
        Neuron initiatorNeuron = GetNeuronByName(initiator);
        Neuron receiverNeuron = GetNeuronByName(receiver);

        // we link the initiator & the sensation & the receiver & the sensation
        Synapse synapseA = GetSynapseBetween(initiatorNeuron, sensationNeuron);
        Synapse synapseB = GetSynapseBetween(receiverNeuron, sensationNeuron);

        // we create 3 raycasts from each neuron and wait for them to arrive
        // ? should a big intensity make more raycast or just longer raycasts ?

        if (log) { Debug.Log($"[NeuralBrain] Sensed {type} with intensity {intensity} from {initiator} to {receiver}"); }

        // we start thinking
        Thought[] thoughts = new Thought[3];
        thoughts[0] = new Thought(initiatorNeuron, (int)intensity);
        thoughts[1] = new Thought(sensationNeuron, (int)intensity);
        thoughts[2] = new Thought(receiverNeuron, (int)intensity);
        OnStartedThinking?.Invoke(thoughts);
        StartCoroutine(think_coroutine(thoughts));
    }
    private IEnumerator think_coroutine(Thought[] thoughts)
    {
        // we loop while at least one thought is still thinking
        bool anyThinking = true;
        int step = 0;
        while (anyThinking && step < max_thinking_steps)
        {
            anyThinking = false;
            for (int i = 0; i < thoughts.Length; i++)
            {
                Thought thought = thoughts[i];
                if (thought.state == ThoughtState.Thinking)
                {
                    thought.Think();
                    anyThinking = true;
                }
            }
            step++;
            yield return new WaitForSeconds(thinking_delay);
        }

        if (log) { Debug.Log($"[NeuralBrain] Thinking completed in {step} steps."); }

        // we analyse the thoughts
        Dictionary<Neuron, int> goalCounts = new Dictionary<Neuron, int>();
        for (int i = 0; i < thoughts.Length; i++)
        {
            Thought thought = thoughts[i];
            if (thought.state != ThoughtState.Completed) { continue; }

            Neuron goalNeuron = thought.currentNeuron;
            if (!goalCounts.ContainsKey(goalNeuron))
            {
                goalCounts[goalNeuron] = 0;
            }
            goalCounts[goalNeuron]++;
        }

        // we check which goal neurons won
        Neuron finalGoal = null;
        int maxCount = 0;
        foreach (var pair in goalCounts)
        {
            if (pair.Value > maxCount)
            {
                maxCount = pair.Value;
                finalGoal = pair.Key;
            }
        }
        if (finalGoal != null)
        {
            if (log) { Debug.Log($"[NeuralBrain] Final goal neuron: {finalGoal.name} with {maxCount} votes."); }
        }
        else
        {
            if (log) { Debug.Log($"[NeuralBrain] No goal neuron reached."); }
        }

        OnCompletedThinking?.Invoke(thoughts);
    }


    // GETTERS
    public Synapse GetSynapseBetween(Neuron a, Neuron b)
    {
        // try to find existing synapse
        for (int i = 0; i < a.synapses.Count; i++)
        {
            Synapse synapse = a.synapses[i];
            if (synapse.neuronA == b || synapse.neuronB == b)
            {
                return synapse;
            }
        }

        // if not found, create it
        Synapse newSynapse = new Synapse(a, b);
        newSynapse.Init();
        synapses.Add(newSynapse);
        OnSynapseCreated?.Invoke(newSynapse);

        if (log) { Debug.Log($"[NeuralBrain] Created synapse between {a.name} and {b.name}"); }
        return newSynapse;
    }
    private Neuron GetNeuronByName(string name)
    {
        // try to find the neuron by name
        Neuron neuron = neurons.Find(n => n.name == name);
        if (neuron != null) { return neuron; }

        // and create one if not found
        return create_neuron(name);
    }

    // NEURON CREATION / REMOVAL
    private Neuron create_neuron(string name)
    {
        Neuron neuron = new Neuron(name, Random.insideUnitSphere * 5f);

        neurons.Add(neuron);
        OnNeuronCreated?.Invoke(neuron);

        if (log) { Debug.Log($"[NeuralBrain] Created neuron: {name}"); }

        return neuron;
    }
}

public class Thought
{
    public ThoughtState state;
    public List<Neuron> traveledNeurons = new List<Neuron>();
    public Neuron currentNeuron => traveledNeurons[traveledNeurons.Count - 1];
    public Neuron lastNeuron => traveledNeurons.Count > 1 ? traveledNeurons[traveledNeurons.Count - 2] : null;
    public int ttl; // time to live in neurons traveled

    public System.Action<Synapse> OnThoughtStep;

    public Thought(Neuron creator, int ttl)
    {
        traveledNeurons.Add(creator);
        this.ttl = ttl;
        state = ThoughtState.Thinking;
    }

    public void Think()
    {
        // we find the next neuron (by choosing the synapse from the actual neuron)
        Synapse synapse = choose_synapse(currentNeuron, lastNeuron);
        Neuron nextNeuron = synapse.neuronA == currentNeuron ? synapse.neuronB : synapse.neuronA;

        traveledNeurons.Add(nextNeuron);

        // we check if this is a GoalSynapse then we took a decision !! we stop and complete
        if (nextNeuron.is_goal)
        {
            state = ThoughtState.Completed;
            return;
        }
        ttl--;

        OnThoughtStep?.Invoke(synapse);
    }

    private Synapse choose_synapse(Neuron currentNeuron, Neuron lastNeuron)
    {

        // todo add a very small chance to create a new synapse to a close not - connected neuron
        // so the brain can creates emergent decision

        int totalWeight = 0;
        List<Synapse> synapses = new List<Synapse>();
        List<int> weights = new List<int>();

        // we calculate the potentials synapses, their weights and the total weight
        for (int i = 0; i < currentNeuron.synapses.Count; i++)
        {
            Synapse synapse = currentNeuron.synapses[i];
            Neuron otherNeuron = synapse.neuronA == currentNeuron ? synapse.neuronB : synapse.neuronA;

            if (lastNeuron != null && otherNeuron == lastNeuron) { continue; }

            synapses.Add(synapse);
            int weight = synapse.neuronA == currentNeuron ? synapse.weightA : synapse.weightB;
            weights.Add(weight);
            totalWeight += weight;
        }

        // we pick a random synapse weighted by weights
        int randomValue = Random.Range(0, totalWeight);
        for (int i = 0; i < weights.Count; i++)
        {
            if (randomValue < weights[i])
            {
                return synapses[i];
            }
            randomValue -= weights[i];
        }

        // if we reach here, something went wrong, we choose a random synapse
        return synapses[Random.Range(0, synapses.Count)];
    }
}


public enum ThoughtState
{
    Thinking,
    Completed
}