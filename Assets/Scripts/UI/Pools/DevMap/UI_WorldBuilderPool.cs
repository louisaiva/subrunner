using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_WorldBuilderPool : UI_SlottablePool, Descriptable
{

    [Header("References")]
    [SerializeField] private UI_WorldSlot world_slot;
    [SerializeField] private GameObject level_slot_prefab;
    [SerializeField] private RectTransform levels_container;
    private List<UI_LevelSlot> level_slots = new List<UI_LevelSlot>();
    public string Name => world_slot.Name;
    public string Description => world_slot.Description;



    protected override void before_showing()
    {
        world_slot.Initialize(WorldManager.Instance.SelectedWorldData);

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
    /* protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();

        // we unload the world
        yield return WorldManager.Instance.UnloadCurrentWorld();
    }
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();

        // we load the world
        WorldManager.Instance.LoadSelectedWorld();
    } */
}