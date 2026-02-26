using System.Collections.Generic;
using UnityEngine;

public class BSOD_System<T> : MonoBehaviour where T : MonoBehaviour
{
    // AWAKE & SINGLETON LOGIC
    public static T Instance;
    public virtual void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this as T; }
        else { Destroy(gameObject); return; }

        // loadObjectsData(); // <- this is for the example below
    }


    /*

    ! This is a basic example for using this system type of script as a system
    ! create a new class that inherit from BSOD_System and then you can paste it the code below
    ! don't forget to override Awake() method to put there your own awake logic
    
    
    [Header("Loaded objects")]
    public List<BSOD_Object> loaded_objects = new List<BSOD_Object>();

    [Header("Objects data")]
    private string data_path = "Assets/Resources/wherever/you/store/your/objects/data/";
    public List<BSOD_Data> objects_data = new List<BSOD_Data>();

    [Header("Loading parameters")]
    public int frames_between_loading_objects = 10;

    // LOAD OBJECTS
    protected void loadObjectsData()
    {
        // we load all the json files in the data path and convert them to RoomData objects
        string[] files = System.IO.Directory.GetFiles(data_path, "*.json");
        foreach (string file in files)
        {
            string json = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8);
            BSOD_Data data = JsonUtility.FromJson<BSOD_Data>(json);
            objects_data.Add(data);
        }
    }
    public async void LoadObjects(string[] objects_ids, Transform object_container)
    {
        // todo do this asynchronally

        // for each room id we need to find its data and load it
        foreach (string object_id in objects_ids)
        {
            BSOD_Data data = objects_data.Find(r => r.id == object_id);
            if (data == null) { Debug.LogWarning("(BSOD_System) Object data not found for id: " + object_id); continue; }

            // we load the object
            BSOD_Object obj = BSOD_ObjectBank.Instance.Load(data);
            loaded_objects.Add(obj);

            // we set the object as a child of the object_container
            obj.transform.SetParent(object_container);

            // we wait for X frames
            for (int i = 0; i < frames_between_loading_objects; i++) { await System.Threading.Tasks.Task.Yield(); }
        }
    }
    
    
    
    */

}