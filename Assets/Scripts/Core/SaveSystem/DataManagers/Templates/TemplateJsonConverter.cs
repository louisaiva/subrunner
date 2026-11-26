using UnityEngine;
using Newtonsoft.Json;
using System;




#if UNITY_EDITOR
using UnityEditor;
#endif

public class TemplateJsonConverter : MonoBehaviour
{

    /* private static JsonSerializerSettings instance_contract = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
        ContractResolver = new InstanceContractResolver()
    }; */


    public GameObject template;
    [SerializeField] private JSON_Contract converter = JSON_Contract.Default;

    [Header("Logs")]
    public bool log = false;

    public void ConvertTemplateToJSON()
    {
        if (template == null) { Debug.LogError("(TemplateJsonConverter) Attach a template in the inspector first !"); return; }
        if (template.TryGetComponent(out Capable capable)) { convertCapableToJSON(capable); return; }
        if (template.TryGetComponent(out Capacity capacity)) { convertCapacityToJSON(capacity); return; }
        Debug.LogError("(TemplateJsonConverter) Attached gameobject must have either a Capable or Capacity to save !");
    }
    public void convertCapableToJSON(Capable capable)
    {
        CapableData data = (CapableData)capable.GetStaticData();

        // we apply some modifs to the data, i.e. adding some specific capacities templates
        // this is mainly for simplifying the work flow inside unity, bcz with data-driven approach
        // we don't have a lot of control on modifying data in scene view
        // We do most heavy job with templates, AND SO we must be fast
        TemplatesManager.PrepareCapableDataTemplate(capable, ref data);

        // convert the current data to a json file
        string json = convertToJSON(data);
        // string path = templates_data_path + "capables/" + data.id + ".json";
        // System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);

        if (log) { Debug.Log($"(TemplateJsonConverter) Converted : {capable.ID} to JSON using {converter}.\n\n{json}\n\n{data.GetDetails()}"); }
    }
    public void convertCapacityToJSON(Capacity capacity)
    {
        CapacityData capacity_data = capacity.GetStaticData();

        // convert the current data to a json file
        string capacity_json = convertToJSON(capacity_data);
        // string path = templates_data_path + "capacities/" + capacity_data.id + ".json";
        // System.IO.File.WriteAllText(path, capacity_json, System.Text.Encoding.UTF8);

        if (log) { Debug.Log($"(TemplateJsonConverter) Converted : {capacity.ID} to JSON using {converter}.\n\n{capacity_json}\n\n{capacity_data.GetDetails()}"); }
    }

    public string convertToJSON(object data)
    {
        string json = "";

        JsonSerializerSettings contract = get_contract();
        if (contract == null) { return "Default\n" + JsonUtility.ToJson(data, true); }

        try
        {
            json = JsonConvert.SerializeObject(data, contract);
            return converter + "\n" + json;
        }
        catch (Exception ex) { Debug.LogException(ex); }
        return json;
    }

    private JsonSerializerSettings get_contract()
    {
        switch (converter)
        {
            case JSON_Contract.Old: return SaveEngine.OldContract;
            case JSON_Contract.Instance: return SaveEngine.InstanceContract;
            case JSON_Contract.Kind: return SaveEngine.KindContract;
        }
        return null;
    }

    private enum JSON_Contract
    {
        Default,
        Old,
        Instance,
        Kind
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(TemplateJsonConverter))]
    public class TemplatesManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TemplateJsonConverter saver = (TemplateJsonConverter)target;

            DrawDefaultInspector();
            if (GUILayout.Button("Convert Template"))
            {
                saver.ConvertTemplateToJSON();
                AssetDatabase.Refresh();
            }
        }
    }
    #endif
}