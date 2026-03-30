using UnityEngine;
using System.Collections.Generic;

public class WorldDebug : MonoBehaviour, MultipleDebuggable
{
    // WORLD
    private string world_name = "wip...";
    private int levels_count = 0;
    private int rooms_count = 0;
    private int capables_count = 0;
    private int capacities_count = 0;

    // LEVEL
    private string current_level_name = "";
    private int current_level_rooms_count = 0;
    private int current_level_capables_count = 0;
    private int current_level_capacities_count = 0;

    // ROOM
    private string current_room_name = "";
    private int current_room_capables_count = 0;
    private int current_room_capacities_count = 0;

    // START
    private void Start()
    {
        DebugManager.Instance.AddDebuggable(this, "world");

        // register to world events
        LevelEngine.Instance.OnLevelChange += handle_level_change;
        RoomEngine.Instance.OnRoomChange += handle_room_change;
        RoomEngine.Instance.OnCapableAddedToRoom += handle_capable_added_or_remove_to_from_room;
        RoomEngine.Instance.OnCapableRemovedFromRoom += handle_capable_added_or_remove_to_from_room;
    }

    // EVENTS HANDLERS
    private void handle_world_change(World2 world)
    {
        if (world == null) { return; }
        // todo : implement world data etc
    }
    private void handle_level_change(LevelData level)
    {
        if (level == null) { return; }
        current_level_name = level.id;
        current_level_rooms_count = level.TotalRoomsCount;
        current_level_capables_count = level.TotalCapablesCount;
        // current_level_capacities_count = level.TotalCapacitiesCount;
    }
    private void handle_room_change(RoomData room)
    {
        if (room == null) { return; }
        current_room_name = room.id;
        current_room_capables_count = room.TotalCapablesCount;
        // current_room_capacities_count = room.TotalCapacitiesCount;
    }
    private void handle_capable_added_or_remove_to_from_room(string capable_id, RoomData room)
    {
        if (capable_id == null || room == null) { return; }

        // we refresh the level if this is the current level
        if (LevelEngine.Instance.CurrentLevelID == LevelEngine.Instance.GetLevelOfRoom(room.id).data.id)
        {
            handle_level_change(LevelEngine.Instance.current_level.data);
        }

        // we refresh the room if this is the current room
        if (RoomEngine.Instance.main_room_data != null && RoomEngine.Instance.main_room_data.id == room.id)
        {
            handle_room_change(RoomEngine.Instance.main_room_data);
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
            get_room_line()
        };
        return lines;
    }

    // lines generators
    private string get_world_line()
    {
        string line = "world : " + world_name + "\n";
        line += ">>> levels : " + levels_count + "\n";
        line += ">>> rooms : " + rooms_count + "\n";
        line += ">>> capables : " + capables_count + "\n";
        // line += ">>> capacities : " + capacities_count;
        return line;
    }
    private string get_level_line()
    {
        string line = "current level : " + current_level_name + "\n";
        line += ">>> rooms : " + current_level_rooms_count + "\n";
        line += ">>> capables : " + current_level_capables_count + "\n";
        // line += ">>> capacities : " + current_level_capacities_count;
        return line;
    }
    private string get_room_line()
    {
        string line = "current room : " + current_room_name + "\n";
        line += ">>> capables : " + current_room_capables_count + "\n";
        // line += ">>> capacities : " + current_room_capacities_count;
        return line;
    }
}

