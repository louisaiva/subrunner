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
    public void SwitchToHUD() => UI_Manager.Instance?.SwitchToHUD();
    public void SwitchTo(string ui_pool) => UI_Manager.Instance?.SwitchTo(ui_pool);
    public void QuitApp() => AppManager.Instance?.Exit();
    public void BackToTitleScreen() => SceneLoader.Instance?.GoBackToMainMenu();
    public void LoadGame() => SceneLoader.Instance?.LoadGame();
    public void UnloadWorld() => WorldManager.Instance?.UnloadCurrentWorld();
    public void LoadWorld() => WorldManager.Instance?.LoadSelectedWorld();
    public void CreateWorld() => WorldManager.Instance?.CreateNewWorld();
    public void OpenWorldFolder() => AppManager.OpenWorldsFolder();
    public void ToggleTimeScale() { Time.timeScale = Time.timeScale == 0 ? 1 : 0; }
    public void ToggleGlitches() => PostProcessManager.Instance?.ToggleGlitches();
    public void SetGlitchMode(string timestamp_name) => PostProcessManager.Instance?.SetGlitchMode(timestamp_name);
    public void ChangeUIMode(string mode) => UI_Manager.Instance?.ChangeUIMode(mode);
    public void ToggleWorldBuilder() => LevelBuilder.StaticInstance?.gameObject.SetActive(!LevelBuilder.StaticInstance.gameObject.activeSelf);
}