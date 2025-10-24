using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SynapseVisualizer : MonoBehaviour
{

    [Header("Synapse Data")]
    public Synapse synapse;

    [Header("Synapse Visuals")]
    [SerializeField] private SpriteRenderer line;
    private bool active = false;
    // [SerializeField] private TextMeshPro label;
    [SerializeField] private Color base_synapse_color = Color.white;
    [SerializeField] private Color active_synapse_color = Color.yellow;

    [Header("Scale Settings")]
    [SerializeField] private float scale_up_factor = 1.2f;
    private float base_scale = 1f;

    private System.Action<Neuron> oneNeuronMoved;
    private System.Action<Synapse> onWeightChangedCallback;

    // INIT
    public void Initialize(Synapse synapse)
    {
        this.synapse = synapse;

        base_scale = transform.localScale.x;

        // creates callbacks
        oneNeuronMoved = (n) => UpdateVisual();
        onWeightChangedCallback = (s) => UpdateVisual();

        // subscribe to position updates
        synapse.neuronA.OnNeuronMoved += oneNeuronMoved;
        synapse.neuronB.OnNeuronMoved += oneNeuronMoved;

        // and weight update
        synapse.OnWeightChanged += onWeightChangedCallback;

        UpdateVisual();
    }

    // UPDATE VISUAL
    public void UpdateVisual(bool active = false)
    {
        this.active = active;

        // we set the line position
        transform.position = synapse.neuronA.position;

        // we set the color
        line.color = active ? active_synapse_color : base_synapse_color;

        // set thickness based on synapses weight
        base_scale = 0.8f + 0.1f * (synapse.weightA + synapse.weightB);

        // set scale
        float distance = Vector3.Distance(transform.position, synapse.neuronB.position);
        transform.localScale = new Vector3(base_scale, distance * line.sprite.pixelsPerUnit, transform.localScale.z);


        // set rotation (from A to B)
        Vector3 dir = synapse.neuronB.position - synapse.neuronA.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        line.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // SCALING
    public void ScaleUp()
    {
        transform.localScale = new Vector3(base_scale * scale_up_factor, transform.localScale.y, transform.localScale.z);
    }
    public void ScaleDown()
    {
        transform.localScale = new Vector3(base_scale, transform.localScale.y, transform.localScale.z);
    }


    // ON DESTROY
    public void OnDestroy()
    {
        synapse.neuronA.OnNeuronMoved -= oneNeuronMoved;
        synapse.neuronB.OnNeuronMoved -= oneNeuronMoved;

        // and weight update
        synapse.OnWeightChanged -= onWeightChangedCallback;
    }

}