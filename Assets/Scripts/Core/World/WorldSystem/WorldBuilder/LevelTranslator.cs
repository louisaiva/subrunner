using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

/// <summary>
/// this class translates a world generated from the builder
/// into a level that can be used by "Level" and can be saved
/// as a real playable level. creates/override Level & Rooms
/// </summary>
public class LevelTranslator : MonoBehaviour
{
    public string level_name;
    public bool target_current_level = false; // if true, will override the level name to save into the current level (if we have one)

    [Header("Level creation")]
    public Level level_prefab;
    private Transform _level_parent;
    private Transform level_parent
    {
        get
        {
            if (_level_parent != null) { return _level_parent; }
        
            _level_parent = World.LazyInstance?.LevelParent;
            if (_level_parent == null)
            {
                GameObject level_parent_go = new GameObject("Levels");
                _level_parent.SetParent(transform);
                _level_parent = level_parent_go.transform;
            }
            return _level_parent;           
        }
    }

    [Header("Room creation")]
    public Room room_prefab;
    public bool add_roomgraph_neighbour_node = true;
    public RoomNodeEditor roomgraph_node_prefab;

    [Header("Doors & Lights")]
    public Door door_vertical_prefab;
    public Door door_horizontal_prefab;
    public Light2D light_prefab;

    [Header("Logs")]
    public bool log = false;
    public bool log_translate_extended = false;

    // EVENTS
    public System.Action<Level> OnLevelTranslated = delegate { };


    // low level level methods
    private Level find_target_level()
    {
        Level[] levels;
        if (target_current_level && World.LazyInstance != null)
        {
            // we have to have ONLY one enabled level in the scene to be able to target it, otherwise we exit with a warning
            levels = FindObjectsByType<Level>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (levels.Length != 1)
            {
                Debug.LogWarning("(LevelTranslator) found " + levels.Length + " enabled levels in the scene. Exiting. Need to be precisely 1 enabled level to target current level.");
                return null;
            }
            return levels[0];
        }

        // else we want to find the level with the right name.
        levels = level_parent.GetComponentsInChildren<Level>(includeInactive: true);
        foreach (Level level in levels)
        {
            if (level.ID == level_name) { return level; }
        }

        // else we have not found any level, we create a new one
        return create_level(level_name);
    }
    private Level create_level(string id)
    {
        // we instanciate a new level and assign the data to it
        Level new_level = Instantiate(level_prefab, level_parent);
        new_level.data = new LevelData()
        {
            id = id,
            rooms_ids = new List<string>()
        };
        new_level.name = id;
        return new_level;
    }

    // low level room methods
    private Room find_room(string id, List<Room> rooms, Level level)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].ID == id) { return rooms[i]; }
        }

        // else we found no room with the id, we create a new one
        return create_room(id, level);
    }
    private bool find_room(string id, List<Room> rooms, out Room found_room)
    {
        found_room = null;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].ID == id) { found_room = rooms[i]; return true; }
        }
        return false;
    }
    private Room create_room(string id, Level level)
    {
        // we instanciate a new room and assign the data to it
        Room new_room = Instantiate(room_prefab, level.transform);
        new_room.data = new RoomData() { id = id };
        new_room.name = id;

        // we add the room id to the level data
        level.GrabStaticRoom(id);
        return new_room;
    }


    // TRANSLATION
    public async void Translate(BuiltLevelData built_level)
    {
        string world_id = built_level.world;
        string level_id = built_level.level;

        if (log) { Debug.Log($"(LevelTranslator) translating schematics of level {level_id} (world : {world_id}) into an AIO Level ready to save"); }

        // get target level to save into
        Level level = SaveEngine.AIO_Loader.LoadAIO_Level(world_id, level_id);
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) AIO Level loaded for translation"); }

        // get the rooms
        List<Room> old_rooms = new List<Room>(level.GetStaticRooms());
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) gathered {old_rooms.Count} rooms"); }

        // get all capables
        List<Capable> old_capables = level.transform.Find("Capables").GetComponentsInChildren<Capable>(includeInactive: true).ToList();
        List<Door> existing_doors = old_capables.OfType<Door>().ToList();
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) gathered {old_capables.Count} capables and {existing_doors.Count} doors"); }

        // clear the placed doors
        Transform capables_parent = level.transform.Find("Capables");
        doors_placed.Clear();
        List<Capable> new_capables = new List<Capable>();

        // we find/create each room in the level and assign tilemaps, collider, doors, lights to them
        if (log) { Debug.Log($"(LevelTranslator) Creating rooms, doors and lights"); }
        List<Room> rooms = new List<Room>();
        foreach (WorldRoomVisualizer room_visu in built_level.Rooms)
        {
            if (!find_room(room_visu.name, old_rooms, out Room room))
            {
                room = create_room(room_visu.name, level);
                if (log_translate_extended) { Debug.Log($"(LevelTranslator) created room {room_visu.name}"); }
            }

            // we assign collider to the room
            room.RoomCollider.SetPath(0, room_visu.PolygonCollider.points);

            // we assign tilemaps to the room
            if (!built_level.Tilemaps.TryGetValue(room_visu.name, out Dictionary<string, Tilemap> tilemaps)) { Debug.LogWarning($"(LevelTranslator) no tilemaps found for room {room_visu.name}"); continue; }
            apply_tilemaps(room, tilemaps);

            // we assign the doors and lights to the room
            foreach (WorldDoorVisualizer door_visu in room_visu.Doors)
            {
                find_or_create_door(room, capables_parent, door_visu, ref existing_doors, ref new_capables);
            }
            find_or_create_light(room, room_visu.Lights);

            // if add_roomgraph_neighbour_node is true, we add a node for each neighbour of the room in the roomgraph
            // if (add_roomgraph_neighbour_node) { Instantiate(roomgraph_node_prefab, room.transform); }

            rooms.Add(room);
            if (log_translate_extended) { Debug.Log($"(LevelTranslator) room added: {room_visu.name}"); }
        }

        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Building neighbours & navmesh"); }
        // now we need to build the room graph neighbour nodes connections

        // now we can build the navmesh

        // now we can regenerate the ids for the capables & their capacities
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Regenerating IDs for capables and capacities"); }
        IDsGenerator.Instance.GenerateIDsOnlyForCapablesAndCapacities(old_capables, new_capables);


        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Deleting old capables"); }

        // here we can delete the old doors that were not reused and the rooms that were not reused
        foreach (Door door in existing_doors)
        {
            if (doors.ContainsValue(door)) { continue; }
            if (log_translate_extended) { Debug.Log($"(LevelTranslator) destroying door {door.name}"); }
            Destroy(door.gameObject);
        }
        foreach (Room old_room in old_rooms)
        {
            if (rooms.Contains(old_room)) { continue; }
            if (log_translate_extended) { Debug.Log($"(LevelTranslator) destroying room {old_room.name}"); }
            Destroy(old_room.gameObject);
        }

        // we can then make rooms grab their capables
        await System.Threading.Tasks.Task.Delay(300); // we delay a lil bit bcz the colliders were just created
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Making rooms grab capables"); }
        RoomEngine.MakeRoomsGrabCapables(rooms.ToArray(), only_capables: true);

        // and finally we make the level regrab all its rooms
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Making level grab static rooms"); }
        level.GrabStaticRooms();

        if (log) { Debug.Log($"(LevelTranslator) Level translated successfully"); }
        OnLevelTranslated?.Invoke(level);
    }
    public void Translate2(BuiltLevelData built_level)
    {
        string world_id = built_level.world;
        string level_id = built_level.level;

        if (log) { Debug.Log($"(LevelTranslator) translating schematics of level {level_id} (world : {world_id}) into AIO Level ready to save"); }

        // get target level to save into
        // Level level = find_target_level();
        Level level = SaveEngine.AIO_Loader.LoadAIO_Level(world_id, level_id);



        List<Room> level_rooms = new List<Room>(level.GetStaticRooms());
        // clear the placed doors
        doors_placed.Clear();

        // we create each room in the level and assign tilemaps to them
        List<Room> rooms = new List<Room>();
        foreach (WorldRoomVisualizer room_visu in built_level.Rooms)
        {
            Room room = find_room(room_visu.name, level_rooms, level);

            // we assign collider to the room
            room.RoomCollider.SetPath(0, room_visu.PolygonCollider.points);

            // we assign tilemaps to the room
            if (!built_level.Tilemaps.TryGetValue(room_visu.name, out Dictionary<string, Tilemap> tilemaps)) { Debug.LogWarning($"(LevelTranslator) no tilemaps found for room {room_visu.name}"); continue; }
            apply_tilemaps(room, tilemaps);

            // we assign the doors and lights to the room
            // create_or_apply_doors(room, room.transform.Find("Doors"), room_visu.Doors, ref existing_doors);
            find_or_create_light(room, room_visu.Lights);

            // if add_roomgraph_neighbour_node is true, we add a node for each neighbour of the room in the roomgraph
            if (add_roomgraph_neighbour_node) { Instantiate(roomgraph_node_prefab, room.transform); }
        }
    }


    // TILEMAPS 
    private void apply_tilemaps(Room room, Dictionary<string, Tilemap> tilemaps)
    {
        // we apply the tilemaps to the room
        foreach (KeyValuePair<string, Tilemap> entry in tilemaps)
        {
            string tilemap_type = entry.Key;
            Tilemap tilemap = entry.Value;

            // we find the corresponding tilemap in the room and assign the tiles to it
            Tilemap room_tilemap = room.GetStaticTilemap(tilemap_type);
            if (room_tilemap == null) { Debug.LogWarning($"(LevelTranslator) no tilemap of type {tilemap_type} found in room {room.name}"); continue; }
            copy_tilemap(tilemap, room_tilemap);

            // check if this is the mask tilemap and hide_mask == true, then we disable the tilemap renderer
            /* if (tilemap_type == "mask" && hide_mask)
            {
                TilemapRenderer tilemap_renderer = room_tilemap.GetComponent<TilemapRenderer>();
                if (tilemap_renderer != null) { tilemap_renderer.enabled = false; }
            } */
        }
    }
    private void copy_tilemap(Tilemap source, Tilemap target)
    {
        target.ClearAllTiles();

        // we copy the tiles from the source tilemap to the target tilemap
        TileBase tile;
        foreach (Vector3Int pos in source.cellBounds.allPositionsWithin)
        {
            tile = source.GetTile(pos);
            if (tile != null) { target.SetTile(pos, tile); }
        }
    }


    // DOORS & LIGHTS
    private List<WorldDoorVisualizer> doors_placed = new List<WorldDoorVisualizer>();
    private Dictionary<WorldDoorVisualizer, Door> doors = new Dictionary<WorldDoorVisualizer, Door>();
    
    /// <summary>
    /// returns true if we created a new door,
    /// false otherwise (we found an existing door and just assigned the room to it)
    /// </summary>
    /// <param name="room"></param>
    /// <param name="capables_parent"></param>
    /// <param name="door_visu"></param>
    /// <param name="existing_doors"></param>
    /// <param name="new_capables"></param>
    private bool find_or_create_door(Room room, Transform capables_parent, WorldDoorVisualizer door_visu, ref List<Door> existing_doors, ref List<Capable> new_capables)
    {
        Door door;
        if (doors_placed.Contains(door_visu))
        {
            // we set the room as the door's other room.
            door = this.doors[door_visu];
            if (string.IsNullOrEmpty(door.room1_id)) { door.room1_id = room.ID; }
            else if (string.IsNullOrEmpty(door.room2_id)) { door.room2_id = room.ID; }
            else { Debug.LogError($"(LevelTranslator) door {door.ID} already has 2 rooms assigned. Cannot assign room {room.ID} to it."); }
            return false;
        }

        // get the position 
        Vector2 world_pos = (door_visu.WorldPosition + door_visu.OtherWorldPosition) / 2f;
        if (door_visu.is_vertical) { world_pos.y -= 0.25f; }
        else { world_pos.y -= 0.5f; } // to adjust the door position a bit (because the door pivot is not centered)

        // check if we already have a door at this position
        door = existing_doors.FirstOrDefault(d => Vector2.Distance(d.transform.position, world_pos) < 0.1f);
        bool need_to_be_created = door == null;
        if (need_to_be_created)
        {
            // we create the door
            Door door_prefab = door_visu.is_vertical ? door_vertical_prefab : door_horizontal_prefab;
            door = Instantiate(door_prefab, capables_parent);
            door.name = $"{door_prefab.name}";

            // apply the position
            door.transform.position = world_pos;
        }

        // get a position to check where is the room located compared to door
        Vector2 world_position_in_first_room = door_visu.WorldPosition;
        if (door_visu.is_vertical) { world_position_in_first_room.y += 0.5f; }
        else { world_position_in_first_room.x += 0.5f; }

        // check if the position is inside the room, it means we are in the first room, else we are in the second room
        if (room.OverlapPoint(world_position_in_first_room)) { door.room1_id = room.ID; }
        else { door.room2_id = room.ID; }

        doors_placed.Add(door_visu);
        this.doors[door_visu] = door;

        // add the door as a capable
        new_capables.Add(door);
        return need_to_be_created;
    }
    private void find_or_create_light(Room room, List<WorldLightVisualizer> lights)
    {
        // find the parent
        Transform light_parent = room.LightsParent;

        // gather the existing Light2D in the room
        List<Light2D> existing_lights = light_parent.GetComponentsInChildren<Light2D>(includeInactive: true).ToList();
        foreach (WorldLightVisualizer light_visu in lights)
        {
            // check if we already have a light at this position
            Vector2 world_pos = light_visu.WorldPosition;
            Light2D light = existing_lights.FirstOrDefault(l => Vector2.Distance(l.transform.position, world_pos) < 0.1f);
            if (light != null) { continue; }

            // else we create the light
            light = Instantiate(light_prefab, light_parent);
            light.transform.position = world_pos;
        }
    }
}