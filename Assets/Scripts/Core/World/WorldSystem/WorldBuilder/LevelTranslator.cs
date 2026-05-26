using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    // SUB SYSTEMS
    private AutoChunker _auto_chunker;
    public AutoChunker AutoChunker
    {
        get
        {
            if (_auto_chunker == null) { _auto_chunker = GetComponentInChildren<AutoChunker>(includeInactive: true); }
            return _auto_chunker;
        }
    }

    private AutoNeighbourer _auto_neighbourer;
    public AutoNeighbourer AutoNeighbourer
    {
        get
        {
            if (_auto_neighbourer == null) { _auto_neighbourer = GetComponentInChildren<AutoNeighbourer>(includeInactive: true); }
            return _auto_neighbourer;
        }
    }


    [Header("Room creation")]
    public Room room_prefab;
    public Chunk chunk_prefab;

    [Header("Doors & Lights")]
    public Door door_vertical_prefab;
    public Door door_horizontal_prefab;
    public Light2D light_prefab;

    [Header("Logs")]
    public bool log = false;
    public bool log_translate_extended = false;
    public Loggable<LevelTranslator> log_chunking;
    public Loggable<LevelTranslator> log_chunk_grabbing;

    // EVENTS
    public System.Action<Level> OnLevelTranslated = delegate { };

    


    ///
    //
    /// MAIN ENTRY POINT : TRANSLATION
    //
    ///


    // TRANSLATION
    public async Task Translate(BuiltLevelData built_level)
    {
        string world_id = built_level.world;
        string level_id = built_level.level;

        if (log) { Debug.Log($"(LevelTranslator) translating schematics of level {level_id} (world : {world_id}) into an AIO Level ready to save"); }



        //
        /// 1 - LOAD TARGET EXISTING LEVEL
        //

        // get target level to save into
        Level level = await SaveEngine.AIO_Loader.LoadAIO_Level_NoWorldLoaded(world_id, level_id);
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) AIO Level loaded for translation"); }

        //
        /// 2 - GATHER DATA FROM EXISTING LEVEL + instanciate data structures
        //

        // get the rooms & chunks
        List<Room> old_rooms = new List<Room>(level.GetStaticRooms());
        List<Chunk> old_chunks = new List<Chunk>(level.GetStaticChunks());
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) gathered {old_rooms.Count} rooms & {old_chunks.Count} chunks"); }

        // get all capables
        List<Capable> old_capables = level.transform.Find("Capables").GetComponentsInChildren<Capable>(includeInactive: true).ToList();
        List<Door> existing_doors = old_capables.OfType<Door>().ToList();
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) gathered {old_capables.Count} capables and {existing_doors.Count} doors"); }

        // clear the placed doors
        Transform capables_parent = level.transform.Find("Capables");
        doors_placed.Clear();
        List<Capable> new_capables = new List<Capable>();



        //
        /// 2.5 - AUTO CHUNK
        //
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Auto chunking big rooms"); }
        built_level = await AutoChunker.ChunkRooms(built_level);
        if (log_chunking.Verbose >= Verbosity.Specific)
        {
            string log_chunking_details = $"chunks : {built_level.Chunks.Count}\n";
            foreach (WorldChunkVisualizer chunk in built_level.Chunks)
            {
                log_chunking_details += $"  - {chunk.name} with path of {chunk.Path.Length} points\n";
            }
            log_chunking_details += $"\n\n rooms : {built_level.RoomChunks.Count}\n";
            foreach (KeyValuePair<string, List<string>> entry in built_level.RoomChunks)
            {
                string room_name = entry.Key;
                List<string> chunk_names = entry.Value;
                log_chunking_details += $"  - {room_name} with chunks : {string.Join(", ", chunk_names)}\n";
            }
            log_chunking?.LogSpecific(log_chunking_details);
        }

        // wait while the editor is paused
        /* await Task.Delay(1000*3);
        if (Application.isEditor)
        {
            #if UNITY_EDITOR
            while (UnityEditor.EditorApplication.isPaused)
            {
                await Task.Delay(100);
            }
            #endif
        } */



        //
        /// 3 - CREATE MISSING CHUNKS, ASSIGN TILEMAPS, COLLIDERS, DOORS, LIGHTS
        //

        // we find/create each room in the level and assign tilemaps, collider, doors, lights to them
        List<Chunk> chunks = translate_chunks(built_level, old_chunks, level, capables_parent, ref existing_doors, ref new_capables);
        List<Room> rooms = translate_rooms(built_level, old_rooms, level, ref chunks);


        //
        /// 4 - AUTO NEIGHBOURING
        //



        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Building neighbours"); }
        // now we need to build the room graph neighbour nodes connections
        AutoNeighbourer.TraceRoomGraphNeighbours(ref chunks);




        //
        /// 5 - GENERATE CAPABLES IDS, CLEAN OBSOLETE THINGS (DOORS, ROOMS)
        //

        // now we can regenerate the ids for the capables & their capacities
        // we do this before cleaning bcz the specific IDsGenerator method we use here only generates IDs for things that need it.
        // (useless to generate IDs for capables that will die ://)
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Regenerating IDs for capables and capacities"); }
        IDsGenerator.Instance.GenerateIDsOnlyForCapablesAndCapacities(old_capables, new_capables);

        clean_obsolete_chunks_and_doors(existing_doors, old_chunks, chunks);
        // await Task.Delay(1000); // we delay a lil bit bcz the colliders were just created


        await Task.Yield();
        await Task.Yield();
        await Task.Yield();
        await Task.Yield(); // yeahaaa
        Physics2D.SyncTransforms();




        //
        /// 6 - BAKE LEVEL NAVMESH, MAKE ROOMS GRAB THEIR CAPABLES, MAKE LEVEL GRAB ITS ROOMS
        //

        // we can then make rooms grab their capables
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Making rooms grab capables"); }
        ChunkEngine.MakeChunksGrabCapables(chunks.ToArray(), only_capables: false, log_chunk_grabbing);

        // wait while the editor is paused
        /* await Task.Delay(1000*3);
        if (Application.isEditor)
        {
            #if UNITY_EDITOR
            while (UnityEditor.EditorApplication.isPaused)
            {
                await Task.Delay(100);
            }
            #endif
        } */

        // and finally we make the level regrab all its rooms
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Making level grab static rooms"); }
        level.GrabStaticRooms(built_level.RoomChunks.Keys.ToList());

        // now we can build the navmesh
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Building navmesh for level"); }
        LevelEngine.LazyInstance.NavBaker.BuildLevelNavMesh(level, force_rebuild: true);


        //
        /// 7 - WE are DONE !!!
        //

        if (log) { Debug.Log($"(LevelTranslator) Level translated successfully"); }
        OnLevelTranslated?.Invoke(level);
    }




    ///
    //
    /// MEDIUM LEVEL METHODS
    //
    ///

    private List<Chunk> translate_chunks(BuiltLevelData built_level, List<Chunk> old_chunks, Level level, Transform capables_parent, ref List<Door> existing_doors, ref List<Capable> new_capables)
    {
        if (log) { Debug.Log($"(LevelTranslator) Creating chunks, doors and lights"); }
        List<Chunk> chunks = new List<Chunk>();
        foreach (WorldChunkVisualizer chunk_visu in built_level.Chunks)
        {
            if (!find_chunk(chunk_visu.name, old_chunks, out Chunk chunk))
            {
                chunk = create_chunk(chunk_visu.name, level);
                if (log_translate_extended) { Debug.Log($"(LevelTranslator) created room {chunk_visu.name}"); }
            }

            // we assign collider to the chunk
            chunk.ChunkCollider.SetPath(0, chunk_visu.Path);


            // we assign the doors and lights to the chunk
            foreach (WorldDoorVisualizer door_visu in chunk_visu.Doors)
            {
                find_or_create_door(capables_parent, door_visu, ref existing_doors, ref new_capables);
            }
            find_or_create_light(chunk, chunk_visu.Lights);

            // if add_chunkgraph_neighbour_node is true, we add a node for each neighbour of the chunk in the chunkgraph
            // if (add_chunkgraph_neighbour_node) { Instantiate(chunkgraph_node_prefab, chunk.transform); }

            chunks.Add(chunk);
            if (log_translate_extended) { Debug.Log($"(LevelTranslator) chunk added: {chunk_visu.name}"); }
        }
        return chunks;
    }
    private List<Room> translate_rooms(BuiltLevelData built_level, List<Room> old_rooms, Level level, ref List<Chunk> chunks)
    {

        if (log) { Debug.Log($"(LevelTranslator) Creating rooms and applying tilemaps"); }
        List<Room> rooms = new List<Room>();
        foreach (KeyValuePair<string, List<string>> entry in built_level.RoomChunks)
        {
            string room_name = entry.Key;
            List<string> chunk_names = entry.Value;
            if (!find_room(room_name, old_rooms, out Room room))
            {
                room = create_room(room_name, level, chunk_names);
                if (log_translate_extended) { Debug.Log($"(LevelTranslator) created room {room_name}"); }
            }

            // we find the chunks that belong to the room
            foreach (string chunk_name in chunk_names)
            {
                Chunk chunk = chunks.FirstOrDefault(c => c.ID == chunk_name);
                if (chunk == null) { Debug.LogError($"(LevelTranslator) no chunk found with name {chunk_name} for room {room_name}"); continue; }
                chunk.data.room_id = room.ID;
                chunk.transform.SetParent(room.transform.Find("Chunks"));
            }

            // we apply the tilemaps
            if (!built_level.Tilemaps.TryGetValue(room_name, out Dictionary<string, Tilemap> tilemaps)) { Debug.LogWarning($"(LevelTranslator) no tilemaps found for room {room_name}"); continue; }
            apply_tilemaps(room, tilemaps);

            rooms.Add(room);
            if (log_translate_extended) { Debug.Log($"(LevelTranslator) room added: {room_name}"); }
        }

        return rooms;
    }
    private void clean_obsolete_chunks_and_doors(List<Door> existing_doors, List<Chunk> old_chunks, List<Chunk> chunks)
    {
        if (log_translate_extended) { Debug.Log($"(LevelTranslator) Cleaning old things (doors, rooms)"); }

        // here we can delete the old doors that were not reused and the rooms that were not reused
        foreach (Door door in existing_doors)
        {
            if (doors.ContainsValue(door)) { continue; }
            if (log_translate_extended) { Debug.Log($"(LevelTranslator) destroying door {door.name}"); }
            Destroy(door.gameObject);
        }
        foreach (Chunk old_room in old_chunks)
        {
            if (chunks.Contains(old_room)) { continue; }
            if (log_translate_extended) { Debug.Log($"(LevelTranslator) destroying room {old_room.name}"); }
            Destroy(old_room.gameObject);
        }
    }




    ///
    //
    /// LOW LEVEL METHODS FOR TRANSLATION
    //
    ///

    // ROOMS
    private bool find_room(string id, List<Room> rooms, out Room found_room)
    {
        found_room = null;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].ID == id) { found_room = rooms[i]; return true; }
        }
        return false;
    }
    private Room create_room(string id, Level level, List<string> chunk_names)
    {
        // we instanciate a new room and assign the data to it
        Room new_room = Instantiate(room_prefab, level.transform);
        new_room.data = new RoomData()
        {
            id = id,
            chunks_ids = chunk_names,
        };
        new_room.name = id;

        // we add the room id to the level data
        level.GrabStaticRoom(id);
        return new_room;
    }

    // CHUNKS
    private bool find_chunk(string id, List<Chunk> rooms, out Chunk found_room)
    {
        found_room = null;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].ID == id) { found_room = rooms[i]; return true; }
        }
        return false;
    }
    private Chunk create_chunk(string id, Level level)
    {
        // we instanciate a new room and assign the data to it
        Chunk new_room = Instantiate(chunk_prefab, level.transform);
        new_room.data = new ChunkData() { id = id };
        new_room.name = id;

        // we add the room id to the level data
        level.GrabStaticRoom(id);
        return new_room;
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
    /// <param name="chunk"></param>
    /// <param name="capables_parent"></param>
    /// <param name="door_visu"></param>
    /// <param name="existing_doors"></param>
    /// <param name="new_capables"></param>
    private bool find_or_create_door(Transform capables_parent, WorldDoorVisualizer door_visu, ref List<Door> existing_doors, ref List<Capable> new_capables)
    {
        Door door;
        if (doors_placed.Contains(door_visu)) { return false; }

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
            door.name = $"{door_prefab.name}-{doors_placed.Count}";

            // apply the position
            door.transform.position = world_pos;
        }

        // updates the door chunks rooms to make sure it's correct
        door.room1_id = door_visu.room1_id;
        door.room2_id = door_visu.room2_id;

        doors_placed.Add(door_visu);
        this.doors[door_visu] = door;

        // add the door as a capable
        new_capables.Add(door);
        return need_to_be_created;
    }
    private void find_or_create_light(Chunk room, List<WorldLightVisualizer> lights)
    {
        // find the parent
        Transform light_parent = room.LightsParent;

        // gather the existing Light2D in the room
        List<Light2D> existing_lights = light_parent.GetComponentsInChildren<Light2D>(includeInactive: true).ToList();
        List<Light2D> kept_lights = new List<Light2D>();
        foreach (WorldLightVisualizer light_visu in lights)
        {
            // check if we already have a light at this position
            Vector2 world_pos = light_visu.WorldPosition;
            Light2D light = existing_lights.FirstOrDefault(l => Vector2.Distance(l.transform.position, world_pos) < 0.1f);
            if (light != null) { kept_lights.Add(light); continue; }

            // else we create the light
            light = Instantiate(light_prefab, light_parent);
            light.transform.position = world_pos;
            kept_lights.Add(light);
        }

        // we destroy the lights that were not kept
        foreach (Light2D existing_light in existing_lights)
        {
            if (kept_lights.Contains(existing_light)) { continue; }
            if (log_translate_extended) { Debug.Log($"(LevelTranslator) destroying light {existing_light.name}"); }
            Destroy(existing_light.gameObject);
        }
    }
}