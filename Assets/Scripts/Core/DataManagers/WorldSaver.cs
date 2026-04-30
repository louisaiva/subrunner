using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;




#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldSaver : MonoBehaviour
{

    [Header("World Saving")]
    public string world_to_save = "";
    public bool save_levels = false;
    
    
    [Header("Logs")]
    public bool log = false;
    

    [Header("Components")]
    private LevelDataManager _level_manager;
    private LevelDataManager LevelManager
    {
        get
        {
            if (_level_manager == null) { _level_manager = GetComponent<LevelDataManager>(); }
            if (_level_manager == null) { Debug.LogError($"(WorldManager) No LevelDataManager found on the {name} game object. Please add one to the scene."); }
            return _level_manager;
        }
    }
    private IDsGenerator _ids_generator;
    private IDsGenerator IDsGenerator
    {
        get
        {
            if (_ids_generator == null) { _ids_generator = GetComponent<IDsGenerator>(); }
            if (_ids_generator == null) { Debug.LogError($"(WorldManager) No IDsGenerator found on the {name} game object. Please add one to the scene."); }
            return _ids_generator;
        }
    }


    // ALL IN ONE METHOD
    public void CleanAndSaveWorldData()
    {
        // we regenerate the ids for all the capables & capacities in the world
        if (log) { Debug.Log(" "); }
        if (log) { Debug.Log("(WorldManager) ################# 1 - Regenerating IDs for all Capables and Capacities in the world..."); }
        if (log) { Debug.Log(" "); }
        IDsGenerator.GenerateIDsForAllCapablesAndCapacitiesInWorld();

        // we make the rooms grab the capables
        if (log) { Debug.Log(" "); }
        if (log) { Debug.Log("(WorldManager) ################# 2 - Making the rooms grab the capables..."); }
        if (log) { Debug.Log(" "); }
        LevelManager.MakeRoomsGrabCapables(World.StaticInstance.GetStaticLevels());

        // todo generate the navmesh ????
        // NavMeshBuilder.BuildNavMesh();

        // we save the world data
        if (log) { Debug.Log(" "); }
        if (log) { Debug.Log($"(WorldManager) ################# 3 - Saving the world data... (id is {get_world_id()})"); }
        if (log) { Debug.Log(" "); }
        SaveWorldData();
    }


    // SAVING DATA
    public void SaveWorldData()
    {
        string id = get_world_to_save(out World world);
        if (string.IsNullOrEmpty(id) || world == null) { Debug.LogError("(WorldManager) Cannot save world data: no world found to save."); return; }
        save_world_data(id, world);
    }
    private void save_world_data(string id, World world)
    {
        bool just_created = World.EnsureWorldDataHierarchy(id); // make sure all the folders for this world exist in the persistent data path

        // check if we need to save the levels data
        if (save_levels)
        {
            Level[] levels = world.GetStaticLevels();
            LevelManager.SaveLevels(levels.ToList(), World.GetWorldDataPath(id));
        }

        // get the data
        WorldData data = world.GetStaticData();
        data.game_version = Application.version;
        data.last_update_date = DateTime.Now.ToString();
        data.creation_date = (just_created || string.IsNullOrEmpty(data.creation_date)) ? data.last_update_date : data.creation_date;

        // check if we just created it and we don't have any icon, then we set random color and default icon
        if (just_created && string.IsNullOrEmpty(data.icon_path))
        {
            data.color = WorldManager.Instance.GetRandomWorldColor();
            data.icon_path = WorldManager.Instance.GetRandomIconPath(out string icon_name);
            data.icon_name = icon_name;
        }

        // save the current WorldData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(World.GetWorldDataPath(id), "world_data.json");
        System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        if (log) { Debug.Log($"(WorldManager) Updated & Saved WorldData : {id} (to {path})\n\n{json}"); }
    }
    private string get_world_to_save(out World world)
    {
        // we get the world instance
        world = World.StaticInstance;
        if (world == null) { Debug.LogError("(WorldManager) No World instance found in the scene. Please add one to the scene."); return ""; }
        
        // we check if we have a world_id to save
        if (!string.IsNullOrEmpty(world_to_save)) { return world_to_save; }
        
        // else we try to get it from the world instance
        if (!string.IsNullOrEmpty(world.world_id)) { return world.world_id; }

        // finally we throw an error if we don't have a world_id to save
        Debug.LogError("(WorldManager) Cannot save world data: world to save id is null or empty.");
        return "";
    }
    private string get_world_id()
    {
        if (!string.IsNullOrEmpty(world_to_save)) { return world_to_save; }

        World world = World.StaticInstance;
        if (world == null) { Debug.LogError("(WorldManager) No World instance found in the scene. Please add one to the scene."); return "null"; }
        if (!string.IsNullOrEmpty(world.world_id)) { return world.world_id; }
        return "empty ://";
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(WorldSaver))]
    public class WorldSaverEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            WorldSaver saver = (WorldSaver)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Update and Save World")) { saver.SaveWorldData(); }

            if (GUILayout.Button("Update and Save World (Clean)")) { saver.CleanAndSaveWorldData(); }
        }
    }
#endif
}