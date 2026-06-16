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
    
}