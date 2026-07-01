using UnityEngine;
/// <summary>
/// this class is a helper class to call some things
/// it is mostly for calling singleton, sometimes
/// it is more useful to add this component to an
/// object and go through it instead of calling
/// directly the singleton. especially useful
/// for when using UnityEvents, since we need
/// to plug the singleton in the UnityEvent but sometimes
/// the singleton gets destroyed over scene ://
/// </summary>
public class Caller : MonoBehaviour
{
    // UI MANAGER
    public void SwitchToHUD() => UI_Manager.Instance?.SwitchToHUD();
    public void SwitchForceToHUD() => UI_Manager.Instance?.SwitchToHUD(force: true);
    public void SwitchTo(string ui_pool) => UI_Manager.Instance?.SwitchTo(ui_pool);
    public void SwitchForceTo(string ui_pool) => UI_Manager.Instance?.SwitchTo(ui_pool, force:true);
    public void StackPool(string ui_pool) => UI_Manager.Instance?.StackPool(ui_pool);
    public void UnstackCurrentPool() => UI_Manager.Instance?.UnstackCurrentPool();
    public void CancelPool() => UI_Manager.Instance?.CancelCurrentPool();
    public void ChangeUIMode(string mode) => UI_Manager.Instance?.ChangeUIMode(mode);


    // TWEENS / POST PROCESS
    public void ToggleTimeScale() { Time.timeScale = Time.timeScale == 0 ? 1 : 0; }
    public void ToggleGlitches() => PostProcessManager.Instance?.ToggleGlitches();
    public void SetGlitchMode(string timestamp_name) => PostProcessManager.Instance?.SetGlitchMode(timestamp_name);



    // APP MANAGER
    public void QuitApp() => AppManager.Instance?.Exit();
    public void BackToTitleScreen() => SceneLoader.Instance?.GoBackToMainMenu();
    public void LoadGame() => SceneLoader.Instance?.LoadGame();
    public void SaveWorld() => SaveEngine.SaveDynamicWorld();

    // PLAY BUTTON
    public void Play()
    {
        // if we have no world, we create a new one and launch it immediately
        if (WorldManager.Instance.ExistingWorldsCount == 0)
        {
            WorldManager.Instance.CreateNewWorldWithName(auto_load: true); // will choose default template, won't ask anything
            return;
        }

        // else if we have at least 1 world, we go to the world selection screen
        SwitchTo("world_selector");
    }



    // WORLD MANAGER
    public void OpenWorldFolder() => AppManager.OpenWorldsFolder();
    public async void UnloadWorld() { await WorldManager.Instance?.UnloadCurrentWorld(); }
    public void LoadWorld() { WorldManager.Instance?.LoadSelectedWorld(); }
    public void CreateWorld() => WorldManager.Instance?.CreateNewWorld(); // will ask a name for this world
    public void CreateLevel() => WorldManager.Instance?.CreateNewLevel();


    // LEVEL BUILDER
    public void ToggleWorldBuilder() => WorldBuilder.LevelBuilder?.gameObject.SetActive(!WorldBuilder.LevelBuilder.gameObject.activeSelf);

    // OBJECT PLACER
    public void PlaceObject() => WorldPlacer.LazyInstance.AskAndThenStartPlacingObject();


    // SOFA
    public void ExitSofa()
    {
        if (Controller.Capable == null) { return; }
        if (!Controller.Capable.TryGetCapacity(out SitCapacity sitter)) { return; }
        if (sitter.CurrentSofa == null) { return; }
        sitter.ExitSofa();
    }

    // SETTINGS
    public void EnableSetting(string setting_name) => SettingsManager.Instance?.SetSetting(setting_name, 1f);
    public void DisableSetting(string setting_name) => SettingsManager.Instance?.SetSetting(setting_name, 0f);

}