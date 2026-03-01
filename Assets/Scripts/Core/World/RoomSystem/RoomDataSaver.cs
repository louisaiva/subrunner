using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomDataSaver : MonoBehaviour
{

    [Header("RoomData Loading")]
    public List<string> rooms_to_load = new List<string>();

    [Header("RoomData Saving")]
    public string data_folder = "Assets/Resources/data/rooms/";
    public List<Room> rooms_to_save = new List<Room>();

    [Header("Logs")]
    public bool log = false;


    public void SaveRoomsData()
    {
        foreach (Room room in rooms_to_save)
        {
            RoomData data = room.UpdateData();

            // save the current RoomData to a json file
            string json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(data_folder + data.id + ".json", json, System.Text.Encoding.UTF8);

            if (log) { Debug.Log($"(RoomDataSaver) Updated & Saved RoomData : {room.name} (to {data_folder + data.id + ".json"})\n\n{json}"); }
        }
    }
    

#if UNITY_EDITOR
    [CustomEditor(typeof(RoomDataSaver))]
    public class RoomDataSaverEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            RoomDataSaver saver = (RoomDataSaver)target;

            if (GUILayout.Button("Load Rooms"))
            {
                // check if we have a RoomSystem & RoomBank
                RoomSystem room_system;
                if (RoomSystem.Instance == null)
                {
                    saver.GetComponent<RoomSystem>().Awake();
                    saver.GetComponent<RoomBank>().Awake();
                }

                // load all rooms from system
                room_system = RoomSystem.Instance;
                room_system.LoadRooms(saver.rooms_to_load.ToArray());
            }
            if (GUILayout.Button("Unload Rooms"))
            {
                // check if we have a RoomSystem
                if (RoomSystem.Instance == null) { return; }

                // unload all rooms from system
                RoomSystem.Instance.UnloadAllRooms();
                RoomBank.Instance.DestroyPooledRooms();
            }

            DrawDefaultInspector();

            if (GUILayout.Button("Update and Save RoomData")) { saver.SaveRoomsData(); }
        }
    }
#endif
}