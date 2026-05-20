using System.Collections.Generic;
using UnityEngine;

public class ChunkBank : MonoBehaviour
{

    // AWAKE & SINGLETON LOGIC
    public static ChunkBank Instance { get; private set; }
    public void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // initialize the pool of rooms
        pooled_rooms = new Stack<Chunk>();
    }

    // ROOM LOADING
    [Header("Loaded chunks")]
    [SerializeField] protected List<Chunk> loaded_chunks;
    [SerializeField] protected Chunk room_prefab;
    private Transform room_parent => World.LazyInstance.ChunkParent;

    [Header("Sleeping chunks")]
    [SerializeField] protected Stack<Chunk> pooled_rooms;

    // LOAD UNLOAD ROOMS
    public Chunk Load(ChunkData data)
    {
        // if we have no pooled room we need to instantiate one
        if (pooled_rooms.Count == 0)
        {
            Chunk new_room = Instantiate(room_prefab, room_parent);
            new_room.LoadData(data);
            loaded_chunks.Add(new_room);
            return new_room;
        }

        // extract a room from the pooled ones and load its data
        Chunk room = pooled_rooms.Pop();
        room.LoadData(data);
        room.enabled = true;
        loaded_chunks.Add(room);
        return room;
    }
    public void Unload(ChunkData data)
    {
        // get room
        Chunk room = GetLoadedRoom(data);
        if (room == null) { return; }
        Unload(room);
    }
    public void Unload(Chunk room)
    {
        // unload the room's data and put it back in the pool
        room.UnloadData();
        pooled_rooms.Push(room);

        // remove the room from the loaded rooms list
        loaded_chunks.Remove(room);

        // disable room component
        room.enabled = false;
    }


    // DESTROY ROOMS
    public void DestroyAllRoomsInstantly()
    {
        destroy_all_loaded_rooms();
        destroy_all_pooled_rooms();
    }
    private void destroy_all_loaded_rooms()
    {
        while (loaded_chunks.Count > 0)
        {
            Chunk room = loaded_chunks[0];
            if (!Application.isPlaying) { DestroyImmediate(room.gameObject); }
            else { Destroy(room.gameObject); }
            loaded_chunks.RemoveAt(0);
        }
        loaded_chunks.Clear();
    }
    private void destroy_all_pooled_rooms()
    {
        while (pooled_rooms.Count > 0)
        {
            Chunk room = pooled_rooms.Pop();
            if (!Application.isPlaying) { DestroyImmediate(room.gameObject); }
            else { Destroy(room.gameObject); }
        }
        pooled_rooms.Clear();
    }


    // ROOM GETTING
    public Chunk GetLoadedRoom(string id)
    {
        // we look for the room with the given id in the pool of loaded rooms
        for (int i = 0; i < loaded_chunks.Count; i++)
        {
            Chunk room = loaded_chunks[i];
            if (room.data != null && room.data.id == id)
            {
                return room;
            }
        }
        return null;
    }
    public Chunk GetLoadedRoom(ChunkData data)
    {
        return GetLoadedRoom(data.id);
    }
    public List<Chunk> GetLoadedRooms(List<ChunkData> data)
    {
        List<Chunk> rooms = new List<Chunk>();
        foreach (ChunkData d in data)
        {
            Chunk room = GetLoadedRoom(d);
            if (room != null) { rooms.Add(room); }
        }
        return rooms;
    }
    public List<Chunk> GetAllLoadedRooms()
    {
        return new List<Chunk>(loaded_chunks);
    }
    public bool IsRoomLoaded(ChunkData data)
    {
        for (int i = 0; i < loaded_chunks.Count; i++)
        {
            Chunk room = loaded_chunks[i];
            if (room.data != null && room.data == data) { return true; }
        }
        return false;
    }
}