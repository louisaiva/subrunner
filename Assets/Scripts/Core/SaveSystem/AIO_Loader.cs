using System.Collections.Generic;
using UnityEngine;

public class AIO_Loader : MonoBehaviour
{
    
    [Header("Prefabs & Parents")]
    [SerializeField] private Transform level_parent;
    [SerializeField] private Level level_prefab;
    [SerializeField] private Room room_prefab;

    [Header("RTO Loaded AIO Levels")]
    private Dictionary<string, Level> loaded_levels = new Dictionary<string, Level>(); // key is level_id, value is the loaded level

    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool hide_no_level_warning = false;

    public Level LoadAIO_Level(string world_id, string level_id)
    {
        if (log) { Debug.Log($"(AIO_Loader) Loading level '{level_id}' for world '{world_id}'"); }

        // CLEAR OLD DATA
        ClearCache();

        // grab the level data from the world data
        LevelData level_data = LevelEngine.LoadWorldLevelData(world_id, level_id);
        if (level_data == null)
        {
            if (!hide_no_level_warning) { Debug.LogWarning($"(AIO_Loader) No level data found for level '{level_id}' in world '{world_id}'"); }
            return null;
        }

        // load the level
        Level aio_level = load_level(level_data);

        // load the rooms
        List<Room> rooms = load_rooms(world_id, level_data.rooms_ids, aio_level.transform);
        if (rooms.Count == 0) { Debug.LogWarning($"(AIO_Loader) No rooms loaded for level '{level_id}' in world '{world_id}'"); }

        return aio_level;
    }

    // low level level loading
    private Level load_level(LevelData data)
    {
        // we instanciate a new level and assign the data to it
        Level new_level = Instantiate(level_prefab, level_parent);
        new_level.data = data;
        new_level.gameObject.name = data.id;
        if (log) { Debug.Log($"(AIO_Loader) Level '{new_level.ID}' loaded"); }
        loaded_levels[data.id] = new_level;
        return new_level;
    }

    // low level rooms loading
    private List<Room> load_rooms(string world_id, List<string> room_ids, Transform rooms_parent)
    {
        // grab the room data from the world data
        List<Room> rooms = new List<Room>();
        List<RoomData> rooms_data = RoomEngine.LoadRoomsData(world_id, room_ids);
        foreach (RoomData data in rooms_data)
        {
            // load the room
            Room room = load_room(data, rooms_parent);
            if (room != null) { rooms.Add(room); }
        }

        return rooms;
    }
    private Room load_room(RoomData data, Transform parent)
    {
        // we instanciate a new room and assign the data to it
        Room new_room = Instantiate(room_prefab, parent);

        // load the data
        new_room.data = data;
        new_room.gameObject.name = data.id;
        new_room.transform.position = data.position;

        // load the colliders in the composite collider
        new_room.RoomCollider.SetPath(0, data.collider_points.ToArray());
        new_room.RoomCollider.enabled = true;

        // build the tilemaps
        RoomEngine.Instance.TilemapEngine.BuildTilemapsForAIO_Room(new_room);

        // load the lights
        RoomEngine.Instance.LightsEngine.LoadLights(data.lights_data, data.id, new_room.LightsParent);

        return new_room;
    }

    // CLEAR CACHE
    public void ClearCache()
    {
        // we clear the loaded levels
        foreach (Level level in loaded_levels.Values)
        {
            Destroy(level.gameObject);
        }
        loaded_levels.Clear();

        // we clear the subsystems
        RoomEngine.Instance.TilemapEngine.ClearTilemaps(log: true);
        RoomEngine.Instance.LightsEngine.ClearLights(log: true);
    }
}