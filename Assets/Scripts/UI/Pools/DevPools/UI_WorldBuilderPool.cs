using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_WorldBuilderPool : UI_SlottablePool, Descriptable
{

    [Header("Level Slots")]
    [SerializeField] private GameObject level_slot_prefab;
    [SerializeField] private RectTransform levels_container;
    private List<UI_LevelSlot> level_slots = new List<UI_LevelSlot>();

    [Header("References")]
    [SerializeField] private UI_WorldSlot world_slot;
    public string Name => world_slot.Name;
    public string Description => world_slot.Description;
    [SerializeField] private WorldFolderCaller world_folder_caller;


    protected override void before_adding_to_stack()
    {
        GameManager.State = GameState.Building;
        
        WorldDataHelper selected_world_data = SaveEngine.GetWorldData(WorldManager.Instance.SelectedWorld);
        if (selected_world_data == null) { Debug.LogError($"No world data found for selected world '{WorldManager.Instance.SelectedWorld}'"); return; }
        world_slot.Initialize(selected_world_data);
        world_folder_caller.world_id = WorldManager.Instance.SelectedWorld;
    }
    protected override void before_showing() { RefreshLevelSlots(); }
    public void RefreshLevelSlots()
    {
        // clear the levels slots if any
        foreach (UI_LevelSlot level_slot in level_slots)
        {
            Destroy(level_slot.transform.parent.gameObject);
        }
        level_slots.Clear();

        // we create the level slots
        List<LevelData> existing_levels_data_list = LevelEngine.LoadWorldLevelsData(WorldManager.Instance.SelectedWorld);
        foreach (LevelData ldata in existing_levels_data_list)
        {
            UI_LevelSlot slot = Instantiate(level_slot_prefab, levels_container).transform.Find("level_slot").GetComponent<UI_LevelSlot>();
            slot.Initialize(WorldManager.Instance.SelectedWorld, ldata);
            level_slots.Add(slot);
        }
    }

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();

        // we register the WorldBuilder callbacks
        WorldBuilder.LazyInstance.RegisterCallbacks();
    }
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();

        // we unregister the WorldBuilder callbacks
        WorldBuilder.LazyInstance.RemoveCallbacks();

    }


    // EVENTS
    protected override void after_removed_from_stack()
    {
        GameManager.State = GameState.Paused;
    }
}