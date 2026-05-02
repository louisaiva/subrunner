using System.Collections.Generic;
using UnityEngine;

public class WorldBuilderCaller : MonoBehaviour
{
    private LevelBuilder _worldBuilder;
    private LevelBuilder worldBuilder
    {
        get
        {
            if (_worldBuilder == null) { _worldBuilder = LevelBuilder.StaticInstance; }
            return _worldBuilder;
        }
    }

    // open world builder after unload world popup
    public void OpenPopupThenWorldBuilder()
    {
        // open the world builder
        UI_Manager.Instance.OpenQuestionPopup(
            "Open World Builder",
            "World needs to be unloaded to proceed.\n(will be saved first)\n\n<b>Unload World ?</b>",
            unload_world_then_open_world_builder
        );
    }
    private async void unload_world_then_open_world_builder()
    {
        await WorldManager.StaticInstance.UnloadCurrentWorld();
        while (UI_Manager.Instance.IsInTransition) { await System.Threading.Tasks.Task.Yield(); }
        UI_Manager.Instance.StackPool("dev_world_builder");
    }
    public void OpenPopupThenUnstackWorldBuilder()
    {
        // open the world builder
        UI_Manager.Instance.OpenQuestionPopup(
            "Close World Builder",
            "World needs to be reloaded to proceed.\n\n<b>Reload World ?</b>",
            load_world_then_close_world_builder
        );
    }
    private async void load_world_then_close_world_builder()
    {
        await WorldManager.StaticInstance.LoadSelectedWorld();
        while (UI_Manager.Instance.IsInTransition) { await System.Threading.Tasks.Task.Yield(); }
        UI_Manager.Instance.UnstackPool("dev_world_builder");
    }



    // tools
    public void SelectTool(string tool_type) => worldBuilder.SelectTool(tool_type);


    // general builders
    public void BuildWorld() => worldBuilder.Build();
    public void EraseAll() => worldBuilder.Erase();
    public void ClearTilemaps() => worldBuilder.ClearTilemaps();
    public void SaveData() => worldBuilder.SaveCurrentLevelSchematic();

    // specifics builders
    public void Build(string builder) => worldBuilder.Build(builder);
}