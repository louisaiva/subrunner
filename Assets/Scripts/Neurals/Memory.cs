using System.Collections.Generic;
using UnityEngine;

public class Memory
{
    public Sensation sensation;
    public List<Thought> thoughts;
    public Neuron decision;

    public Memory(Sensation sensation, List<Thought> thoughts, Neuron decision)
    {
        this.sensation = sensation;
        this.thoughts = thoughts;
        this.decision = decision;
    }

    // MEMORY MANAGEMENT
    public void Reinforce()
    {
        // reinforce memory -- reinforce thoughts that leads to the good decision
        for (int i = 0; i < thoughts.Count; i++)
        {
            Thought thought = thoughts[i];
            // we check if the thought went to the decision
            if (decision == null || thought.state == ThoughtState.TookDecision && thought.currentNeuron == decision)
            {
                modify_thought(thought, weight_added: 1);
            }
        }
    }
    public void Fragilise()
    {
        // fragilise memory -- fragilise thoughts that leads to the bad decision
        for (int i = 0; i < thoughts.Count; i++)
        {
            Thought thought = thoughts[i];
            // we check if the thought went to the decision
            if (decision == null || thought.state == ThoughtState.TookDecision && thought.currentNeuron == decision)
            {
                modify_thought(thought, weight_added: -1);
            }
        }
    }
    private void modify_thought(Thought thought, int weight_added = 1)
    {
        // we go through the traveled neurons and modify the synapses
        for (int i = 1; i < thought.traveledNeurons.Count; i++)
        {
            Neuron fromNeuron = thought.traveledNeurons[i - 1];
            Neuron toNeuron = thought.traveledNeurons[i];
            Synapse synapse = NeuralBrain.Instance.GetSynapseBetween(fromNeuron, toNeuron, createIfNotFound: false);
            if (synapse == null) { continue; }

            // we increase the weight in the direction of the travel
            synapse.ChangeWeight(fromNeuron, weight_added);

            string action = weight_added > 0 ? "Reinforced" : "Fragilised";
        }
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
        if (synapse == null || ttl <= 0)
        {
            state = ThoughtState.Failed;
            return;
        }
        Neuron nextNeuron = synapse.neuronA == currentNeuron ? synapse.neuronB : synapse.neuronA;

        traveledNeurons.Add(nextNeuron);

        OnThoughtStep?.Invoke(synapse);
        ttl--;

        // we check if this is a GoalSynapse then we took a decision !! we stop and complete
        if (nextNeuron.is_goal)
        {
            state = ThoughtState.TookDecision;
            return;
        }
    }

    private Synapse choose_synapse(Neuron currentNeuron, Neuron lastNeuron)
    {

        // add a very small chance to create a new synapse to a close not - connected neuron
        // so the brain can creates emergent decision
        float imagination_roll = Random.Range(0f, 1f);
        if (imagination_roll < NeuralBrain.Instance.imagination_percentage)
        {
            Neuron randomNeuron = NeuralBrain.Instance.GetRandomCloseNeuron(currentNeuron);
            if (randomNeuron != null)
            {
                Synapse randomSynapse = NeuralBrain.Instance.GetSynapseBetween(currentNeuron, randomNeuron, createIfNotFound: true);
                if (randomSynapse != null) { return randomSynapse; }
            }
        }

        // otherwise we simply choose common sense instead of imagination
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

        // if we reach here, something went wrong, we choose a random neuron
        if (synapses.Count == 0)
        {
            Neuron randomNeuron = NeuralBrain.Instance.GetRandomCloseNeuron(currentNeuron);
            if (randomNeuron == null) { return null; }
            Synapse randomSynapse = NeuralBrain.Instance.GetSynapseBetween(currentNeuron, randomNeuron, createIfNotFound: true);
            return randomSynapse;
        }
        return synapses[Random.Range(0, synapses.Count)];
    }
}


public enum ThoughtState
{
    Thinking,
    TookDecision,
    Failed
}