using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TemplateCapableVisualizer : MonoBehaviour
{
    public string template;

    [Header("Status")]
    public CapablePlacementStatus status = CapablePlacementStatus.NotWorking;


    [Header("Components")]
    [SerializeField] private AnimPlayer player;
    [SerializeField] private Transform feet;

    [SerializeField] private Loggable<TemplateCapableVisualizer> log;

    // INIT & CLEAR
    public void Init(string template)
    {
        // here we get the CapableData from the CapableEngine
        CapableData data = CapableEngine.LazyInstance.GetTemplateData(template);
        if (data == null)
        {
            log.Warning($"No data found for template {template} !!");
            return;
        }

        this.template = template;

        // then we load the feet & anim data
        load_anim_data(data.anim_data);
        load_feet_data(data.feet_data);

        // then we start following the mouse :D
        log.Log($"Initialized with template {template}, we got the data : \n{data.GetDetails()}");
    }
    public void Clear()
    {
        // we clear feet
        HashSet<GameObject> feets = new HashSet<GameObject>();
        foreach (Transform foot in feet) { feets.Add(foot.gameObject); }
        ColliderBank.Instance.UnloadColliders(feets);

        // clear anim
        CapableBank.LazyInstance.LayerBank.UnloadAnimData(player);
        player.Hide();

        // then we start following the mouse :D
        log.Log($"Cleared visualizer");
    }


    // UPDATE
    private void Update()
    {
        if (string.IsNullOrEmpty(template)) { return; }

        // we get the position of the mouse
        // (we get the world position)
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector2 world_mouse = Camera.main.ScreenToWorldPoint(mousePos);

        // then we clamp the position on a grid ?
        // todo, do it

        // we place the object at right position
        transform.position = world_mouse;

        // then we update the placement status
        update_placement_status(); // todo, do it
    }
    private void update_placement_status()
    {
        // we check the collider

        // we update the status

        // and we update its material color based on status
    }



    // DATA MANAGEMENT
    private void load_feet_data(FeetData feet_data)
    {
        // load box colliders
        for (int i = 0; i < feet_data.box_colliders.Count; i++)
        {
            ColliderBank.Instance.LoadCollider(feet_data.box_colliders[i], feet);
        }

        // load circle colliders
        for (int i = 0; i < feet_data.circle_colliders.Count; i++)
        {
            ColliderBank.Instance.LoadCollider(feet_data.circle_colliders[i], feet);
        }
    }
    private void load_anim_data(AnimPlayerData anim_data)
    {
        // filter the data a little bit
        anim_data.sorting_layer_id = SortingLayer.NameToID("up");
        anim_data.order_in_layer = 2;
        anim_data.material_name = "outline_transparent_material";
        if (anim_data.layers != null)
        {
            foreach (AnimLayerData laydata in anim_data.layers)
            {
                laydata.material_name = "outline_transparent_material";
            }
        }

        player.LoadPlayerData(anim_data);

        // and the layers
        CapableBank.LazyInstance.LayerBank.LoadAnimData(player, anim_data);

        player.Show();
    }

}

public enum CapablePlacementStatus
{
    NotWorking,
    Placeable,
    NotPlaceable,
}