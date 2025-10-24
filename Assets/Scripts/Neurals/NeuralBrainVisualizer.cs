using System.Collections.Generic;
using UnityEngine;

public class NeuralBrainVisualizer : MonoBehaviour
{
    [SerializeField] private NeuralBrain neuralBrain;

    [Header("Neurons")]
    [SerializeField] private GameObject neuron_visualizer_prefab;
    [SerializeField] private List<NeuronVisualizer> neuron_visualizers = new List<NeuronVisualizer>();

    [Header("Synapses")]
    [SerializeField] private GameObject synapse_visualizer_prefab;
    [SerializeField] private List<SynapseVisualizer> synapse_visualizers = new List<SynapseVisualizer>();

    // AWAKE
    private void Awake()
    {
        neuralBrain.OnNeuronCreated += HandleNeuronCreated;
        neuralBrain.OnSynapseCreated += HandleSynapseCreated;
        neuralBrain.OnStartedThinking += HandleStartedThinking;
        neuralBrain.OnCompletedThinking += HandleCompletedThinking;
    }

    // NEURON & SYNAPSE CREATION
    private void HandleNeuronCreated(Neuron neuron)
    {
        GameObject neuronGO = Instantiate(neuron_visualizer_prefab, transform);
        NeuronVisualizer visualizer = neuronGO.GetComponent<NeuronVisualizer>();
        visualizer.Initialize(neuron);
        neuron_visualizers.Add(visualizer);
    }
    private void HandleSynapseCreated(Synapse synapse)
    {
        GameObject synapseGO = Instantiate(synapse_visualizer_prefab, transform);
        SynapseVisualizer visualizer = synapseGO.GetComponent<SynapseVisualizer>();
        visualizer.Initialize(synapse);
        synapse_visualizers.Add(visualizer);
    }

    // THINKING HANDLERS
    private void HandleStartedThinking(Thought[] thoughts)
    {
        // we set all visuals to inactive
        foreach (var neuronVis in neuron_visualizers)
        {
            neuronVis.UpdateVisual(false);
        }
        foreach (var synapseVis in synapse_visualizers)
        {
            synapseVis.UpdateVisual(false);
        }

        // we register thoughts events
        for (int i = 0; i < thoughts.Length; i++)
        {
            thoughts[i].OnThoughtStep += HandleThoughtStep;
        }
    }
    private void HandleCompletedThinking(Thought[] thoughts)
    {
        // we unregister thoughts events
        for (int i = 0; i < thoughts.Length; i++)
        {
            thoughts[i].OnThoughtStep -= HandleThoughtStep;
        }
    }
    private void HandleThoughtStep(Synapse synapse)
    {
        // we activate the synapse
        SynapseVisualizer synapseVis = synapse_visualizers.Find(v => v.synapse == synapse);
        if (synapseVis != null)
        {
            synapseVis.UpdateVisual(true);
        }

        // we activate neurons A & B visu
        bool foundA = false;
        bool foundB = false;
        for (int i = 0; i < neuron_visualizers.Count; i++)
        {
            NeuronVisualizer neuronVis = neuron_visualizers[i];
            if (neuronVis.neuron == synapse.neuronA)
            {
                neuronVis.UpdateVisual(true);
                foundA = true;
            }
            else if (neuronVis.neuron == synapse.neuronB)
            {
                neuronVis.UpdateVisual(true);
                foundB = true;
            }

            if (foundA && foundB) { break; }
        }
    }

}