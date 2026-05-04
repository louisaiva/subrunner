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
            if (_level_manager == null) { Debug.LogError($"(WorldSaver) No LevelDataManager found on the {name} game object. Please add one to the scene."); }
            return _level_manager;
        }
    }
    private IDsGenerator _ids_generator;
    private IDsGenerator IDsGenerator
    {
        get
        {
            if (_ids_generator == null) { _ids_generator = GetComponent<IDsGenerator>(); }
            if (_ids_generator == null) { Debug.LogError($"(WorldSaver) No IDsGenerator found on the {name} game object. Please add one to the scene."); }
            return _ids_generator;
        }
    }


    // ALL IN ONE METHOD
    public void CleanAndSaveWorldData()
    {
        // we regenerate the ids for all the capables & capacities in the world
        if (log) { Debug.Log(" "); }
        if (log) { Debug.Log("(WorldSaver) ################# 1 - Regenerating IDs for all Capables and Capacities in the world..."); }
        if (log) { Debug.Log(" "); }
        IDsGenerator.GenerateIDsForAllCapablesAndCapacitiesInWorld();

        // we make the rooms grab the capables
        if (log) { Debug.Log(" "); }
        if (log) { Debug.Log("(WorldSaver) ################# 2 - Making the rooms grab the capables..."); }
        if (log) { Debug.Log(" "); }
        LevelManager.MakeRoomsGrabCapables(World.LazyInstance.GetStaticLevels());

        // todo generate the navmesh ????
        // NavMeshBuilder.BuildNavMesh();

        // we save the world data
        if (log) { Debug.Log(" "); }
        if (log) { Debug.Log($"(WorldSaver) ################# 3 - Saving the world data... (id is {get_world_id()})"); }
        if (log) { Debug.Log(" "); }
        SaveWorldData();
    }


    // SAVING DATA
    public void SaveWorldData()
    {
        string id = get_world_to_save(out World world);
        if (string.IsNullOrEmpty(id) || world == null) { Debug.LogError("(WorldSaver) Cannot save world data: no world found to save."); return; }
        save_world_data(id, world);
    }
    private void save_world_data(string id, World world)
    {
        // get the data & save it
        WorldData data = world.GetStaticData();
        if (string.IsNullOrEmpty(data.id)) { data.id = id; } // we set the id if not already set, so we can save it from the editor not at runtime
        SaveEngine.SaveWorldData(data); // will ensure the hierarchy exists and then save the world data

        // check if we need to save the levels data
        if (save_levels)
        {
            Level[] levels = world.GetStaticLevels();
            LevelManager.SaveLevels(levels.ToList(), id);
        }
    }
    private string get_world_to_save(out World world)
    {
        // we get the world instance
        world = World.LazyInstance;
        if (world == null) { Debug.LogError("(WorldSaver) No World instance found in the scene. Please add one to the scene."); return ""; }
        
        // we check if we have a world_id to save
        if (!string.IsNullOrEmpty(world_to_save)) { return world_to_save; }
        
        // else we try to get it from the world instance
        if (!string.IsNullOrEmpty(world.world_id)) { return world.world_id; }

        // finally we throw an error if we don't have a world_id to save
        Debug.LogError("(WorldSaver) Cannot save world data: world to save id is null or empty.");
        return "";
    }
    private string get_world_id()
    {
        if (!string.IsNullOrEmpty(world_to_save)) { return world_to_save; }

        World world = World.LazyInstance;
        if (world == null) { Debug.LogError("(WorldSaver) No World instance found in the scene. Please add one to the scene."); return "null"; }
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