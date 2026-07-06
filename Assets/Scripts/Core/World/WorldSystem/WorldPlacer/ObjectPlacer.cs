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
        // check if we have a navigator and if it has a hovered ui element
        if (UI_Navigator.Instance.IsHoveringSlot)
        {
            if (capable_visu.gameObject.activeSelf) { capable_visu.gameObject.SetActive(false); }
            return;
        }
        if (!capable_visu.gameObject.activeSelf) { capable_visu.gameObject.SetActive(true); }


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
        // we get the capable data 
        CapableData data = CapableEngine.Instance.DuplicateTemplate(capable_visu.template);
        if (data == null) { log.Warning("(ObjectPlacer) failed to get capable data for template " + capable_visu.template); return; }

        // we apply some small modifications to the data (like setting the position to the mouse position)
        data.position = capable_visu.GetPosition();

        // also if this is an item we make sure it is not grabbed since we spawn it on the ground
        if (data is ItemData item_data) { item_data.is_grabbed = false; }

        // we spawn the object !!
        Capable spawned_object = CapableEngine.Instance.SpawnCapable(data);
        if (spawned_object == null) { log.Warning("(ObjectPlacer) failed to spawn capable with template " + capable_visu.template); return; }
    }
    private void handle_right_click() { UI_Manager.Instance.UnstackPool("capable_placer"); }
}