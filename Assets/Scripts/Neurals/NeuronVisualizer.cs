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

    public void Initialize(Neuron neuron)
    {
        this.neuron = neuron;
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
}