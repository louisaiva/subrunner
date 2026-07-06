
using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using My.Common.Scripts.Editor;
using UnityEditor;
#endif

[ExecuteInEditMode, RequireComponent(typeof(SpriteRenderer))]
public class RoomNodeEditor : MonoBehaviour
{

    [Header("References")]
    private Chunk _room;
    public Chunk Room
    {
        get
        {
            if (_room == null && transform.parent != null) { _room = transform.parent.GetComponent<Chunk>(); }
            return _room;
        }
    }
    private SpriteRenderer _sprite_renderer;
    public SpriteRenderer SpriteRenderer
    {
        get
        {
            if (_sprite_renderer == null) { _sprite_renderer = GetComponent<SpriteRenderer>(); }
            return _sprite_renderer;
        }
    }

    [Header("Hover Parameters")]
    [SerializeField] private bool Hovered = false;
    [SerializeField] private Color normal_color = Color.white;
    private static float normal_scale = 1f;
    private static float hover_scale = 2f;
    private static float hover_distance = 3f;
    
    [Header("Selection Parameters")]
    [SerializeField] private bool Selected = false;
    [SerializeField] private Color selecting_color = Color.skyBlue;
    [SerializeField] private Color selected_color = Color.cyan;
    private static float selection_delay = 1f;
    private float time_since_last_hover = 0f;
    private static RoomNodeEditor current_selected_node = null;

    [Header("Links")]
    public List<RoomLinkEditor> Links = new List<RoomLinkEditor>();
    [SerializeField] private GameObject link_prefab;

    [Header("Update Parameters")]
    private static float update_rate = 0.5f;
    private float time_since_last_update = 0f;

#if UNITY_EDITOR
    // ON ENABLE / DISABLE
    private void OnEnable()
    {
        if (!Application.isEditor) { Destroy(this); return; }
        SceneView.duringSceneGui += OnScene;

        // show / hide the node according to the current state of the room graph visibility
        if (IsRoomGraphVisible) { Show(); }
        else { Hide(); }
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnScene;
    }
    

    // SHOW HIDE
    public void Show()
    {
        SpriteRenderer.enabled = true;
        UnSelect();
        Unhover();
    }
    public void Hide()
    {
        SpriteRenderer.enabled = false;
        UnSelect();
        Unhover();
    }

    // UPDATE
    private void OnScene(SceneView scene_view)
    {
        // if (Application.isPlaying) { return; }
        if (!IsRoomGraphVisible) { return; }
        if (Room == null) { return; }

        Event e = Event.current;

        // we check mouse hover in scene view
        if (!Selected)
        {
            Vector2 mouse_position = MouseWorldSceneWindow.CurrentMouseWorldPositionInSceneView;
            if (mouse_position != Vector2.zero &&
                Vector2.Distance(transform.position, mouse_position) <= hover_distance) { Hover(); }
            else { Unhover(); }

            // update the selection
            UpdateSelection();
        }
        else
        {
            // if we click while being selected, we unselect ourselves
            if (e.type == EventType.MouseDown) { UnSelect(); }
        }

        // we update the node at fixed rate
        time_since_last_update += Time.unscaledDeltaTime;
        if (time_since_last_update < update_rate) { return; }
        time_since_last_update = 0f;
        UpdateNodePosition();
    }
    private void OnTransformParentChanged() { _room = null; }

    // UPDATE NODE
    public Action<RoomNodeEditor> OnNodeUpdated;
    private void UpdateNodePosition()
    {
        // we set our position to the center of the room
        Bounds room_bounds = Room.GetStaticBounds();
        transform.position = room_bounds.center;
        OnNodeUpdated?.Invoke(this);
    }


    // HOVER / UNHOVER
    private void Hover()
    {
        if (Hovered) { return; }
        transform.localScale = Vector3.one * hover_scale;
        Hovered = true;
    }
    private void Unhover()
    {
        if (!Hovered) { return; }
        SpriteRenderer.color = normal_color;
        transform.localScale = Vector3.one * normal_scale;
        Hovered = false;
    }

    // SELECTION
    private void UpdateSelection()
    {
        if (!Hovered)
        {
            time_since_last_hover = 0f;
            return;
        }

        time_since_last_hover += Time.unscaledDeltaTime;

        // update selection color
        SpriteRenderer.color = Color.Lerp(normal_color, selecting_color, time_since_last_hover / selection_delay);
        if (time_since_last_hover < selection_delay) { return; }
        
        // we select the node !
        SpriteRenderer.color = selected_color;
        Selected = true;
        if (current_selected_node == null)
        {
            current_selected_node = this;
            return;
        }

        // we check if we already have a selected node
        if (current_selected_node == this) { return; }

        // we check if we already have a link between the two nodes we destroy it
        if (HasLinkWith(current_selected_node)) { DestroyLink(current_selected_node); }

        // else we create a link between the two nodes
        else { CreateLink(current_selected_node); }
        

        current_selected_node.UnSelect();
        UnSelect();
    }
    private void UnSelect()
    {
        Selected = false;
        SpriteRenderer.color = normal_color;
        if (current_selected_node == this) { current_selected_node = null; }
    }


    // CREATE LINK
    private void CreateLink(RoomNodeEditor node_a)
    {
        Chunk room_a = node_a.Room;
        Chunk room_b = this.Room;
        if (room_a == null || room_b == null) { return; }

        // we create a link object on a new sibling gameobject
        GameObject link_object = Instantiate(link_prefab, Vector3.zero, Quaternion.identity, transform.parent);
        RoomLinkEditor link_editor = link_object.GetComponent<RoomLinkEditor>();
        link_editor.Init(node_a, this);
        Debug.Log($"(RoomGraph) Created link between '{room_a.name}' and '{room_b.name}'");

        // no need for updating lists of links in the nodes, as it is called from Link.Init
    }
    private void DestroyLink(RoomNodeEditor other_node)
    {
        Chunk room_a = other_node.Room;
        Chunk room_b = this.Room;
        if (room_a == null || room_b == null) { return; }
        
        RoomLinkEditor link = GetLinkWith(other_node);
        if (link == null) { return; }
        DestroyImmediate(link.gameObject);
        Debug.Log($"(RoomGraph) Destroyed link between '{room_a.name}' and '{room_b.name}'");

        // no need for updating lists of links in the nodes, as it is called from Link.OnDestroy
    }

    // GETTERS
    public bool HasLinkWith(RoomNodeEditor other_node)
    {
        return GetLinkWith(other_node) != null;
    }
    public RoomLinkEditor GetLinkWith(RoomNodeEditor other_node)
    {
        foreach (RoomLinkEditor link in Links)
        {
            if ((link.node_a == this && link.node_b == other_node) || (link.node_b == this && link.node_a == other_node))
            {
                return link;
            }
        }
        return null;
    }


    // ROOMGRAPH TOOLS SETTINGS
    public const string RoomGraphVisibleMenuItemPath = "Tools/subrunner/Toggle Room Graph Scene Editor";
    public static bool IsRoomGraphVisible = false;
    [MenuItem(RoomGraphVisibleMenuItemPath)]
    public static void ToggleRoomGraph()
    {
        RoomNodeEditor[] nodes = FindObjectsByType<RoomNodeEditor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        RoomLinkEditor[] links = FindObjectsByType<RoomLinkEditor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (IsRoomGraphVisible)
        {
            // we hide all the nodes and links in the scene
            foreach (RoomNodeEditor node in nodes) { node.Hide(); }
            foreach (RoomLinkEditor link in links) { link.Hide(); }
            IsRoomGraphVisible = false;
        }
        else
        {
            // we show all the nodes and links in the scene
            foreach (RoomNodeEditor node in nodes) { node.Show(); }
            foreach (RoomLinkEditor link in links) { link.Show(); }
            IsRoomGraphVisible = true;
        }
        Menu.SetChecked(RoomGraphVisibleMenuItemPath, IsRoomGraphVisible);
    }

    [MenuItem("Tools/subrunner/Regenerate Room Neighbours from Room Graph")]
    public static void RegenerateNeighbours()
    {
        RoomLinkEditor[] links = FindObjectsByType<RoomLinkEditor>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // we gather all the concerned rooms and we clear their neighbors list
        List<Chunk> concerned_rooms = new List<Chunk>();
        foreach (RoomLinkEditor link in links)
        {
            Chunk room_a = link.node_a.Room;
            Chunk room_b = link.node_b.Room;
            if (room_a != null && !concerned_rooms.Contains(room_a)) { concerned_rooms.Add(room_a); }
            if (room_b != null && !concerned_rooms.Contains(room_b)) { concerned_rooms.Add(room_b); }
        }
        foreach (Chunk room in concerned_rooms)
        {
            room.ClearStaticNeighbors();
        }
        
        // we go through all the links and we make them connect the rooms
        foreach (RoomLinkEditor link in links) { link.ConnectRooms();}
    }

    [MenuItem("Tools/subrunner/Clear Room Links")]
    public static void ClearRoomLinks()
    {
        // get active level transform
        Transform active_level_transform = null;
        Level[] levels = FindObjectsByType<Level>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (levels.Length == 0) { Debug.LogError("No level found in the scene !"); return; }
        else if (levels.Length > 1) { Debug.LogError("Multiple levels found in the scene !"); return; }
        else { active_level_transform = levels[0].transform; }
        
        List<RoomLinkEditor> links = new List<RoomLinkEditor>(active_level_transform.GetComponentsInChildren<RoomLinkEditor>(includeInactive: true));

        // we gather all the concerned rooms and we clear their neighbors list
        List<Chunk> concerned_rooms = new List<Chunk>();
        foreach (RoomLinkEditor link in links)
        {
            Chunk room_a = link.node_a?.Room;
            Chunk room_b = link.node_b?.Room;
            if (room_a != null && !concerned_rooms.Contains(room_a)) { concerned_rooms.Add(room_a); }
            if (room_b != null && !concerned_rooms.Contains(room_b)) { concerned_rooms.Add(room_b); }
        }
        foreach (Chunk room in concerned_rooms)
        {
            room.ClearStaticNeighbors();
        }

        // we destroy all the links
        while (links.Count > 0)
        {
            DestroyImmediate(links[0].gameObject);
            links.RemoveAt(0);
        }
    }
    #endif
}