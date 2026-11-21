using System;
using SingularityGroup.HotReload;
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
    public void LoadGame() => SceneLoader.Instance?.LoadGame();
    public void SaveWorld() => SaveEngine.SaveDynamicWorld();
    public void StartTimer() => World.Instance?.Timer.StartTimer();
    public void StopTimer() => World.Instance?.Timer.StopTimer();
    public void OpenURL(string url) => Application.OpenURL(url);
    public void BackToTitleScreen() => SceneLoader.Instance?.GoBackToMainMenu();
    public void ConfirmAndGoBackToTitleScreen()
    {
        string world_id = WorldManager.StaticSelectedWorld;
        if (string.IsNullOrEmpty(world_id)) { BackToTitleScreen(); return; }

        // check if we saved in the last 10 seconds or if we are on a sofa
        WorldSaveData save = SaveEngine.GetWorldSave(world_id);
        float not_saved_delay = save.world.GetDelaySinceLastSave(out Color highlight);
        if (not_saved_delay < 5) { BackToTitleScreen(); return; }

        // check if we are on a sofa
        if (Controller.Capable == null) { BackToTitleScreen(); return; }
        else if (!Controller.Capable.TryGetCapacity(out SitCapacity sitter)) { BackToTitleScreen(); return; }
        else if (sitter.CurrentSofa != null) { BackToTitleScreen(); return; }

        // we show a confirmation popup because we did not save in a long time
        UI_Manager.Instance.OpenQuestionPopup(
            title:          "Warning : Game Not Saved",

            question:       $"You are trying to quit the game but you did not save for <b>{not_saved_delay.ToHMS("00").AddColor(highlight)}</b>.\n\n" +
                            "Progression since last save will be lost".Grey() +
                            "\n\n<b>Confirm quitting game ?</b>\n".AddColor(not_saved_delay > 60 ? highlight : Color.white),

            on_validate:    BackToTitleScreen,
            outline_color:  highlight,
            desc_name:      "Tooltip",
            desc:           "Sit on a sofa to save the game"/* .Grey() */
        );
    }

    // STEAM BUTTON
    private bool next_we_open_steam = true;
    private string steam_url = "steam://store/4898010";
    private string steam_url_2 = "https://store.steampowered.com/app/4898010/SUBRUNNER/";
    public void OpenSteamStore()
    {
        if (next_we_open_steam) { Application.OpenURL(steam_url); }
        else { Application.OpenURL(steam_url_2); }
        next_we_open_steam = !next_we_open_steam;
    }

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