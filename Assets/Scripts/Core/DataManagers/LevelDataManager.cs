using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelDataManager : MonoBehaviour
{

    [Header("LevelData Saving")]
    public string data_folder = "Assets/Resources/data/levels/";
    public List<Level> levels_to_save = new List<Level>();

    [Header("Logs")]
    public bool log = false;


    public void SaveLevelsData()
    {
        foreach (Level level in levels_to_save)
        {
            LevelData data = level.GetStaticData();

            // save the current LevelData to a json file
            string json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(data_folder + data.id + ".json", json, System.Text.Encoding.UTF8);

            if (log) { Debug.Log($"(LevelDataManager) Updated & Saved LevelData : {level.name} (to {data_folder + data.id + ".json"})\n\n{json}"); }
        }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
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
        }
    }
#endif
}