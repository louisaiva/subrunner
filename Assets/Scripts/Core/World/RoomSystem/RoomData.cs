using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class RoomData
{
    [field: SerializeField] public string id { get; set; }
    public List<string> chunks_ids;

    // lights management
    public List<LightData> lights_data;
    
    // tilemaps data
    public string[] tilebases_names;
    public BoundsIntData ceiling_bounds;
    public int[] ceiling_tiles;
    public BoundsIntData edges_bounds;
    public int[] edges_tiles;
    public BoundsIntData walls_bounds;
    public int[] walls_tiles;
    public BoundsIntData carpet_bounds;
    public int[] carpet_tiles;
    public BoundsIntData ground_bounds;
    public int[] ground_tiles;


    public int TotalChunksCount { get { return chunks_ids != null ? chunks_ids.Count : 0; } }
    public int TotalCapablesCount
    {
        get
        {
            // we get the chunks and add them together
            List<ChunkData> chunks = ChunkEngine.Instance.GetChunksDataFromIDs(chunks_ids);
            int count = 0;
            foreach (ChunkData chunk_data in chunks)
            {
                count += chunk_data.TotalCapablesCount;
            }
            return count;
        }
    }
    public int TotalCapacitiesCount
    {
        get
        {
            // we get the chunks and add them together
            List<ChunkData> chunks = ChunkEngine.Instance.GetChunksDataFromIDs(chunks_ids);
            int count = 0;
            foreach (ChunkData chunk_data in chunks)
            {
                count += chunk_data.TotalCapacitiesCount;
            }
            return count;
        }
    }


    // GETTERS
    public bool HasTiles(string tilemap_type)
    {
        switch (tilemap_type)
        {
            case "edges":
                return edges_tiles != null && edges_tiles.Length > 0;
            case "ceiling":
                return ceiling_tiles != null && ceiling_tiles.Length > 0;
            case "walls":
                return walls_tiles != null && walls_tiles.Length > 0;
            case "carpet":
                return carpet_tiles != null && carpet_tiles.Length > 0;
            case "ground":
                return ground_tiles != null && ground_tiles.Length > 0;
            default:
                Debug.LogError($"(RoomData - HasTiles) Invalid tilemap type: {tilemap_type}");
                return false;
        }
    }
    protected int calculate_tilemap_non_null_tiles(int[] tiles)
    {
        if (tiles == null) { return 0; }
        int count = 0;
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != -1) { count++; }
        }
        return count;
    }
    public string GetDetails()
    {
        string details = $"Room {id} :\n";
        details += $"  - chunks : {(chunks_ids != null ? chunks_ids.Count : 0)}\n";
        if (chunks_ids != null)
        {
            for (int i = 0; i < chunks_ids.Count; i++)
            {
                details += $"    - {chunks_ids[i]}\n";
            }
        }
        details += $"  - tilemaps :\n";
        details += $"    - edges : {calculate_tilemap_non_null_tiles(edges_tiles)} tiles\n";
        details += $"    - ceiling : {calculate_tilemap_non_null_tiles(ceiling_tiles)} tiles\n";
        details += $"    - walls : {calculate_tilemap_non_null_tiles(walls_tiles)} tiles\n";
        details += $"    - carpet : {calculate_tilemap_non_null_tiles(carpet_tiles)} tiles\n";
        details += $"    - ground : {calculate_tilemap_non_null_tiles(ground_tiles)} tiles\n";
        return details;
    }
}

[Serializable] public class Vector3IntData
{
    public int x;
    public int y;
    public int z;

    public Vector3IntData(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Vector3IntData(Vector3Int vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }
    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    }
}

[Serializable] public class BoundsIntData
{
    public Vector3IntData m_Position;
    public Vector3IntData m_Size;

    public BoundsIntData()
    {
        this.m_Position = new Vector3IntData(0, 0, 0);
        this.m_Size = new Vector3IntData(0, 0, 0);
    }
    public BoundsIntData(BoundsInt bounds)
    {
        this.m_Position = new Vector3IntData(bounds.position);
        this.m_Size = new Vector3IntData(bounds.size);
    }
    public BoundsInt ToBoundsInt()
    {
        return new BoundsInt(m_Position.ToVector3Int(), m_Size.ToVector3Int());
    }
}