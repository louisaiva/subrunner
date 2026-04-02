using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Level : MonoBehaviour
{
    [Header("Data")]
    public LevelData data;
    public List<NavMeshData> loaded_navmeshes;

    // LOAD NAV MESHES
    public void LoadNavMeshesPath()
    {
        if (data == null || data.navmesh_data_paths == null) { return; }
        loaded_navmeshes = new List<NavMeshData>();
        foreach (string path in data.navmesh_data_paths)
        {
            NavMeshData navMeshData = Resources.Load<NavMeshData>(path);
            if (navMeshData == null) { Debug.LogWarning($"(Level) Could not load navmesh data at path '{path}' for level '{data.id}'"); continue; }
            
            loaded_navmeshes.Add(navMeshData);
        }
    }

    // LOAD UNLOAD
    public void Load()
    {
        // we load the navmeshes into the navmesh for this level
        NavMeshBuilder.Instance.LoadLevelNavMeshData(this.loaded_navmeshes);

        // for now we are a dummy we only tell the RoomSystem to
        // load all the rooms based on their data ids
        RoomEngine.Instance?.LoadRooms(data.rooms_ids.ToArray());

    }
    public void Unload()
    {
        // same shit
        RoomEngine.Instance?.UnloadRooms(data.rooms_ids.ToArray());
    }

    // SET STATIC DATA
    public void AddNavMeshPath(string path)
    {
        if (data == null || data.navmesh_data_paths == null) { return; }

        // we remove Assets/Resources/ and .asset from the path to get the resource path to load it later
        path = path.Replace("Assets/Resources/", "").Replace(".asset", "");

        if (data.navmesh_data_paths.Contains(path)) { return; }
        data.navmesh_data_paths.Add(path);

        // mark as dirty to save the data
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    public void ClearNavMeshPaths()
    {
        if (data == null || data.navmesh_data_paths == null) { return; }
        data.navmesh_data_paths.Clear();
        
        // mark as dirty to save the data
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    // GET STATIC DATA

    /// <summary>
    /// just as other GetStaticData() methods (ie Capable's one), this method
    /// is not meant to be run in a BUILD !!! IT WON T WORK because it does not
    /// update this.data . it creates a new data based from actual static
    /// variables states of the object. if run inside a build, it could overwrite
    /// some data such as tilebases_used paths which would break the save.
    /// </summary>
    /// <returns></returns>
    public LevelData GetStaticData()
    {
        LevelData new_data = new LevelData
        {
            // set base data things
            id = GetStaticID(),
            rooms_ids = get_static_rooms_ids(),
            navmesh_data_paths = data != null ? data.navmesh_data_paths : new List<string>()
        };

        return new_data;
    }
    public string GetStaticID()
    {
        string id = this.name;
        if (this.data == null) { return id; }
        if (string.IsNullOrEmpty(this.data.id)) { return id; }
        return this.data.id;
    }
    private List<string> get_static_rooms_ids()
    {
        // if (data == null) { return new List<string>(); }
        // if (data.rooms_ids != null && data.rooms_ids.Count > 0) { return data.rooms_ids; }

        // we go statically get the rooms ids from the children rooms
        Room[] rooms = GetStaticRooms();
        List<string> rooms_ids = new List<string>();
        foreach (Room room in rooms)
        {
            string room_id = room.GetStaticData().id;
            if (!string.IsNullOrEmpty(room_id))
            {
                rooms_ids.Add(room_id);
            }
        }

        // we apply it to the current data also
        data.rooms_ids = rooms_ids;
        return rooms_ids;
    }
    public Room[] GetStaticRooms() { return GetComponentsInChildren<Room>(includeInactive: true); }
    public Bounds GetStaticBounds()
    {
        // we get all the rooms in the children
        Room[] rooms = GetComponentsInChildren<Room>(includeInactive: true);
        if (rooms.Length == 0) { return new Bounds(); }

        // we get the colliders of all the rooms
        float min_x = float.MaxValue;
        float max_x = float.MinValue;
        float min_y = float.MaxValue;
        float max_y = float.MinValue;
        for (int i = 0; i < rooms.Length; i++)
        {
            PolygonCollider2D polygon = rooms[i].GetComponent<PolygonCollider2D>();
            if (polygon == null) { continue; }

            Bounds colliderBounds = polygon.bounds;
            min_x = Mathf.Min(min_x, colliderBounds.min.x);
            max_x = Mathf.Max(max_x, colliderBounds.max.x);
            min_y = Mathf.Min(min_y, colliderBounds.min.y);
            max_y = Mathf.Max(max_y, colliderBounds.max.y);

            // Debug.Log($"(Level) Room '{rooms[i].name}' bounds : {colliderBounds}, current level bounds : min_x={min_x}, max_x={max_x}, min_y={min_y}, max_y={max_y}");
        }
        return new Bounds(new Vector3((min_x + max_x) / 2, (min_y + max_y) / 2, 0), new Vector3(max_x - min_x, max_y - min_y, 0));
    }
    public Capable[] GetStaticCapables() { return GetComponentsInChildren<Capable>(includeInactive: true); }
}