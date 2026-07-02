using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightsEngine : MonoBehaviour
{
    [Header("Cache")]
    private Dictionary<string, Dictionary<Vector2, Light2D>> room_lights = new Dictionary<string, Dictionary<Vector2, Light2D>>();
    
    [Header("Prefab & Parent")]
    [SerializeField] private Light2D light_prefab;
    public Transform LightsParent;

    [Header("Logs")]
    [SerializeField] private Loggable<LightsEngine> log;


    ///
    //
    /// LOAD LIGHTS
    //
    ///


    public void CreateLights(List<LightData> lights_data, string room_id)
    {
        if (lights_data == null) { return; }
        foreach (var light_data in lights_data) { LoadLight(light_data, room_id); }
    }
    public void LoadLights_AIO(List<LightData> lights_data, string room_id, Transform parent = null)
    {
        if (lights_data == null) { return; }
        foreach (var light_data in lights_data) { LoadLight(light_data, room_id, parent, add_to_cache: false); }
    }
    public Light2D LoadLight(LightData data, string room_id, Transform parent = null, bool add_to_cache = true)
    {
        if (!room_lights.ContainsKey(room_id)) { room_lights[room_id] = new Dictionary<Vector2, Light2D>(); }
        if (room_lights[room_id].TryGetValue(data.position, out Light2D existing_light)) { return existing_light; }

        Light2D new_light = Instantiate(light_prefab, parent ?? LightsParent);
        new_light.transform.position = data.position;
        new_light.color = data.color;
        new_light.intensity = data.intensity;
        new_light.pointLightInnerRadius = data.radius.x;
        new_light.pointLightOuterRadius = data.radius.y;
        new_light.falloffIntensity = data.falloff;
        new_light.enabled = false; // we disable it by default, it will be enabled when the chunk will be shown
        if (add_to_cache) { room_lights[room_id][data.position] = new_light; }
        log.LogExtended($"Light loaded at {data.position} in room {room_id}");
        return new_light;
    }

    ///
    //
    /// SHOW / HIDE LIGHTS
    //
    ///

    public void ShowLights(RoomData room)
    {
        // gather all chunks data
        // List<ChunkData> chunks_data = ChunkEngine.Instance.GetChunksDataFromIDs(chunks);

        // we try to get all lights
        List<Light2D> lights_to_show = new List<Light2D>();
        Dictionary<Vector2, Light2D> lights_in_room = room_lights.ContainsKey(room.id) ? room_lights[room.id] : null;
        if (lights_in_room == null) { return; }
        log.LogExtended($"Showing {lights_in_room.Count} lights in room {room.id}");
        lights_to_show.AddRange(lights_in_room.Values);

        // we show them
        foreach (var light in lights_to_show) { if (light != null) { light.enabled = true; } }
    }
    public void HideLights(RoomData room)
    {
        // gather all chunks data
        // List<ChunkData> chunks_data = ChunkEngine.Instance.GetChunksDataFromIDs(chunks);

        // we try to get all lights
        List<Light2D> lights_to_hide = new List<Light2D>();
        Dictionary<Vector2, Light2D> lights_in_room = room_lights.ContainsKey(room.id) ? room_lights[room.id] : null;
        if (lights_in_room == null) { return; }
        log.LogExtended($"Hiding {lights_in_room.Count} lights in chunk {room.id}");
        lights_to_hide.AddRange(lights_in_room.Values);

        // we hide them
        foreach (var light in lights_to_hide) { if (light != null) { light.enabled = false; } }
    }


    ///
    //
    /// CLEAR LIGHTS
    //
    ///

    public void ClearLights(bool log)
    {
        for (int i = 0; i < room_lights.Count; i++)
        {
            Dictionary<Vector2, Light2D> lights = room_lights.ElementAt(i).Value;
            foreach (var light in lights) { if (light.Value != null) { Destroy(light.Value.gameObject); } }
        }
        room_lights.Clear();
        if (log) { Debug.Log($"(LightsEngine) Lights cleared"); }
    }

}