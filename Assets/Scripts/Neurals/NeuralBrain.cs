using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralBrain : Singleton<NeuralBrain>
{
    [SerializeField] private bool log = false;

    [Header("Neural Data")]
    [SerializeField] private List<Neuron> neurons = new List<Neuron>();
    [SerializeField] private List<Synapse> synapses = new List<Synapse>();
    public System.Action<Neuron> OnNeuronCreated;
    public System.Action<Synapse> OnSynapseCreated;
    public System.Action<Synapse> OnSynapseDeleted;

    [Header("Thinking Settings")]
    public float imagination_percentage = 0.1f; // percentage of thinking steps that can go to random neurons
    [SerializeField] private int max_thinking_steps = 100;
    [SerializeField] private float thinking_delay = 0.1f; // delay between thinking steps
    public System.Action<Thought[]> OnStartedThinking;
    public System.Action<Thought[]> OnCompletedThinking;

    [Header("Memory Settings")]
    public Memory instant_memory;

    // START
    private void Start()
    {
        LoadBrain();
    }

    // BRAIN LOADING
    public void SetBrain(List<Neuron> neurons, List<Synapse> synapses)
    {
        this.neurons = neurons;
        this.synapses = synapses;
    }
    public void LoadBrain()
    {
        // we call OnCreated for all neurons & synapses we just loaded
        for (int i = 0; i < neurons.Count; i++)
        {
            OnNeuronCreated?.Invoke(neurons[i]);
        }
        for (int i = 0; i < synapses.Count; i++)
        {
            synapses[i].Init();
            OnSynapseCreated?.Invoke(synapses[i]);
        }

        if (log) { Debug.Log($"[NeuralBrain] Loaded brain with {neurons.Count} neurons and {synapses.Count} synapses."); }
    }

    // SENSE
    public void Sense(string type, float intensity, string initiator, string receiver)
    {
        // we check if we have those 3 neurons already
        Neuron sensationNeuron = GetNeuronByName(type, createIfNotFound: true);
        Neuron initiatorNeuron = GetNeuronByName(initiator, createIfNotFound: true);
        Neuron receiverNeuron = GetNeuronByName(receiver, createIfNotFound: true);

        // we link the initiator & the sensation & the receiver & the sensation
        Synapse synapseA = GetSynapseBetween(initiatorNeuron, sensationNeuron, createIfNotFound: true);
        Synapse synapseB = GetSynapseBetween(receiverNeuron, sensationNeuron, createIfNotFound: true);

        // we create 3 raycasts from each neuron and wait for them to arrive
        // ? should a big intensity make more raycast or just longer raycasts ?

        int steps = Mathf.CeilToInt(Mathf.Abs(intensity));

        if (log) { Debug.Log($"[NeuralBrain] Sensed {type} with intensity {intensity} from {initiator} to {receiver} ({steps} steps)"); }

        // we start thinking
        Thought[] thoughts = new Thought[3];
        thoughts[0] = new Thought(initiatorNeuron, steps);
        thoughts[1] = new Thought(sensationNeuron, steps);
        thoughts[2] = new Thought(receiverNeuron, steps);
        OnStartedThinking?.Invoke(thoughts);
        StartCoroutine(think_coroutine(thoughts));

        // we add to instant memory
        instant_memory = new Memory(
            new Sensation
            {
                type = type,
                intensity = intensity,
                initiator = initiatorNeuron,
                receiver = receiverNeuron
            },
            new List<Thought>(thoughts),
            null
        );
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
            if (thought.state == ThoughtState.Failed) { continue; }

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

        // we add to instant memory
        instant_memory.decision = finalGoal;
        OnCompletedThinking?.Invoke(thoughts);
    }


    // GETTERS
    public Synapse GetSynapseBetween(Neuron a, Neuron b, bool createIfNotFound = false)
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

        if (!createIfNotFound) { return null; }

        // if not found, create it
        Synapse newSynapse = new Synapse(a, b);
        newSynapse.Init();
        synapses.Add(newSynapse);
        OnSynapseCreated?.Invoke(newSynapse);

        if (log) { Debug.Log($"[NeuralBrain] Created synapse between {a.name} and {b.name}"); }
        return newSynapse;
    }
    public Neuron GetNeuronByName(string name, bool createIfNotFound = false)
    {
        // try to find the neuron by name
        Neuron neuron = neurons.Find(n => n.name == name);
        if (neuron != null) { return neuron; }
        if (!createIfNotFound) { return null; }

        // and create one if not found
        return create_neuron(name);
    }
    public Neuron GetRandomCloseNeuron(Neuron fromNeuron, float maxDistance = 15f)
    {
        List<Neuron> closeNeurons = new List<Neuron>();
        for (int i = 0; i < neurons.Count; i++)
        {
            Neuron neuron = neurons[i];
            if (neuron == fromNeuron) { continue; }
            if (Vector3.Distance(neuron.position, fromNeuron.position) <= maxDistance)
            {
                closeNeurons.Add(neuron);
            }
        }
        if (closeNeurons.Count == 0) { return null; }

        // we sort them by distance
        closeNeurons.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.position, fromNeuron.position);
            float distB = Vector3.Distance(b.position, fromNeuron.position);
            return distA.CompareTo(distB);
        });

        // 50 % chance to pick the closest
        for (int i = 0; i < closeNeurons.Count; i++)
        {
            int random = Random.Range(0, 2);
            if (random == 0) { return closeNeurons[i]; }
        }
        return closeNeurons[closeNeurons.Count - 1];
    }

    // NEURON CREATION / REMOVAL
    private Neuron create_neuron(string name)
    {
        Neuron neuron = new Neuron(name, Random.insideUnitSphere * 5f);
        neuron.SetPosition(new Vector3(neuron.position.x, neuron.position.y, 0f)); // we flatten z

        neurons.Add(neuron);
        OnNeuronCreated?.Invoke(neuron);

        if (log) { Debug.Log($"[NeuralBrain] Created neuron: {name}"); }

        return neuron;
    }
    public void DeleteSynapse(Synapse synapse)
    {
        synapse.neuronA.synapses.Remove(synapse);
        synapse.neuronB.synapses.Remove(synapse);
        synapses.Remove(synapse);

        OnSynapseDeleted?.Invoke(synapse);

        if (log) { Debug.Log($"[NeuralBrain] Deleted synapse between {synapse.neuronA.name} and {synapse.neuronB.name}"); }
    }
}
