using System.Collections.Generic;
using UnityEngine;

public class RoomSystem : MonoBehaviour
{
    // AWAKE & SINGLETON LOGIC
    public static RoomSystem Instance { get; private set; }
    private void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // load rooms data
        loadRoomsData();
    }


    [Header("Loaded rooms")]
    public List<Room> loaded_rooms = new List<Room>();

    [Header("Rooms data")]
    private string data_path = "Assets/Resources/data/rooms/";
    public List<RoomData> rooms_data = new List<RoomData>();

    [Header("Loading parameters")]
    public int frames_between_loaded_rooms = 10;

    // LOAD ROOMS
    protected void loadRoomsData()
    {
        // we load all the json files in the data path and convert them to RoomData objects
        string[] files = System.IO.Directory.GetFiles(data_path, "*.json");
        foreach (string file in files)
        {
            string json = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8);
            RoomData data = JsonUtility.FromJson<RoomData>(json);
            rooms_data.Add(data);
        }
    }
    public async void LoadRooms(string[] rooms_ids, Level level)
    {
        // todo do this asynchronally

        // for each room id we need to find its data and load it
        foreach (string room_id in rooms_ids)
        {
            RoomData data = rooms_data.Find(r => r.id == room_id);
            if (data == null) { Debug.LogWarning("(RoomSystem) Room data not found for id: " + room_id); continue; }

            // we load the room
            Room room = RoomBank.Instance.Load(data);
            loaded_rooms.Add(room);

            // we set the room as a child of the level
            room.transform.SetParent(level.transform);

            // we wait for X frames
            for (int i = 0; i < frames_between_loaded_rooms; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
    }

}