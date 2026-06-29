#pragma warning disable 1998

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

public class RoomEngine : BSOD_System<RoomEngine>
{

    // SUB SYSTEMS
    private TilemapEngine _tilemap_engine;
    public TilemapEngine TilemapEngine
    {
        get
        {
            if (_tilemap_engine == null) { _tilemap_engine = GetComponentInChildren<TilemapEngine>(includeInactive: true); }
            return _tilemap_engine;
        }
    }

    private TileBaseBank _tilebase_bank;
    public TileBaseBank TileBaseBank
    {
        get
        {
            if (_tilebase_bank == null) { _tilebase_bank = GetComponentInChildren<TileBaseBank>(includeInactive: true); }
            return _tilebase_bank;
        }
    }


    private static DoorEngine _door_engine;
    public static DoorEngine DoorEngine
    {
        get
        {
            if (_door_engine == null) { _door_engine = LazyInstance.GetComponentInChildren<DoorEngine>(includeInactive: true); }
            return _door_engine;
        }
    }

    private Dictionary<string, RoomData> rooms_data = new Dictionary<string, RoomData>();

    [Header("Logs")]
    [SerializeField] private Loggable<RoomEngine> log_data;
    [SerializeField] private Loggable<RoomEngine> log_loading;
    [SerializeField] private Loggable<RoomEngine> log;


    ///
    //
    /// 1. AWAKE & DATA LOADING
    //
    ///


    // LOAD / UNLOAD WORLD DATA
    public override async Task LoadWorldData(string world_id, bool log)
    {
        loadRoomsData(world_id);

        // register to chunkengine events
        ChunkEngine.LazyInstance.OnCapableChangedChunk += OnCapableChangedChunk;

        if (log) { Debug.Log($"(RoomEngine) ROOM ENGINE SUCCESSFULLY LOADED : {world_id}"); }
    }
    public override async Task UnloadWorldData(bool log)
    {
        ChunkEngine.LazyInstance.OnCapableChangedChunk -= OnCapableChangedChunk;

        // clear sub systems caches
        TilemapEngine.ClearTilemaps(log);
        await DoorEngine.UnloadWorldData(log);

        if (log) { Debug.Log($"(RoomEngine) ROOM ENGINE SUCCESSFULLY UNLOADED"); }
    }


    // LOAD / UNLOAD DATA
    protected void loadRoomsData(string world_id)
    {
        // we empty the rooms_data and runtime ids
        rooms_data = new Dictionary<string, RoomData>();
        string log_rooms_details = "\n\n";

        // we load all the json files in the data path and convert them to RoomData objects
        List<RoomData> rooms = World.Save.rooms;
        // string[] files = AppManager.LoadJsonsFromWorldFolder(world_id, "rooms");
        foreach (RoomData data in rooms)
        {
            rooms_data.Add(data.id, data);
            log_rooms_details += data.GetDetails() + "\n";
        }

        log?.Log("(RoomEngine) ROOMS DATA LOADED : " + rooms_data.Count + log_rooms_details);
    }




    ///
    //
    /// MAIN LOGIC
    //
    ///

    // LOAD UNLOAD ROOMS
    public void LoadRoom(string room_id)
    {
        // no need for loading the room LMAO
        // i mean we only load the chunks inside
        RoomData room_data = GetRoomDataFromID(room_id);
        if (room_data == null) { log_loading?.Warning($"(RoomEngine) RoomData for room '{room_id}' not found when trying to load it"); return; }

        ChunkEngine.Instance?.LoadChunks(room_data.chunks_ids.ToArray());
        log_loading?.Log($"(RoomEngine) Room '{room_id}' loaded.");
    }
    public void UnloadRoom(string room_id)
    {
        // no need for unloading the room LMAO
        // i mean we only unload the chunks inside
        RoomData room_data = GetRoomDataFromID(room_id);
        if (room_data == null) { log_loading?.Warning($"(RoomEngine) RoomData for room '{room_id}' not found when trying to unload it"); return; }

        ChunkEngine.Instance?.UnloadChunks(room_data.chunks_ids.ToArray());
        log_loading?.Log($"(RoomEngine) Room '{room_id}' unloaded.");
    }
    public void LoadRooms(string[] room_ids)
    {
        foreach (string room_id in room_ids) { LoadRoom(room_id); }
    }
    public void UnloadRooms(string[] room_ids)
    {
        foreach (string room_id in room_ids) { UnloadRoom(room_id); }
    }
    public bool IsRoomLoaded(RoomData room_data)
    {
        return ChunkEngine.Instance.IsAnyChunkLoaded(room_data.chunks_ids);
    }

    // SHOW HIDE ROOMS TILEMAPS
    public void ShowTilemaps(RoomData room_data) => TilemapEngine.ShowTilemaps(room_data);
    public void HideTilemaps(RoomData room_data) => TilemapEngine.HideTilemaps(room_data.id);


    ///
    //
    /// CAPABLE TRANSFER CALLBACKS
    //
    ///
    public Action<string, RoomData> OnCapableChangedRoom = delegate {};
    private void OnCapableChangedChunk(string capable_id, ChunkData from, ChunkData to)
    {
        if (to == null) { return; }
        if (from != null && from.room_id == to.room_id) { return; } // stays in same room
        string to_room = to.room_id;
        if (!rooms_data.TryGetValue(to_room, out RoomData room)) { return; }        
        OnCapableChangedRoom?.Invoke(capable_id, room);
    }


    ///
    //
    /// GETTERS
    //
    ///
    public RoomData GetRoomDataFromID(string room_id)
    {
        log_data?.Log($"Getting data of room '{room_id}'...");
        if (rooms_data.TryGetValue(room_id, out RoomData room_data))
        {
            log_data?.Log($"RoomData '{room_id}' was found :D");
            return room_data;
        }
        log_data?.Warning($"RoomData '{room_id}' not found :///     (returning null)");
        return null;
    }
    public List<RoomData> GetRoomsDataFromIDs(List<string> room_ids)
    {
        List<RoomData> rooms_data = new List<RoomData>();
        foreach (KeyValuePair<string, RoomData> kvp in this.rooms_data)
        {
            if (!room_ids.Contains(kvp.Key)) { continue; }
            rooms_data.Add(kvp.Value);
        }
        return rooms_data;
    }
    public List<RoomData> LoadRoomsData(string world_id, List<string> room_ids)
    {
        if (string.IsNullOrEmpty(world_id)) { Debug.LogWarning($"(RoomEngine - LoadRoomsData) Invalid world_id : '{world_id}'"); return new List<RoomData>(); }

        List<RoomData> rooms_data = new List<RoomData>();
        WorldSaveData save = SaveEngine.GetWorldSave(world_id);

        foreach (RoomData data in save.rooms)
        {
            if (!room_ids.Contains(data.id)) { continue; }
            rooms_data.Add(data);
        }
        return rooms_data;
    }
    public Level GetLevelOfChunk(string chunk_id)
    {
        foreach (var kvp in rooms_data)
        {
            RoomData room_data = kvp.Value;
            if (room_data.chunks_ids.Contains(chunk_id))
            {
                return LevelEngine.Instance.GetLevelOfRoom(room_data.id);
            }
        }
        log_data?.Warning("[GetLevelOfChunk] No RoomData with chunk id '" + chunk_id + "' was found. Returning null.");
        return null;
    }
    public List<CapableData> GetCapablesDataInRoom(RoomData room_data)
    {
        // gather the chunk data
        List<ChunkData> chunks_data = ChunkEngine.Instance.GetChunksDataFromIDs(room_data.chunks_ids);

        List<CapableData> capables_data = new List<CapableData>();
        foreach (ChunkData chunk_data in chunks_data)
        {
            capables_data.AddRange(CapableEngine.Instance.GetCapablesDataFromIDs(chunk_data.capables_ids.Concat(chunk_data.movables_ids).ToList()));
        }
        return capables_data;
    }
    public bool TryGetCapableRoom(CapableData cdata, out RoomData room)
    {
        if (TryGetCapableRoom(cdata.id, out room)) { return true; }
        room = GetRoomAtPositionInLevel(cdata.Position);
        return room != null;
    }
    public bool TryGetCapableRoom(string capable_id, out RoomData room)
    {
        room = null;
        if (!ChunkEngine.Instance.TryGetCapableChunk(capable_id, out ChunkData chunk)) { return false; }

        // find the room of the chunk
        if (string.IsNullOrEmpty(chunk.room_id))
        {
            Debug.LogError($"(RoomEngine - TryGetCapableRoom) '{capable_id}' is in chunk '{chunk.id}' but this chunk has no room or empty !");
            return false;
        }
        
        room = GetRoomDataFromID(chunk.room_id);
        if (room == null) { return false; }
        return true;
    }
    public RoomData GetRoomAtPositionInLevel(Vector2 position, string level_id = null)
    {
        ChunkData chunk = ChunkEngine.Instance.GetChunkAtPositionInLevel(position, level_id);
        if (chunk == null) { return null; }
        return GetRoomDataFromID(chunk.room_id);
    }

    // STATIC GETTERS
    public static List<RoomData> LoadWorldRoomsData(string world_id, List<string> room_ids)
    {
        if (string.IsNullOrEmpty(world_id)) { return new List<RoomData>(); }
        WorldSaveData save = SaveEngine.GetWorldSave(world_id);

        List<RoomData> levels_data = new List<RoomData>();

        // we load all the json files in the data path and convert them to RoomData objects
        // string[] files = AppManager.LoadSpecificJsonsFromWorldFolder(world_id, "rooms", room_ids);
        foreach (RoomData data in save.rooms)
        {
            if (!room_ids.Contains(data.id)) { continue; }
            levels_data.Add(data);
        }
        return levels_data;
    }
}