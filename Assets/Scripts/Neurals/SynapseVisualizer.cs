using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SynapseVisualizer : MonoBehaviour
{

    [Header("Synapse Data")]
    public Synapse synapse;

    [Header("Synapse Visuals")]
    [SerializeField] private SpriteRenderer line;
    // [SerializeField] private TextMeshPro label;
    [SerializeField] private Color base_synapse_color = Color.white;
    [SerializeField] private Color active_synapse_color = Color.yellow;

    public void Initialize(Synapse synapse)
    {
        this.synapse = synapse;
        UpdateVisual();
    }

    // UPDATE VISUAL
    public void UpdateVisual(bool active = false)
    {
        // we set the line position
        transform.position = synapse.neuronA.position;

        // we set the color
        line.color = active ? active_synapse_color : base_synapse_color;

        // set scale
        float distance = Vector3.Distance(transform.position, synapse.neuronB.position);
        transform.localScale = new Vector3(transform.localScale.x, distance * line.sprite.pixelsPerUnit, transform.localScale.z);

        // set rotation (from A to B)
        Vector3 dir = synapse.neuronB.position - synapse.neuronA.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        line.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}