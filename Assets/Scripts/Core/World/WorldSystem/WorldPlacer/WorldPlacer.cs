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

    public WorldPlacerStatus Status = WorldPlacerStatus.NotWorking;


    // ENTRY POINTS
    public void AskAndThenStartPlacingObject()
    {
        // show the text popup to enter capable template id
        UI_Manager.Instance.OpenInputPopup("place capable", "enter capable template id", StartPlacingObject);
    }
    public void StartPlacingObject(string template)
    {
        // check if template is valid or not
        if (string.IsNullOrEmpty(template)) { return; }

        // enable the placer
        Status = WorldPlacerStatus.PlacingObject;
        gameObject.SetActive(true);
        Placer.StartPlacingCapable(template);

        // show the ui_pool
        UI_Manager.Instance.SwitchTo("capable_placer");
        // todo
    }
    public void StopPlacingObject()
    {
        // todo switch ui_pool
        gameObject.SetActive(false);
        Status = WorldPlacerStatus.NotWorking;

        UI_Manager.Instance.UnstackPool("capable_placer");
    }
}

public enum WorldPlacerStatus
{
    NotWorking,
    PlacingObject
}