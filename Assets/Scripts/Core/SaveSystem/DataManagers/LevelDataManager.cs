using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;




#if UNITY_EDITOR
using UnityEditor;
#endif

[Obsolete("Most of the DataManagers are now obsolete since we save in-game :D")]
public class LevelDataManager : MonoBehaviour
{

    [Header("LevelData Saving")]
    // public static string CurrentLevelDataFolder => Path.Combine(WorldManager.CurrentStaticWorldDataPath, "levels");
    public List<Level> levels_to_save = new List<Level>();
    public bool save_rooms_data = false; // if true, when we save the levels data, we also save the rooms data (ie we update the rooms data with the current overlapping capables in the editor)
    public bool save_capables_data = false;

    [Header("Logs")]
    public bool log = false;

    private CapableDataManager _capable_manager;
    private CapableDataManager CapableManager
    {
        get
        {
            if (_capable_manager == null) { _capable_manager = GetComponent<CapableDataManager>(); }
            if (_capable_manager == null) { Debug.LogError("(LevelDataManager) No CapableDataManager found in the scene. Please add one to the scene."); }
            return _capable_manager;
        }
    }


    // SAVE LEVELS DATA
    public void SaveLevelsData()
    {
        foreach (Level level in levels_to_save)
        {
            save_level_data(level, WorldManager.CurrentStaticWorldDataPath);
        }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    public void SaveLevels(List<Level> levels, string world_id)
    {
        foreach (Level level in levels)
        {
            save_level_data(level, world_id);
        }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    
    
    // MAKE ROOMS GRAB CAPABLES
    public void MakeRoomsGrabCapables() { MakeRoomsGrabCapables(levels_to_save.ToArray()); }
    public void MakeRoomsGrabCapables(Level[] levels)
    {
        List<Capable> overlapping_capables = new List<Capable>();
        List<string> added_capable_ids = new List<string>();

        string log = "";

        foreach (Level level in levels)
        {
            make_level_grab_capables(level, ref overlapping_capables, ref added_capable_ids, ref log);
        }

        if (this.log) { Debug.Log($"(LevelDataManager) Total Capables grabbed : {added_capable_ids.Count}\n{log}"); }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    private void make_level_grab_capables(Level level, ref List<Capable> overlapping, ref List<string> added_ids, ref string log)
    {
        log += $" - Level {level.name} :\n";
        Chunk[] rooms = level.GetStaticChunks();
        foreach (Chunk room in rooms)
        {
            log += $"   - Room {room.name} :\n";
            overlapping.Clear();
            overlapping.AddRange(room.GetStaticOverlappingCapables<LevelDataManager>());

            // . clear the capables & movables ids room data
            room.data.capables_ids = new List<string>();
            room.data.movables_ids = new List<string>();

            // we try to add the capable ids to the room data
            foreach (Capable capable in overlapping)
            {
                string capable_id = capable.GetStaticID();
                if (added_ids.Contains(capable_id)) { continue; } // already added somewhere

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
                added_ids.Add(capable_id);
            }
            log += "\n";

            // mark the room data as dirty so it gets saved
            #if UNITY_EDITOR
            EditorUtility.SetDirty(room);
            #endif
        }
        log += "\n";
    }


    // low level saving
    private void save_level_data(Level level, string world_id)
    {
        LevelData data = level.GetStaticData();
        SaveEngine.SaveLevelData(data, world_id);

        // check if we need to save the rooms also
        if (save_rooms_data) { save_chunks_level_data(level, world_id); }

        // check if we need to save the capables also
        if (save_capables_data) { save_capables_level_data(level, world_id); }
    }
    private void save_chunks_level_data(Level level, string world_id)
    {
        string json;
        string path;
        foreach (Chunk room in level.GetStaticChunks())
        {
            ChunkData rdata = room.GetStaticData();

            // save the current RoomData to a json file
            json = JsonUtility.ToJson(rdata, true);
            path = Path.Combine("chunks", rdata.id + ".json");
            AppManager.SaveJsonToWorldFolder(world_id, path, json, log);/* 
            System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);

            if (log) { Debug.Log($"(LevelDataManager) Updated & Saved RoomData : {room.name} (to {path})\n\n{json}"); } */
        }
    }
    private void save_capables_level_data(Level level, string world_id)
    {
        // we use the CapablesDataManager to save the capables data so it saves them with the parameters etc
        // (save capacities & save capables in inventory)
        CapableManager.SaveCapablesData(level.GetStaticCapables().ToList(), world_id);
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