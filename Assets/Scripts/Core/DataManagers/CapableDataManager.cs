
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CapableDataManager : MonoBehaviour
{

    // [Header("CapableData Loading")]
    // public List<string> capables_to_load = new List<string>();

    [Header("CapableData Saving")]
    public List<Capable> capables_to_save = new List<Capable>();
    // public static string CurrentCapableDataFolder => Path.Combine(World.CurrentStaticWorldDataPath, "capables");

    [Header("Extended parameters")]
    public bool save_capables_in_inventory = false;
    public bool save_capables_capacities = false;
    // public static string CurrentCapacityDataFolder => Path.Combine(World.CurrentStaticWorldDataPath, "capacities");


    [Header("Logs")]
    public bool log = false;

    public void SaveCapablesData()
    {
        foreach (Capable capable in capables_to_save)
        {
            saveCapableData(capable, World.CurrentStaticWorldDataPath);
        }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    private void saveCapableData(Capable capable, string world_path)
    {
        ICapableData data = capable.GetStaticData();
        // save the current RoomData to a json file
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(world_path, "capables", data.id + ".json");
        System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        if (log) { Debug.Log($"(Capable - Save Data) Updated & Saved CapableData : {capable.name} (to {path})\n\n{data.GetDetails()}\n\n{json}"); }

        // we also save the capacities of this capable if we want to
        if (save_capables_capacities) { saveCapacitiesOfCapable(capable, world_path); }


        // we also save the capables in its inventory if we want to
        if (!save_capables_in_inventory) { return; }
        Inventory inv = capable.Inventory;
        if (inv == null) { return; }

        List<Item> items = inv.GetStaticItems();
        foreach (Item item in items)
        {
            saveCapableData(item, world_path);
        }
    }
    private void saveCapacitiesOfCapable(Capable capable, string world_path)
    {
        // we get all the capacities (ONLY DIRECT CHILDREN - we don't want to get the capa of the items we store :)
        List<Capacity> capacities = new List<Capacity>();
        for (int i = 0; i < capable.transform.childCount; i++)
        {
            Transform child = capable.transform.GetChild(i);
            Capacity capa = child.GetComponent<Capacity>();
            if (capa == null) { continue; }
            capacities.Add(capa);
        }

        // then we save all the data of these capacities
        foreach (Capacity capacity in capacities)
        {
            CapacityData capacity_data = capacity.GetStaticData();

            // save the current RoomData to a json file
            string capacity_json = JsonUtility.ToJson(capacity_data, true);
            string path = Path.Combine(world_path, "capacities", capacity_data.id + ".json");
            System.IO.File.WriteAllText(path, capacity_json, System.Text.Encoding.UTF8);
            if (log) { Debug.Log($"(Capable - Save Data) Updated & Saved CapacityData : {capacity.name} (to {path})\n\n{capacity_data.GetDetails()}\n\n{capacity_json}"); }
        }
    }
    public void SaveAllCapablesDataInScene()
    {
        Capable[] all_capables = FindObjectsByType<Capable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        string world_path = World.CurrentStaticWorldDataPath;
        foreach (Capable capable in all_capables)
        {
            saveCapableData(capable, world_path);
        }



        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    public void SaveCapablesData(List<Capable> capables, string world_path)
    {
        foreach (Capable capable in capables)
        {
            saveCapableData(capable, world_path);
        }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
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
            if (GUILayout.Button("Update and Save All CapableData in Scene")) { manager.SaveAllCapablesDataInScene(); }
        }
    }
#endif
}