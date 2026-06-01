using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_WorldSelector : UI_SlottablePool, Scrollable
{
    // [SerializeField] private GameObject ui_chroma_title;

    [Header("UI_Worlds")]
    [SerializeField] private UI_WorldSlot world_slot_prefab;
    [SerializeField] private Transform world_slots_container;
    private List<UI_WorldSlot> world_slots = new List<UI_WorldSlot>();

    [Header("Scrollable")]
    [SerializeField] private UI_Scroller scroller;
    public UI_Scroller Scroller => scroller;

    // BEFORE SHOWING
    protected override void before_showing()
    {
        base.before_showing();

        // clear the worlds slots if any
        foreach (UI_WorldSlot world_slot in world_slots)
        {
            Destroy(world_slot.gameObject);
        }
        world_slots.Clear();

        // create the world slots
        WorldManager.Instance.RefreshExistingWorldsData();
        List<WorldData> existing_worlds_data_list = WorldManager.Instance.ExistingWorlds;
        existing_worlds_data_list.Sort((a, b) => b.CompareTime(a)); // we sort the worlds by last ~~played~~ date
        foreach (WorldData world_data in existing_worlds_data_list)
        {
            UI_WorldSlot world_slot = Instantiate(world_slot_prefab, world_slots_container);
            world_slot.Initialize(world_data);
            world_slots.Add(world_slot);
        }

        // we reset the scroller to the top
        scroller.ResetPosition();
    }
}