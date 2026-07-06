using UnityEngine;
using System.Collections.Generic;

public class WorldDebug : MonoBehaviour, MultipleDebuggable
{
    // WORLD
    private string world_name = "wip...";
    private int levels_count = 0;
    private int rooms_count = 0;
    private int chunks_count = 0;
    private int capables_count = 0;
    // private int capacities_count = 0;

    // LEVEL
    private string current_level_name = "";
    private int current_level_rooms_count = 0;
    private int current_level_chunks_count = 0;
    private int current_level_capables_count = 0;
    // private int current_level_capacities_count = 0;

    // CHUNK
    private string current_chunk_name = "";
    private int current_chunk_capables_count = 0;
    // private int current_chunk_capacities_count = 0;

    // START
    private void Start()
    {
        DebugManager.Instance.AddDebuggable(this, "world");

        // register to world events
        LevelEngine.Instance.OnLevelChange += handle_level_change;
        ChunkEngine.Instance.OnPlayerChunkChange += handle_chunk_change;
        ChunkEngine.Instance.OnCapableAddedToChunk += handle_capable_added_or_remove_to_from_chunk;
        ChunkEngine.Instance.OnCapableRemovedFromChunk += handle_capable_added_or_remove_to_from_chunk;
    }

    // EVENTS HANDLERS
    private void handle_world_change(World world)
    {
        if (world == null) { return; }
        // todo : implement world data etc
    }
    private void handle_level_change(LevelData level)
    {
        if (level == null) { return; }
        current_level_name = level.id;
        current_level_rooms_count = level.TotalRoomsCount;
        current_level_chunks_count = level.TotalChunksCount;
        current_level_capables_count = level.TotalCapablesCount;
    }
    private void handle_chunk_change(ChunkData chunk)
    {
        if (chunk == null) { return; }
        current_chunk_name = chunk.id;
        current_chunk_capables_count = chunk.TotalCapablesCount;
    }
    private void handle_capable_added_or_remove_to_from_chunk(string capable_id, ChunkData chunk)
    {
        if (capable_id == null || chunk == null) { return; }

        // we refresh the level if this is the current level
        Level level = RoomEngine.Instance.GetLevelOfChunk(chunk.id);
        if (level != null && LevelEngine.Instance.CurrentLevelID == level.ID)
        {
            handle_level_change(LevelEngine.Instance.current_level.data);
        }

        // we refresh the room if this is the current room
        if (ChunkEngine.Instance.PlayerChunkData != null && ChunkEngine.Instance.PlayerChunkData.id == chunk.id)
        {
            handle_chunk_change(ChunkEngine.Instance.PlayerChunkData);
        }
    }

    // DEBUGGABLE
    public string GetDebugText()
    {
        return string.Join("\n", GetDebugLines());
    }
    public List<string> GetDebugLines()
    {
        // we return a list of strings, one line for total capables count, and one for each type of capable with its count
        List<string> lines = new List<string>
        {
            get_world_line(),
            get_level_line(),
            get_chunk_line()
        };
        return lines;
    }

    // lines generators
    private string get_world_line()
    {
        string line = "world : " + world_name + "\n";
        line += ">>> levels : " + levels_count + "\n";
        line += ">>> rooms : " + rooms_count + "\n";
        line += ">>> chunks : " + chunks_count + "\n";
        line += ">>> capables : " + capables_count + "\n";
        // line += ">>> capacities : " + capacities_count;
        return line;
    }
    private string get_level_line()
    {
        string line = "current level : " + current_level_name + "\n";
        line += ">>> rooms : " + current_level_rooms_count + "\n";
        line += ">>> chunks : " + current_level_chunks_count + "\n";
        line += ">>> capables : " + current_level_capables_count + "\n";
        // line += ">>> capacities : " + current_level_capacities_count;
        return line;
    }
    private string get_chunk_line()
    {
        string line = "current chunk : " + current_chunk_name + "\n";
        line += ">>> capables : " + current_chunk_capables_count + "\n";
        // line += ">>> capacities : " + current_chunk_capacities_count;
        return line;
    }
}

