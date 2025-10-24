using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// loads a set of neuron & synapses from the children's NeuronVisualizer & SynapseVisualizer.
/// Converts those visu into structs and give them to the NeuralBrain
/// </summary>
public class NeuralBrainLoader : MonoBehaviour
{

    [SerializeField] private NeuralBrain neuralBrain;
    [SerializeField] private NeuralBrainVisualizer visualizer;

    [Header("Visus")]
    private List<NeuronVisualizer> neuron_visualizers = new List<NeuronVisualizer>();
    private List<SynapseVisualizer> synapse_visualizers = new List<SynapseVisualizer>();

    // AWAKE
    public void Awake()
    {
        // we get all existing children
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            // try to get a neuron
            NeuronVisualizer neuronVisu = child.GetComponent<NeuronVisualizer>();
            if (neuronVisu != null) { neuron_visualizers.Add(neuronVisu); continue; }

            // try to get a synapse
            SynapseVisualizer synapseVisu = child.GetComponent<SynapseVisualizer>();
            if (synapseVisu != null) { synapse_visualizers.Add(synapseVisu); }
        }

        // we warn the visualizer that we found some visu
        visualizer.LoadExistingVisus(neuron_visualizers, synapse_visualizers);

        // we convert visu into structs
        List<Neuron> neurons = new List<Neuron>();
        for (int i = 0; i < neuron_visualizers.Count; i++)
        {
            neuron_visualizers[i].neuron.SetPosition(neuron_visualizers[i].transform.position);
            neurons.Add(neuron_visualizers[i].neuron);
        }
        List<Synapse> synapses = new List<Synapse>();
        for (int i = 0; i < synapse_visualizers.Count; i++)
        {
            synapses.Add(synapse_visualizers[i].synapse);
        }

        // and give them to the brain
        neuralBrain.SetBrain(neurons, synapses);
    }

    /* private Synapse load_synapse(SynapseData synapseVisu)
    {
    } */
}