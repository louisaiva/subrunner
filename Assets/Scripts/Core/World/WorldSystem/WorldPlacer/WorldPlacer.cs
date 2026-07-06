using Unity.VisualScripting;
using UnityEngine;

public class WorldPlacer : MonoBehaviour
{
    // SINGLETOn
    private static WorldPlacer _instance;
    public static WorldPlacer LazyInstance
    {
        get
        {
            if (_instance != null) { return _instance; }
            _instance = FindFirstObjectByType<WorldPlacer>(FindObjectsInactive.Include);
            if (_instance == null) { Debug.LogError("(WorldPlacer) no WorldPlacer in scene !!"); }
            return _instance;
        }
    }

    // SUB SYSTEMS
    private ObjectPlacer _placer;
    public ObjectPlacer Placer
    {
        get
        {
            if (_placer != null) { return _placer; }
            _placer = GetComponentInChildren<ObjectPlacer>(includeInactive: true);
            if (_placer == null) { Debug.LogError("(WorldPlacer) no ObjectPlacer found in children"); }
            return _placer;
        }
    }
    private Grid grid;
    public Grid Grid
    {
        get
        {
            if (grid != null) { return grid; }
            grid = GetComponentInChildren<Grid>(includeInactive: true);
            if (grid == null) { Debug.LogError("(WorldPlacer) no Grid found in children"); }
            return grid;
        }
    }


    public WorldPlacerStatus Status = WorldPlacerStatus.NotWorking;


    [Header("Logs")]
    [SerializeField] private Loggable<WorldPlacer> log;



    // ENTRY POINTS
    public void AskAndThenStartPlacingObject()
    {
        // show the text popup to enter capable template id
        UI_Manager.Instance.OpenInputPopup("place capable", "enter capable template id", ValidateStartPlacingObject);
    }
    public async void ValidateStartPlacingObject(string template)
    {
        if (string.IsNullOrEmpty(template)) { log.Error("invalid template id"); return; }
        current_template = template;

        // wait until the end of the frame to avoid issues with the popup being closed and the placer being enabled at the same time
        await System.Threading.Tasks.Task.Yield();
        while (UI_Manager.Instance.IsInTransition) { await System.Threading.Tasks.Task.Yield(); }
        UI_Manager.Instance.StackPool("capable_placer");
    }
    private string current_template = null;
    public void Enable()
    {
        // check if template is valid or not
        if (string.IsNullOrEmpty(current_template)) { log.Error("invalid template id"); return; }

        // enable the placer
        Status = WorldPlacerStatus.PlacingObject;
        gameObject.SetActive(true);
        Placer.StartPlacingCapable(current_template);
        log.Log("Enabled WorldPlacer with template : " + current_template);
    }
    public void Disable()
    {
        gameObject.SetActive(false);
        Status = WorldPlacerStatus.NotWorking;
        Placer.CancelCapablePlacement();
        log.Log("Disabled WorldPlacer");
    }

    // ACTIVATE / DEACTIVATE GRID
    public void SetGridSetting(Setting setting)
    {
        log.LogSpecific($"setting {setting.Name} changed, new value : {setting.Value}, we set grid enabled to {setting.Value > 0.5f}");
        Grid.enabled = setting.Value > 0.5f;
    }
    private void OnDestroy()
    {
        SettingsManager.Instance.UnregisterCallback("object_placer_magnetism", SetGridSetting);
    }
}

public enum WorldPlacerStatus
{
    NotWorking,
    PlacingObject
}