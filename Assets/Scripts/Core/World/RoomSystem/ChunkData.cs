using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class ChunkData : IData
{
    [field: SerializeField] public string id { get; set; }
    public string room_id; // the id of the room this chunk belongs to
    public Vector2 position;

    // colliders
    public List<Vector2> collider_points;

    // neighbours data
    public List<string> neighbours_ids; // list of the rooms that are directly connected to this one, used for loading/unloading logic

    // Capable management
    public List<string> capables_ids;
    public List<string> movables_ids;
    public int TotalCapablesCount { get { return capables_ids.Count + movables_ids.Count; } }
    public int TotalCapacitiesCount
    {
        get
        {
            // we get the capable data for each capable in the room and we sum their capacities count
            List<CapableData> capables_data = CapableEngine.Instance.GetCapablesDataFromIDs(capables_ids);
            capables_data.AddRange(CapableEngine.Instance.GetCapablesDataFromIDs(movables_ids));
            int count = 0;
            foreach (CapableData capable_data in capables_data)
            {
                count += capable_data.TotalCapacitiesCount();
            }
            return count;
        }
    }
    

    // lights management
    public List<LightData> lights_data;

    // Events
    [RuntimeOnly] public Action<ChunkData> OnChunkLoaded;
    [RuntimeOnly] public Action<ChunkData> OnChunkUnloaded;


    // GETTERS
    public string GetDetails()
    {
        string details = $"Chunk {id} :\n";
        details += $"  - owner room : {room_id}\n";
        details += $"  - position : {position}\n";
        details += $"  - neighbours : {(neighbours_ids != null ? neighbours_ids.Count : 0)} rooms\n";
        details += $"  - capables : {(capables_ids != null ? capables_ids.Count : 0)} capables\n";
        details += $"  - movables : {(movables_ids != null ? movables_ids.Count : 0)} movables\n";
        details += $"  - colliders : {(collider_points != null ? collider_points.Count : 0)} points\n";
        details += $"  - lights : {(lights_data != null ? lights_data.Count : 0)} lights\n";
        return details;
    }

    // DUPLICATE
    public ChunkData Duplicate()
    {
        ChunkData duplicate = new ChunkData()
        {
            id = this.id,
            room_id = this.room_id,
            position = this.position,
            collider_points = new List<Vector2>(this.collider_points),
            neighbours_ids = new List<string>(this.neighbours_ids),
            capables_ids = new List<string>(this.capables_ids),
            movables_ids = new List<string>(this.movables_ids),
            lights_data = new List<LightData>()
        };
        foreach (LightData light in this.lights_data)
        {
            duplicate.lights_data.Add(light.Duplicate());
        }
        return duplicate;
    }

}


[Serializable] public class LightData
{
    // public string id;
    public Vector2 position;
    public Color color;
    public float intensity;
    public Vector2 radius; // inner & outer radius for the light falloff
    public float falloff; // how fast the light decreases

    // DUPLICATE
    public LightData Duplicate()
    {
        return new LightData()
        {
            position = this.position,
            color = this.color,
            intensity = this.intensity,
            radius = this.radius,
            falloff = this.falloff
        };
    }
}