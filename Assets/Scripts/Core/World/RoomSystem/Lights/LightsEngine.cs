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


    ///
    //
    /// LOAD LIGHTS
    //
    ///


    public void LoadLights(List<LightData> lights_data, string room_id, Transform parent = null)
    {
        if (lights_data == null) { return; }
        foreach (var light_data in lights_data) { LoadLight(light_data, room_id, parent); }
    }
    public Light2D LoadLight(LightData data, string room_id, Transform parent = null)
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
        room_lights[room_id][data.position] = new_light;
        return new_light;
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