using System.Collections;
using Unity.VisualScripting;
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
    private Vector2 map_half_ui; // the radius of the map in world units, used to clamp the zoom and offset
    private float min_zoom = 0.05f;
    private float max_zoom = 0.75f;

    // static data
    public static Vector2 world_center; // the offset to apply to all visuals to center the map on the screen when global_offset is (0,0)
    public static Vector2 global_offset = Vector2.zero; // the offset that allows us to move the map around, applied on top of world_offset
    public static float global_zoom = 1f; // the zoom that allows us to zoom the map in and out
    public static float world_to_ui_zoom => instance.world_to_ui_scale;
    public static float MinZoom => instance.min_zoom;
    public static float MaxZoom => instance.max_zoom;

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
        world_center = room_visualizer.CalculateWorldCenter(out Vector2 extents);
        map_half_ui = extents * (int)world_to_ui_scale ;

        // register to level loaded to update the visuals when we load a level
        LevelEngine.Instance.OnLevelLoaded += handle_level_loaded;

        // then create visuals for the rooms
        room_visualizer.CreateVisuals();

        // then call creation of CapableVisualizerManager to create visuals for the capables, now that we have the right canvas size
        capable_visualizer.CreateVisuals();
    }
    private void OnDestroy()
    {
        // unregister to level loaded
        if (LevelEngine.Instance != null) { LevelEngine.Instance.OnLevelLoaded -= handle_level_loaded; }
    }

    // LEVEL LOADED HANDLER
    private void handle_level_loaded(Level level)
    {
        // when we load a level, we update the visuals for the rooms and capables
        room_visualizer.ClearVisuals();
        room_visualizer.CreateVisuals(level.data.id);

        capable_visualizer.ClearVisuals();
        capable_visualizer.CreateVisuals(level.data.id);
    }

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        // we register to UI_Navigator OnSlotHovered
        UI_Navigator.Instance.OnSlotHoverEnter += handle_slot_hovered;

        yield return base.enable_coroutine();
    }
    protected override IEnumerator disable_coroutine()
    {
        yield return base.disable_coroutine();

        // we unregister to UI_Navigator OnSlotHovered
        if (UI_Navigator.Instance != null) { UI_Navigator.Instance.OnSlotHoverEnter -= handle_slot_hovered; }
    }

    // STATIC METHODS
    public static Vector2 GetUIPositionFromWorldPosition(Vector2 world_position)
    {
        // Vector2 canvas_position = UI_Manager.WorldToCanvasLocal(world_position, offsetter, canvas, Camera.main);
        Vector2 final_position = (world_position - world_center) * world_to_ui_zoom;
        // Vector2 final_position = (canvas_position + global_offset) * global_zoom;
        if (LogWorldToCanvas) { Debug.Log($"(UI_DevMap - WorldPosToCanvas) World position: {world_position}"/* => Canvas position: {canvas_position} */+$" => Final position: {final_position}     - ({world_to_ui_zoom})"); }
        return final_position;
    }


    // MOUSE INPUTS
    public void OnDrag(Vector2 mouse_delta)
    {
        // when we drag the mouse, we update the global offset based on the mouse position delta
        global_offset += mouse_delta / global_zoom; // we divide by the zoom to have a consistent movement speed when zoomed in or out

        Vector2 view_half_ui = new Vector2(canvas.pixelRect.width, canvas.pixelRect.height) / 2f;
        view_half_ui /= global_zoom; // we also divide the view size by the zoom to have a consistent clamping when zoomed in or out

        float max_offset_x = map_half_ui.x + view_half_ui.x;
        float max_offset_y = map_half_ui.y + view_half_ui.y;

        global_offset.x = Mathf.Clamp(global_offset.x, -max_offset_x, max_offset_x); // clamp the offset to avoid losing the map when dragging too much
        global_offset.y = Mathf.Clamp(global_offset.y, -max_offset_y, max_offset_y);

        // apply the offset
        offsetter.anchoredPosition = global_offset;
    }
    public void OnScroll(float scroll_delta)
    {
        // when we scroll, we update the global zoom based on the scroll delta
        global_zoom += scroll_delta * 0.1f;
        global_zoom = Mathf.Clamp(global_zoom, min_zoom, max_zoom);

        // apply the zoom
        scaler.localScale = Vector3.one * global_zoom;

        // resize all icons
        capable_visualizer.ResizeAllIcons();
    }

    // GAMEPAD NAVIGATION
    private void handle_slot_hovered(UI_Slot slot)
    {
        // verify that the navigator is made with gamepad
        if (!InputManager.Instance.isUsingGamepad()) { return; }

        // when we hover a slot, if it's a capable visu, we center the map on it
        if (slot is not UI_CapableVisualizer cap_visu) { return; }
        global_offset = -cap_visu.GetComponent<RectTransform>().anchoredPosition;
        offsetter.anchoredPosition = global_offset;
    }
}