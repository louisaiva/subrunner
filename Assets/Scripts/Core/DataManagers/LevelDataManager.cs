using System.Collections.Generic;
using UnityEngine;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelDataManager : MonoBehaviour
{

    [Header("LevelData Saving")]
    public static string CurrentLevelDataFolder => Path.Combine(World.CurrentStaticWorldDataPath, "levels");
    public List<Level> levels_to_save = new List<Level>();
    public bool save_rooms_data = false; // if true, when we save the levels data, we also save the rooms data (ie we update the rooms data with the current overlapping capables in the editor)

    [Header("Logs")]
    public bool log = false;


    public void SaveLevelsData()
    {
        foreach (Level level in levels_to_save)
        {
            LevelData data = level.GetStaticData();

            // save the current LevelData to a json file
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(CurrentLevelDataFolder, data.id + ".json");
            System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);
            if (log) { Debug.Log($"(LevelDataManager) Updated & Saved LevelData : {level.name} (to {path})\n\n{json}"); }

            // check if we need to save the rooms also
            if (!save_rooms_data) { continue; }
            foreach (Room room in level.GetStaticRooms())
            {
                RoomData rdata = room.GetStaticData();

                // save the current RoomData to a json file
                json = JsonUtility.ToJson(rdata, true);
                path = Path.Combine(RoomDataSaver.CurrentRoomDataFolder, rdata.id + ".json");
                System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);

                if (log) { Debug.Log($"(LevelDataManager) Updated & Saved RoomData : {room.name} (to {path})\n\n{json}"); }
            }

        }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }

    public void MakeRoomsGrabCapables()
    {
        List<Capable> overlapping_capables = new List<Capable>();
        List<string> added_capable_ids = new List<string>();

        string log = "";

        foreach (Level level in levels_to_save)
        {
            log += $" - Level {level.name} :\n";
            Room[] rooms = level.GetStaticRooms();
            foreach (Room room in rooms)
            {
                log += $"   - Room {room.name} :\n";
                overlapping_capables.Clear();
                overlapping_capables.AddRange(room.GetStaticOverlappingCapables());

                // . clear the capables & movables ids room data
                room.data.capables_ids = new List<string>();
                room.data.movables_ids = new List<string>();

                // we try to add the capable ids to the room data
                foreach (Capable capable in overlapping_capables)
                {
                    string capable_id = capable.GetStaticID();
                    if (added_capable_ids.Contains(capable_id)) { continue; } // already added somewhere

                    // . verify not Perso
                    if (capable is Perso) { continue; }

                    // . verify if not grabbed item
                    if (capable is Item item && item.GetStaticGrabbed()) { continue; }

                    // . check if movable or capable
                    if (capable is Movable)
                    {
                        if (room.data.movables_ids == null) { room.data.movables_ids = new List<string>(); }
                        if (!room.data.movables_ids.Contains(capable_id)) { room.data.movables_ids.Add(capable_id); }
                        log += $"     - Movable '{capable_id}'\n";
                    }
                    else
                    {
                        if (room.data.capables_ids == null) { room.data.capables_ids = new List<string>(); }
                        if (!room.data.capables_ids.Contains(capable_id)) { room.data.capables_ids.Add(capable_id); }
                        log += $"     - Capable '{capable_id}'\n";
                    }

                    // . memorize we added this capable to a room
                    added_capable_ids.Add(capable_id);
                }
                log += "\n";
            }
            log += "\n";
        }

        if (this.log) { Debug.Log($"(LevelDataManager) Total Capables grabbed : {added_capable_ids.Count}\n{log}"); }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(LevelDataManager))]
    public class LevelDataManagerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            LevelDataManager saver = (LevelDataManager)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Update and Save LevelData")) { saver.SaveLevelsData(); }
            if (GUILayout.Button("Make Rooms grab Capables")) { saver.MakeRoomsGrabCapables(); }
        }
    }
#endif
}