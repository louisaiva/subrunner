using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldManager : MonoBehaviour
{

    [Header("World Saving")]
    public bool save_levels = false;
    
    
    [Header("Logs")]
    public bool log = false;
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




    public void SaveWorldData()
    {
        // get the world instance
        World world = World.StaticInstance;
        if (world == null) { Debug.LogError("(WorldManager) No World instance found in the scene. Please add one to the scene."); return; }
        if (string.IsNullOrEmpty(world.world_id))
        {
            Debug.LogError("(WorldManager) Cannot save world data: world_id is null or empty.");
            return;
        }
        world.EnsureWorldDataHierarchy(); // make sure all the folders for this world exist in the persistent data path


        // check if we need to save the levels data
        if (save_levels)
        {
            Level[] levels = world.GetStaticLevels();
            LevelManager.SaveLevels(levels.ToList());
        }

        WorldData data = world.GetStaticData();

        // save the current WorldData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(world.CurrentWorldDataPath, "world_data.json");
        System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        if (log) { Debug.Log($"(WorldManager) Updated & Saved WorldData : {world.world_id} (to {path})\n\n{json}"); }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(WorldManager))]
    public class WorldManagerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            WorldManager saver = (WorldManager)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Update and Save Current World")) { saver.SaveWorldData(); }
        }
    }
#endif
}