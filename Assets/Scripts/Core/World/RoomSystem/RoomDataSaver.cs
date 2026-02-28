using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomDataSaver : MonoBehaviour
{

    [Header("RoomData Loading")]
    public List<string> rooms_to_load = new List<string>();

    [Header("RoomData Saving")]
    public string data_folder = "Assets/Resources/data/rooms/";

    

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

            if (GUILayout.Button("Update and Save All RoomData"))
            {
                // get all rooms in the scene
                Room[] rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);

                foreach (Room room in rooms)
                {
                    RoomData data = room.UpdateData();
                    
                    // save the current RoomData to a json file
                    string json = JsonUtility.ToJson(data, true);
                    System.IO.File.WriteAllText(saver.data_folder + data.id + ".json", json, System.Text.Encoding.UTF8);
                }
            }
        }
    }
#endif
}