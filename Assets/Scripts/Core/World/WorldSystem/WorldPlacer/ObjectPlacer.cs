using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// This script is a SubSystem of WorldPlacer (maybe having 2 scripts is overkill, we will see).
/// This store the capable visualizer, and call its methods.
/// </summary>
public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] private TemplateCapableVisualizer capable_visu;

    [SerializeField] private Loggable<ObjectPlacer> log;

    // MAIN ENTRY POINTS
    public bool StartPlacingCapable(string template)
    {
        // we check if we have a capable_visu
        if (capable_visu == null) { Debug.LogError("(ObjectPlacer) capable_visu is null, set it in inspector"); return false; }

        // we init the capable_visu
        capable_visu.Init(template);

        log.Log("Started placing capable template : " + template);
        return true;
    }
    public void CancelCapablePlacement()
    {
        // we clear the template capable visu
        capable_visu.Clear();
        log.Log("Cancel placing capable template");
    }


    // UPDATE
    private bool holding_right_click = false;
    private bool holding_left_click = false;
    private void Update()
    {
        // check clicks 

        // LEFT CLICK (add cell)
        if (Input.GetMouseButtonDown(0) && !holding_right_click)
        {
            // todo change color to full green
            holding_left_click = true;
        }
        if (Input.GetMouseButtonUp(0) && holding_left_click)
        {
            holding_left_click = false;
            handle_left_click();
        }

        // RIGHT CLICK (remove cell)
        if (Input.GetMouseButtonDown(1) && !holding_left_click)
        {
            // todo change color to full red
            holding_right_click = true;
        }
        if (Input.GetMouseButtonUp(1) && holding_right_click)
        {
            holding_right_click = false;
            handle_right_click();
        }
    }

    private void handle_left_click()
    {
        // we place the object at the mouse position !!
        Capable spawned_object = CapableEngine.Instance.SpawnCapable(capable_visu.template);
        if (spawned_object == null) { Debug.LogWarning("(ObjectPlacer) failed to spawn capable with template " + capable_visu.template); return; }

        // we set its position to the mouse position
        Vector2 position = capable_visu.GetPosition();
        // Vector3 mousePos = Mouse.current.position.ReadValue();
        // Vector2 world_mouse = Camera.main.ScreenToWorldPoint(mousePos);
        spawned_object.transform.position = new Vector3(position.x, position.y, spawned_object.transform.position.z);
    }
    private void handle_right_click() { UI_Manager.Instance.UnstackPool("capable_placer"); }
}