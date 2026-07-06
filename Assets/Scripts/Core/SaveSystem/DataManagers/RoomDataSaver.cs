using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[Obsolete("RoomDataSaver, as other data saver from editor, is deprecated, use in-game SaveEngine instead")]
public class RoomDataSaver : MonoBehaviour
{

    [Header("RoomData Loading")]
    public List<string> rooms_to_load = new List<string>();

    [Header("RoomData Saving")]
    public static string CurrentRoomDataFolder => Path.Combine(WorldManager.CurrentStaticWorldDataPath, "chunks");
    public List<Chunk> rooms_to_save = new List<Chunk>();

    [Header("Logs")]
    public bool log = false;


    public void SaveRoomsData()
    {
        foreach (Chunk room in rooms_to_save)
        {
            ChunkData data = room.GetStaticData();

            // save the current RoomData to a json file
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(CurrentRoomDataFolder, data.id + ".json");
            System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);

            if (log) { Debug.Log($"(RoomDataSaver) Updated & Saved RoomData : {room.name} (to {path})\n\n{json}"); }
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
                ChunkEngine room_system;
                if (ChunkEngine.Instance == null)
                {
                    room_system = FindFirstObjectByType<ChunkEngine>();
                    room_system.Awake();
                    FindFirstObjectByType<ChunkBank>().Awake();
                }

                // load all rooms from system
                room_system = ChunkEngine.Instance;
                _ = room_system.LoadChunks(saver.rooms_to_load.ToArray());
            }
            if (GUILayout.Button("Unload Rooms"))
            {
                // check if we have a RoomSystem
                if (ChunkEngine.Instance == null) { return; }

                // unload all rooms from system
                _ = ChunkEngine.Instance.UnloadAllChunks();
                ChunkBank.Instance.DestroyAllChunksInstantly();
            }

            DrawDefaultInspector();

            if (GUILayout.Button("Update and Save RoomData")) { saver.SaveRoomsData(); }
        }
    }
#endif
}