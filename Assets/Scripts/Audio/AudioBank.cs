using UnityEngine;
using FMODUnity;
using System;
using System.Collections.Generic;

public class AudioBank : MonoBehaviour
{
    public static AudioBank Instance;
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    [Header("Capacities audio data")]
    public List<CapacityAudioData> capacities_audio;

    [Header("Logs")]
    public bool log_get_event_ref = false;
    public bool log_issues = false;

    public EventReference GetEventReference(string capacity, string skin)
    {
        if (log_get_event_ref) { Debug.Log($"(AudioBank - GetEventReference) asked capacity : '{capacity}' & skin '{skin}'"); }

        CapacityAudioData capacity_data = capacities_audio.Find(s => s.capacity == capacity);
        if (capacity_data == null) { if (log_issues) Debug.LogWarning($"(AudioBank - GetEventReference) Capacity '{capacity}' not found."); return default; }

        SkinAudioData skin_data = capacity_data.GetSkinAudioData(skin);
        if (skin_data == null && (skin_data = capacity_data.GetDefaultSkinAudioData()) != null)
        {
            if (log_issues) Debug.LogWarning($"(AudioBank - GetEventReference) Skin '{skin}' not found for capacity '{capacity}', using '{skin_data.skin}' instead (default skin).");
            return skin_data.event_reference;
        }
        if (skin_data == null) { if (log_issues) Debug.LogWarning($"(AudioBank - GetEventReference) Skin '{skin}' not found for capacity '{capacity}'."); return default; }

        return skin_data.event_reference;
    }
}

[Serializable] public class CapacityAudioData
{
    public string capacity;
    public List<SkinAudioData> skins_audio_data;

    public SkinAudioData GetSkinAudioData(string skin)
    {
        if (skins_audio_data == null) { return null; }
        return skins_audio_data.Find(c => c.skin == skin);
    }
    public SkinAudioData GetDefaultSkinAudioData()
    {
        // litteraly return first one we ve got
        if (skins_audio_data == null) { return null; }
        if (skins_audio_data.Count == 0) { return null; }
        return skins_audio_data[0];
    }
}

[Serializable] public class SkinAudioData
{
    public string skin;
    public EventReference event_reference;
}

