using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_WorldCreator : UI_SlottablePool, Scrollable
{
    [Header("UI_Worlds")]
    [SerializeField] private UI_WorldTemplateSlot prefab;
    [SerializeField] private Transform templates_container;
    private List<UI_WorldTemplateSlot> template_slots = new List<UI_WorldTemplateSlot>();
    private UI_WorldTemplateSlot selected_prefab = null;

    [Header("Scrollable")]
    [SerializeField] private UI_Scroller scroller;
    public UI_Scroller Scroller => scroller;

    // BEFORE SHOWING
    protected override void before_adding_to_stack()
    {
        base.before_adding_to_stack();

        // clear the template_slots slots if any
        /* foreach (UI_WorldTemplateSlot template_slot in template_slots)
        {
            Destroy(template_slot.gameObject);
        }
        template_slots.Clear();
        selected_prefab = null;

        // create the world slots based on the world templates
        WorldManager.Instance.RefreshExistingWorldsTemplates();
        List<WorldData> existing_worlds_data_list = WorldManager.Instance.ExistingWorlds;
        existing_worlds_data_list.Sort((a, b) => b.CompareTime(a)); // we sort the worlds by last ~~played~~ date
        foreach (WorldData world_data in existing_worlds_data_list)
        {
            UI_WorldSlot world_slot = Instantiate(world_slot_prefab, world_slots_container);
            world_slot.Initialize(world_data);
            world_slots.Add(world_slot);
        }

        // we reset the scroller to the top
        scroller.ResetPosition(); */
    }
}