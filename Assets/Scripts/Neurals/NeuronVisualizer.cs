using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NeuronVisualizer : MonoBehaviour
{

    [Header("Neuron Data")]
    public Neuron neuron;

    [Header("Neuron Visuals")]
    [SerializeField] private SpriteRenderer sprite_renderer;
    [SerializeField] private TextMeshPro label;
    [SerializeField] private Color base_neuron_color = Color.white;
    [SerializeField] private Color active_neuron_color = Color.yellow;
    [SerializeField] private Color goal_neuron_color = Color.green;
    [SerializeField] private Color chosen_goal_color = Color.cyan;

    [Header("Scale Settings")]
    [SerializeField] private float scale_up_factor = 1.2f;
    private float base_scale = 1f;

    // INIT
    public void Initialize(Neuron neuron)
    {
        this.neuron = neuron;
        base_scale = transform.localScale.x;
        UpdateVisual();
    }

    // UPDATE VISUAL
    public void UpdateVisual(bool active = false)
    {
        transform.position = neuron.position;
        if (neuron.is_goal)
        {
            sprite_renderer.color = active ? chosen_goal_color : goal_neuron_color;
        }
        else
        {
            sprite_renderer.color = active ? active_neuron_color : base_neuron_color;
        }

        label.text = neuron.name;
    }

    // SCALING
    public void ScaleUp()
    {
        transform.localScale = base_scale * scale_up_factor * Vector3.one;
    }
    public void ScaleDown()
    {
        transform.localScale = base_scale * Vector3.one;
    }
}