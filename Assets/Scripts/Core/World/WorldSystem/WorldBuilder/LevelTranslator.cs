using System.Collections.Generic;
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
            if (_level_parent == null)
            {
                _level_parent = World.StaticInstance?.LevelParent;
                if (_level_parent == null)
                {
                    GameObject level_parent_go = new GameObject("Levels");
                    _level_parent.SetParent(transform);
                    _level_parent = level_parent_go.transform;
                }
            }
            return _level_parent;
        }
    }

    [Header("Room creation")]
    public Room room_prefab;
    // public bool hide_mask = true; // if true, will hide the mask tilemap in the level (useful for trying instantly the generated level)
    public bool add_roomgraph_neighbour_node = true;
    public RoomNodeEditor roomgraph_node_prefab;

    [Header("Doors & Lights")]
    public Door door_vertical_prefab;
    public Door door_horizontal_prefab;
    public Light2D light_prefab;

    [Header("Logs")]
    public bool log_translations = false;

    private void Start()
    {
        // we subscribe to world generation end event
        LevelBuilder.StaticInstance.OnWorldBuilt += Translate;
    }

    // low level level methods
    private Level find_target_level()
    {
        Level[] levels;
        if (target_current_level && World.StaticInstance != null)
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
    public void Translate(BuiltLevelData built_world)
    {
        if (log_translations) { Debug.Log($"(LevelTranslator) translating world into level {level_name}"); }

        // get target level to save into
        Level level = find_target_level();
        List<Room> level_rooms = new List<Room>(level.GetStaticRooms());

        // clear the placed doors
        doors_placed.Clear();

        // we create each room in the level and assign tilemaps to them
        List<Room> rooms = new List<Room>();
        foreach (WorldRoomVisualizer room_visu in built_world.Rooms)
        {
            Room room = find_room(room_visu.name, level_rooms, level);

            // we assign collider to the room
            room.RoomCollider.SetPath(0, room_visu.PolygonCollider.points);

            // we assign tilemaps to the room
            if (!built_world.Tilemaps.TryGetValue(room_visu.name, out Dictionary<string, Tilemap> tilemaps)) { Debug.LogWarning($"(LevelTranslator) no tilemaps found for room {room_visu.name}"); continue; }
            apply_tilemaps(room, tilemaps);

            // we assign the doors and lights to the room
            apply_doors(room, room_visu.Doors);
            apply_lights(room, room_visu.Lights);

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
    private void apply_doors(Room room, List<WorldDoorVisualizer> doors)
    {
        // find the parent
        Transform door_parent = room.transform.Find("Doors");

        foreach (WorldDoorVisualizer door_visu in doors)
        {
            if (doors_placed.Contains(door_visu))
            {
                // we set the room as the door's other room.
                Door door = this.doors[door_visu];
                if (string.IsNullOrEmpty(door.room1_id)) { door.room1_id = room.ID; }
                else if (string.IsNullOrEmpty(door.room2_id)) { door.room2_id = room.ID; }
                else { Debug.LogError($"(LevelTranslator) door {door.ID} already has 2 rooms assigned. Cannot assign room {room.ID} to it."); }
                continue;
            }

            Door door_prefab = door_visu.is_vertical ? door_vertical_prefab : door_horizontal_prefab;
            Door new_door = Instantiate(door_prefab, door_parent);
            
            // get the position and assign it to the door
            Vector2 world_pos = (door_visu.WorldPosition + door_visu.OtherWorldPosition) / 2f;
            if (door_visu.is_vertical) { world_pos.y -= 0.25f; }
            else { world_pos.y -= 0.5f; } // to adjust the door position a bit (because the door pivot is not centered)
            new_door.transform.position = world_pos;

            // assign this room as first room of the door.
            Vector2 world_position_in_first_room = door_visu.WorldPosition;
            if (door_visu.is_vertical) { world_position_in_first_room.y += 0.5f; }
            else { world_position_in_first_room.x += 0.5f; }

            // check if the position is inside the room, it means we are in the first room, else we are in the second room
            if (room.OverlapPoint(world_position_in_first_room)) { new_door.room1_id = room.ID; }
            else { new_door.room2_id = room.ID; }

            doors_placed.Add(door_visu);
            this.doors[door_visu] = new_door;
        }
    }
    private void apply_lights(Room room, List<WorldLightVisualizer> lights)
    {
        // find the parent
        Transform light_parent = room.transform.Find("Lights");
        foreach (WorldLightVisualizer light_visu in lights)
        {
            Light2D new_light = Instantiate(light_prefab, light_parent);
            new_light.transform.position = light_visu.WorldPosition;
        }
    }
}