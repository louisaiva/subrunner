using System;
using System.Collections.Generic;
using Newtonsoft.Json;


/// <summary>
/// [MID TERM]
/// This class is WIP because for now we want to stick to folder saves.
/// But later we will have all the data stored in a single WorldSave file so we can easily
/// transfer it and etc
/// </summary>
[Serializable] public class WorldSaveData
{
    public WorldData world;
    public ControllerData controller;
    public List<LevelData> levels;
    public List<RoomData> rooms;
    public List<ChunkData> chunks;
    public List<CapableData> capables;
    public List<CapacityData> capacities;
    [RuntimeOnly] public string ID => world != null ? world.id : "null";

    public WorldSaveData()
    {
        world = null;
        controller = null;
        levels = new List<LevelData>();
        rooms = new List<RoomData>();
        chunks = new List<ChunkData>();
        capables = new List<CapableData>();
        capacities = new List<CapacityData>();
    }
    public WorldSaveData(WorldData world, ControllerData controller, List<LevelData> levels, List<RoomData> rooms, List<ChunkData> chunks, List<CapableData> capables, List<CapacityData> capacities)
    {
        this.world = world;
        this.controller = controller;
        this.levels = levels;
        this.rooms = rooms;
        this.chunks = chunks;
        this.capables = capables;
        this.capacities = capacities;
    }
    
    public WorldDataHelper ToWorldDataHelper()
    {
        WorldDataHelper helper = new WorldDataHelper
        {
            world = world,
            controller = controller
        };
        return helper;
    }


    public string GetDetails()
    {
        string details = "######## WORLD SAVE DATA DETAILS ########\n";
        details += "world : \n" + world.GetDetails();
        details += "\n";
        details += "controller : \n" + controller.GetDetails();
        details += "\n";


        // levels
        if (levels == null) { details += "levels : null\n"; }
        else
        {
            details += "levels : " + levels.Count + " \n";
            foreach (LevelData level in levels)
            {
                details += "   - " + level.id + "\n";
            }
        }

        // rooms
        if (rooms == null) { details += "rooms : null\n"; }
        else
        {
            details += "rooms : " + rooms.Count + " \n";
            foreach (RoomData room in rooms)
            {
                details += "   - " + room.id + "\n";
            }
        }

        // chunks
        if (chunks == null) { details += "chunks : null\n"; }
        else
        {
            details += "chunks : " + chunks.Count + " \n";
            foreach (ChunkData chunk in chunks)
            {
                details += "   - " + chunk.id + "\n";
            }
        }

        // capables
        if (capables == null) { details += "capables : null\n"; }
        else
        {
            details += "capables : " + capables.Count + " \n";
            foreach (CapableData capable in capables)
            {
                details += "   - " + capable.id + "\n";
            }
        }

        // capacities
        if (capacities == null) { details += "capacities : null\n"; }
        else
        {
            details += "capacities : " + capacities.Count + " \n";
            foreach (CapacityData capacity in capacities)
            {
                details += "   - " + capacity.id + "\n";
            }
        }
        return details;
    }

}


/// <summary>
/// this class is only a helper class for fast deserialization of world data.
/// only used when not in a world (i.e. main menu)
/// </summary>
[Serializable] public class WorldDataHelper
{
    public WorldData world;
    public ControllerData controller;

    [RuntimeOnly] public bool is_one_file = false;

    public string GetDetails()
    {
        string details = "";
        details += world.GetDetails();
        details += "\n";
        details += controller.GetDetails();
        return details;
    }
}