using System.Collections;
using UnityEngine;

public class UI_DevMap : UI_SlottablePool
{

    // static refs
    private static Canvas _canvas;
    public static Canvas canvas
    {
        get
        {
            if (_canvas == null) { _canvas = UI_Manager.Instance.GetComponent<Canvas>(); }
            return _canvas;
        }
    }
    private static UI_DevMap _instance;
    private static UI_DevMap instance
    {
        get
        {
            if (_instance == null) { _instance = UI_Manager.Instance.GetPool<UI_DevMap>(); }
            return _instance;
        }
    }
    private static RectTransform _scaler;
    public static RectTransform scaler
    {
        get
        {
            if (_scaler == null) { _scaler = instance.transform.Find("scaler").GetComponent<RectTransform>(); }
            return _scaler;
        }
    }
    private static RectTransform _offsetter;
    public static RectTransform offsetter
    {
        get
        {
            if (_offsetter == null) { _offsetter = scaler.Find("offsetter").GetComponent<RectTransform>(); }
            return _offsetter;
        }
    }

    
    // dynamic refs
    private RoomVisualizer _room_visualizer;
    private RoomVisualizer room_visualizer
    {
        get
        {
            if (_room_visualizer == null) { _room_visualizer = GetComponentInChildren<RoomVisualizer>(includeInactive: true); }
            return _room_visualizer;
        }
    }
    private CapableVisualizerManager _capable_visualizer;
    private CapableVisualizerManager capable_visualizer
    {
        get
        {
            if (_capable_visualizer == null) { _capable_visualizer = GetComponentInChildren<CapableVisualizerManager>(includeInactive: true); }
            return _capable_visualizer;
        }
    }
    private float world_to_ui_scale = 200f;

    // static data
    public static Vector2 world_center; // the offset to apply to all visuals to center the map on the screen when global_offset is (0,0)
    public static Vector2 global_offset = Vector2.zero; // the offset that allows us to move the map around, applied on top of world_offset
    public static float global_zoom = 1f; // the zoom that allows us to zoom the map in and out
    public static float world_to_ui_zoom => instance.world_to_ui_scale;

    [Header("Logs")]
    [SerializeField] private bool log_world_to_canvas = false;
    private static bool LogWorldToCanvas => instance.log_world_to_canvas;

    // AWAKE
    protected void Start()
    {
        // get base zoom & offset from the scaler and offsetter
        global_zoom = scaler.localScale.x;
        global_offset = offsetter.anchoredPosition;

        // then call room visualizer to calculate bounds
        world_center = room_visualizer.CalculateWorldCenter();

        // then create visuals for the rooms
        room_visualizer.CreateVisuals();

        // then call creation of CapableVisualizerManager to create visuals for the capables, now that we have the right canvas size
        capable_visualizer.CreateVisuals();
    }

    // STATIC METHODS
    public static Vector2 GetUIPositionFromWorldPosition(Vector2 world_position)
    {
        // Vector2 canvas_position = UI_Manager.WorldToCanvasLocal(world_position, offsetter, canvas, Camera.main);
        Vector2 canvas_position = (world_position - world_center) * world_to_ui_zoom;
        Vector2 final_position = (canvas_position + global_offset) * global_zoom;
        if (LogWorldToCanvas) { Debug.Log($"(UI_DevMap - WorldPosToCanvas) World position: {world_position} => Canvas position: {canvas_position} => Final position: {final_position}     - ({world_to_ui_zoom})"); }
        return final_position;
    }
}