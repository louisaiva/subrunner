using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UI_WorldSelector : UI_SlottablePool
{
    [SerializeField] private GameObject ui_chroma_title;

    [Header("UI_Worlds")]
    [SerializeField] private UI_WorldSlot world_slot_prefab;
    [SerializeField] private Transform world_slots_container;
    private List<UI_WorldSlot> world_slots = new List<UI_WorldSlot>();

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();
        ui_chroma_title.SetActive(false);
        
        // we create the world slots
        List<WorldData> existing_worlds_data_list = WorldManager.Instance.ExistingWorlds;

        foreach (WorldData world_data in existing_worlds_data_list)
        {
            UI_WorldSlot world_slot = Instantiate(world_slot_prefab, world_slots_container);
            world_slot.Initialize(world_data);
            world_slots.Add(world_slot);
        }
    }
    protected override IEnumerator disable_coroutine()
    {
        // we destroy the world slots
        foreach (UI_WorldSlot world_slot in world_slots)
        {
            Destroy(world_slot.gameObject);
        }
        world_slots.Clear();

        ui_chroma_title.SetActive(true);
        yield return base.disable_coroutine();
    }
}