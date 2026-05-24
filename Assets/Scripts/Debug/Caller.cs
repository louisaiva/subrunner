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
    public void SwitchTo(string ui_pool) => UI_Manager.Instance?.SwitchTo(ui_pool);
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


    // WORLD MANAGER
    public void OpenWorldFolder() => AppManager.OpenWorldsFolder();
    public async void UnloadWorld() { await WorldManager.Instance?.UnloadCurrentWorld(); }
    public void LoadWorld() { WorldManager.Instance?.LoadSelectedWorld(); }
    public void CreateWorld() => WorldManager.Instance?.CreateNewWorld();
    public void CreateLevel() => WorldManager.Instance?.CreateNewLevel();


    // LEVEL BUILDER
    public void ToggleWorldBuilder() => WorldBuilder.LevelBuilder?.gameObject.SetActive(!WorldBuilder.LevelBuilder.gameObject.activeSelf);

    // OBJECT PLACER
    public void PlaceObject() => WorldPlacer.LazyInstance.AskAndThenStartPlacingObject();

}