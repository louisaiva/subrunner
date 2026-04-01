using System.Collections.Generic;
using UnityEngine;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldManager : MonoBehaviour
{

    [Header("Logs")]
    public bool log = false;


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