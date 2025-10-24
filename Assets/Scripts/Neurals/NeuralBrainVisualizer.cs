using System.Collections.Generic;
using UnityEngine;

public class NeuralBrainVisualizer : MonoBehaviour
{
    [SerializeField] private NeuralBrain neuralBrain;

    [Header("Neurons")]
    [SerializeField] private GameObject neuron_visualizer_prefab;
    [SerializeField] private List<NeuronVisualizer> neuron_visualizers = new List<NeuronVisualizer>();
    public List<NeuronVisualizer> NeuronVisualizers => neuron_visualizers;

    [Header("Synapses")]
    [SerializeField] private GameObject synapse_visualizer_prefab;
    [SerializeField] private List<SynapseVisualizer> synapse_visualizers = new List<SynapseVisualizer>();
    public List<SynapseVisualizer> SynapseVisualizers => synapse_visualizers;

    // AWAKE
    private void Awake()
    {
        neuralBrain.OnNeuronCreated += HandleNeuronCreated;
        neuralBrain.OnSynapseCreated += HandleSynapseCreated;
        neuralBrain.OnSynapseDeleted += HandleSynapseDeleted;
        neuralBrain.OnStartedThinking += HandleStartedThinking;
        neuralBrain.OnCompletedThinking += HandleCompletedThinking;
    }

    // NEURON & SYNAPSE CREATION
    public void LoadExistingVisus(List<NeuronVisualizer> neuronVisus, List<SynapseVisualizer> synapseVisus)
    {
        neuron_visualizers = neuronVisus;
        synapse_visualizers = synapseVisus;
    }
    private void HandleNeuronCreated(Neuron neuron)
    {
        // checks that we don't have a visu for this neuron aleady
        for (int i = 0; i < neuron_visualizers.Count; i++)
        {
            if (neuron_visualizers[i].neuron == neuron)
            {
                neuron_visualizers[i].Initialize(neuron);
                return;
            }
        }

        // creates a visu
        GameObject neuronGO = Instantiate(neuron_visualizer_prefab, transform);
        neuronGO.name = $"neuron_visu_{neuron.name}";
        NeuronVisualizer visualizer = neuronGO.GetComponent<NeuronVisualizer>();
        visualizer.Initialize(neuron);
        neuron_visualizers.Add(visualizer);
    }
    private void HandleSynapseCreated(Synapse synapse)
    {
        // checks that we don't have a visu for this synapse aleady
        for (int i = 0; i < synapse_visualizers.Count; i++)
        {
            if (synapse_visualizers[i].synapse == synapse)
            {
                synapse_visualizers[i].Initialize(synapse);
                return;
            }
        }

        // creates a visu
        GameObject synapseGO = Instantiate(synapse_visualizer_prefab, transform);
        synapseGO.name = $"synapse_visu_{synapse.neuronA.name}_{synapse.neuronB.name}";
        SynapseVisualizer visualizer = synapseGO.GetComponent<SynapseVisualizer>();
        visualizer.Initialize(synapse);
        synapse_visualizers.Add(visualizer);
    }
    private void HandleSynapseDeleted(Synapse synapse)
    {
        // finds the visu
        SynapseVisualizer visualizerToRemove = null;
        for (int i = 0; i < synapse_visualizers.Count; i++)
        {
            if (synapse_visualizers[i].synapse == synapse)
            {
                visualizerToRemove = synapse_visualizers[i];
                break;
            }
        }
        if (visualizerToRemove == null) { Debug.LogWarning($"[NeuralBrainVisualizer] Could not remove synapse : no visualizer found"); return; }
        
        synapse_visualizers.Remove(visualizerToRemove);
        Destroy(visualizerToRemove.gameObject);
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