using UnityEditor;
using UnityEngine;

public class RoomDataSaver : MonoBehaviour
{

    [Header("RoomData Path")]
    public string data_folder = "Assets/Resources/data/rooms/";

#if UNITY_EDITOR
    [CustomEditor(typeof(RoomDataSaver))]
    public class RoomDataSaverEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            RoomDataSaver saver = (RoomDataSaver)target;
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