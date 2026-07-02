using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LightsDataExporter : MonoBehaviour
{
    [SerializeField] private string exportPath;

    private static readonly JsonSerializerSettings exportSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        ContractResolver = new UnityValueTypeContractResolver()
    };

    public Dictionary<string, List<LightData>> ExportLightsData(List<Room> rooms, out int lights_nb)
    {
        lights_nb = 0;
        Dictionary<string, List<LightData>> lights_data_dict = new Dictionary<string, List<LightData>>();
        foreach (Room room in rooms)
        {
            if (room == null) { continue; }
            List<LightData> lights_data = room.GetStaticLightsData();
            lights_data_dict[room.ID] = lights_data;
            lights_nb += lights_data.Count;
        }
        return lights_data_dict;
    }

    public void ExportAndSave()
    {
        if (string.IsNullOrEmpty(exportPath))
        {
            Debug.LogError("LightsDataExporter: exportPath is empty.");
            return;
        }

        Room[] rooms = GetComponentsInChildren<Room>(includeInactive: true);
        Dictionary<string, List<LightData>> lightsData = ExportLightsData(rooms.ToList(), out int lights_nb);

        string json = JsonConvert.SerializeObject(lightsData, exportSettings);
        AppManager.SaveJsonToWorldsDataPath(exportPath, json);

        Debug.Log($"LightsDataExporter: exported {lights_nb} lights from {lightsData.Count} rooms to {exportPath}");
    }
}