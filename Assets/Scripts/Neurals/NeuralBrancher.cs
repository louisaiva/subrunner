using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles mouse movement & neuron & synapse hovers & clicks
/// to create synapse when clicking on 2 neurons, or deleting
/// synapses when right click on synapse, or creating neurons
/// when left click on synapse (creates 2 synapses from the old one)
/// and also manages neuron dragging
/// </summary>
public class NeuralBrancher : MonoBehaviour
{
    [SerializeField] private NeuralBrain neuralBrain;
    [SerializeField] private NeuralBrainVisualizer visualizer;

    // [Header("Brancher Settings")]
    // [SerializeField] private float hover_distance = 0.5f;


    [Header("Hover")]
    [SerializeField] private GameObject hovered_object = null;
    private LayerMask layerMask;

    [Header("Click & Drag")]
    [SerializeField] private GameObject clicked_object = null;
    [SerializeField] private GameObject dragged_object = null;
    [SerializeField] private bool dragged = false;

    [Header("Inputs")]
    Vector2 lastMousePosition;
    [SerializeField] private bool left_downed = false;
    // private bool right_downed = false;

    [Header("Logs")]
    [SerializeField] private bool log_hover = false;
    [SerializeField] private bool log_click = false;

    // AWAKE
    private void Awake()
    {
        layerMask = LayerMask.GetMask("Neurals");
    }

    // FIXED UPDATE
    private void FixedUpdate()
    {
        // HOVER
        update_hover();

        // CLICK
        update_click();

        // DRAG
        update_drag();

        lastMousePosition = Input.mousePosition;
    }
    private void update_hover()
    {
        // get mouse pos
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (log_hover) { Debug.Log($"[NeuralBrancher] Mouse world pos: {mouseWorldPos} ---- Mouse screen pos: {Input.mousePosition}"); }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // we do a raycast to touch a neuron/synapse
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 100f, layerMask);
        if (hits.Length == 0) { unhover(); return; }


        // we get the object with the center of object closer to the hit
        GameObject closestObject = null;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            GameObject obj = hit.collider.gameObject;
            float distance = Vector3.Distance(hit.point, obj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = obj;
            }
        }

        // if we hit something different than before, we unhover the old one & hover the new one
        if (closestObject != hovered_object) { unhover(); }

        // we hover the new object
        hover(closestObject);
    }
    private void update_click()
    {
        if (Input.GetMouseButton(0) && !left_downed)
        {
            left_down();
        }
        if (!Input.GetMouseButton(0) && left_downed)
        {
            click();
        }
        /* else if (Input.GetMouseButtonDown(1) && !right_downed)
        {
            right_down();
        } */
        /* else if (Input.GetMouseButtonUp(1) && right_downed)
        {
            delete();
        } */
    }
    private void update_drag()
    {
        if (dragged_object == null) { return; }
        if (lastMousePosition == (Vector2)Input.mousePosition) { return; }

        // we update the neuron position if it is a neuron
        NeuronVisualizer neuronVis = dragged_object.GetComponent<NeuronVisualizer>();
        if (neuronVis == null) { return; }

        dragged = true;

        // get mouse pos
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // we set the dragged object position to mouse pos
        Vector3 pos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, dragged_object.transform.position.z);
        dragged_object.transform.position = pos;
        neuronVis.neuron.SetPosition(pos);
    }

    // HOVER / UNHOVER LOW LEVEL
    private void hover(GameObject obj)
    {
        hovered_object = obj;

        NeuronVisualizer neuronVis = obj.GetComponent<NeuronVisualizer>();
        if (neuronVis != null) { neuronVis.UpdateVisual(true); return; }

        SynapseVisualizer synapseVis = obj.GetComponent<SynapseVisualizer>();
        if (synapseVis != null) { synapseVis.UpdateVisual(true); return; }
    }
    private void unhover()
    {
        if (hovered_object == null) { return; }

        NeuronVisualizer neuronVis = hovered_object.GetComponent<NeuronVisualizer>();
        SynapseVisualizer synapseVis = hovered_object.GetComponent<SynapseVisualizer>();
        if (neuronVis != null) { neuronVis.UpdateVisual(false); }
        else if (synapseVis != null) { synapseVis.UpdateVisual(false); }

        hovered_object = null;
    }


    // CLICK / DELETE / DOWN LOW LEVEL
    private void left_down()
    {
        left_downed = true;
        if (hovered_object == null) { return; }

        // we scale up the hovered object
        scale(hovered_object, up:true);

        dragged_object = hovered_object;
    }
    private void click()
    {
        left_downed = false;

        // we release the drag
        if (dragged)
        {
            dragged = false;
            unhover();
            scale(dragged_object, up:false);
        }
        dragged_object = null;

        // if we clicked on nothing or dragged we simply release everything
        if (hovered_object == null)
        {
            // we release the clicked object
            if (clicked_object != null)
            {
                scale(clicked_object, up:false);
                clicked_object = null;
            }
            return;
        }

        // now we check if we already had a clicked object.
        // if yes and if it is a neuron, we connect them by creating a synapse 
        // and then scale both down, and release clicked object
        // if not we just register this clicked object
        if (clicked_object == null)
        {
            clicked_object = hovered_object;
            if (log_click) { Debug.Log($"[NeuralBrancher] Clicked object set to {clicked_object.name}"); }
            return;
        }

        // we have a clicked object that is a neuron, and we have a hovered object
        NeuronVisualizer neuronAVis = clicked_object.GetComponent<NeuronVisualizer>();
        NeuronVisualizer neuronBVis = hovered_object.GetComponent<NeuronVisualizer>();
        if (neuronAVis == null || neuronBVis == null)
        {
            // we scale down the clicked object
            scale(clicked_object, up: false);
            clicked_object = hovered_object;
            if (log_click) { Debug.Log($"[NeuralBrancher] Overrid clicked object to {clicked_object.name}"); }
            return;
        }
        if (neuronAVis.neuron == neuronBVis.neuron)
        {
            // we scale down the clicked object
            scale(clicked_object, up: false);
            clicked_object = null;
            if (log_click) { Debug.Log($"[NeuralBrancher] Clicked same neuron, released clicked object"); }
            return;
        }

        // we create the synapse if it does not exist
        neuralBrain.GetSynapseBetween(neuronAVis.neuron, neuronBVis.neuron, createIfNotFound:true);

        // we scale down the both objects
        scale(clicked_object, up:false);
        scale(hovered_object, up: false);
        
        if (log_click) { Debug.Log($"[NeuralBrancher] Created synapse between {neuronAVis.neuron.name} and {neuronBVis.neuron.name}"); }

        // we release clicked object
        clicked_object = null;
    }
    /* private void right_down()
    {
        right_downed = true;
    } */


    private void scale(GameObject obj, bool up)
    {
        if (obj.GetComponent<NeuronVisualizer>() != null)
        {
            if (up) { obj.GetComponent<NeuronVisualizer>().ScaleUp(); }
            else { obj.GetComponent<NeuronVisualizer>().ScaleDown(); }
        }
        else if (obj.GetComponent<SynapseVisualizer>() != null)
        {
            if (up) { obj.GetComponent<SynapseVisualizer>().ScaleUp(); }
            else { obj.GetComponent<SynapseVisualizer>().ScaleDown(); }
        }
    }
}