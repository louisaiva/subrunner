using System.Collections.Generic;
using UnityEngine;

public class RoomBank : MonoBehaviour
{

    // AWAKE & SINGLETON LOGIC
    public static RoomBank Instance { get; private set; }
    public void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // initialize the pool of rooms
        pooled_rooms = new Stack<Room>();
    }

    // ROOM LOADING

    [Header("Loaded rooms")]
    [SerializeField] protected List<Room> loaded_rooms;
    [SerializeField] protected Transform room_parent;
    [SerializeField] protected Room room_prefab;

    [Header("Sleeping rooms")]
    [SerializeField] protected Stack<Room> pooled_rooms;
    
    // LOAD UNLOAD ROOMS
    public void Load(RoomData data)
    {
        // if we have no pooled room we need to instantiate one
        if (pooled_rooms.Count == 0)
        {
            Room new_room = Instantiate(room_prefab, transform);
            new_room.LoadData(data);
            loaded_rooms.Add(new_room);
            return;
        }

        // extract a room from the pooled ones and load its data
        Room room = pooled_rooms.Pop();
        room.LoadData(data);
        room.gameObject.SetActive(true);
        loaded_rooms.Add(room);
    }
    public void Unload(RoomData data)
    {
        // get room
        Room room = GetLoadedRoom(data);
        if (room == null) { return; }
        Unload(room);
    }
    public void Unload(Room room)
    {
        // unload the room's data and put it back in the pool
        room.UnloadData();
        pooled_rooms.Push(room);

        // remove the room from the loaded rooms list
        loaded_rooms.Remove(room);

        // disable the gameObject
        room.gameObject.SetActive(false);
    }


    // ROOM GETTING
    public Room GetLoadedRoom(string id)
    {
        // we look for the room with the given id in the pool of loaded rooms
        for (int i = 0; i < loaded_rooms.Count; i++)
        {
            Room room = loaded_rooms[i];
            if (room.data != null && room.data.id == id)
            {
                return room;
            }
        }
        return null;
    }
    public Room GetLoadedRoom(RoomData data)
    {
        return GetLoadedRoom(data.id);
    }
    public List<Room> GetLoadedRooms(List<RoomData> data)
    {
        List<Room> rooms = new List<Room>();
        foreach (RoomData d in data)
        {
            Room room = GetLoadedRoom(d);
            if (room != null) { rooms.Add(room); }
        }
        return rooms;
    }
    public List<Room> GetAllLoadedRooms()
    {
        return new List<Room>(loaded_rooms);
    }
}