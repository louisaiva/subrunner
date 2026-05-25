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
    public void ValidateStartPlacingObject(string template)
    {
        if (string.IsNullOrEmpty(template)) { log.Error("invalid template id"); return; }
        current_template = template;
        UI_Manager.Instance.SwitchTo("capable_placer");
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
    public void ActivateGrid()
    {
        Grid.enabled = true;
    }
    public void DeactivateGrid()
    {
        Grid.enabled = false;
    }
}

public enum WorldPlacerStatus
{
    NotWorking,
    PlacingObject
}