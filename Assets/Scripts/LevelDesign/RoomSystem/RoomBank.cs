using System.Collections.Generic;
using UnityEngine;

public class RoomBank : MonoBehaviour
{

    // AWAKE & SINGLETON LOGIC
    public static RoomBank Instance { get; private set; }
    private void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // initialize the pool of rooms
        pooled_rooms = new Stack<Room>();
    }

    // ROOM LOADING
    [Header("Room prefab")]
    [SerializeField] protected Room room_prefab;
    protected Stack<Room> pooled_rooms;
    public Room Load(RoomData data)
    {
        // if we have no pooled room we need to instantiate one
        if (pooled_rooms.Count == 0)
        {
            Room new_room = Instantiate(room_prefab, transform);
            new_room.LoadData(data);
            return new_room;
        }

        // extract a room from the pooled ones and load its data
        Room room = pooled_rooms.Pop();
        room.LoadData(data);
        return room;
    }
    public void Unload(Room room)
    {
        // unload the room's data and put it back in the pool
        room.UnloadData();
        pooled_rooms.Push(room);
    }
}