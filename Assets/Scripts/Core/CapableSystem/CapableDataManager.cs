
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CapableDataManager : MonoBehaviour
{

    // [Header("CapableData Loading")]
    // public List<string> capables_to_load = new List<string>();

    [Header("CapableData Saving")]
    public List<Capable> capables_to_save = new List<Capable>();
    private string data_path = "Assets/Resources/data/capables/";

    [Header("Logs")]
    public bool log = false;

    public void SaveCapablesData()
    {
        foreach (Capable capable in capables_to_save)
        {
            CapableData data = capable.GetStaticData();

            // save the current RoomData to a json file
            string json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(data_path + data.id + ".json", json, System.Text.Encoding.UTF8);

            if (log) { Debug.Log($"(Capable - Save Data) Updated & Saved CapableData : {capable.name} (to {data_path + data.id + ".json"})\n\n{json}"); }
        }
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(CapableDataManager))]
    public class CapableDataManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CapableDataManager manager = (CapableDataManager)target;

            if (GUILayout.Button("Update and Save CapableData")) { manager.SaveCapablesData(); }
            DrawDefaultInspector();
        }
    }
#endif
}