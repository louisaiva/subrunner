using System.Collections.Generic;
using UnityEngine;
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
    public bool hide_mask = true; // if true, will hide the mask tilemap in the level (useful for trying instantly the generated level)

    [Header("Logs")]
    public bool log_translations = false;

    private void Start()
    {
        // we subscribe to world generation end event
        WorldBuilder.StaticInstance.OnWorldBuilt += Translate;
    }

    public void Translate(BuiltWorldData built_world)
    {
        if (log_translations) { Debug.Log($"(LevelTranslator) translating world into level {level_name}"); }

        // get target level to save into
        Level level = find_target_level();
        List<Room> level_rooms = new List<Room>(level.GetStaticRooms());

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
        }
    }


    // apply tilemaps
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
            if (tilemap_type == "mask" && hide_mask)
            {
                TilemapRenderer tilemap_renderer = room_tilemap.GetComponent<TilemapRenderer>();
                if (tilemap_renderer != null) { tilemap_renderer.enabled = false; }
            }
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
}