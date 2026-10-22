using System;
using System.Collections.Generic;
using SingularityGroup.HotReload;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// This class allow us to cycle through multiples animations
/// using an AnimPlayer script
/// </summary>
public class AnimViewer : MonoBehaviour
{

    [Header("Anim Viewer")]
    [SerializeField] private string skin;
    [SerializeField] private CapableOrientation orientation;
    [SerializeField] private List<AnimCapacity> anim_cycle = new List<AnimCapacity>();
    private AnimPlayer player;


    [Header("Logs")]
    [SerializeField] private bool logs = false;
    [SerializeField] private bool extended_logs = false;


    // INIT
    private void OnEnable()
    {
        player = GetComponentInChildren<AnimPlayer>();
        player.OnAnimationDonePlaying += OnAnimationEnded;
        CapableEngine.LoadTemplatesCapablesData();
    }
    private void OnDisable()
    {
        player.OnAnimationDonePlaying -= OnAnimationEnded;
    }
    public void Start()
    {
        RefreshView();
        RefreshCycle();
    }


    // ON VALIDATE
    private bool refresh = false;
    private void OnValidate()
    {
        refresh = true;
        // Debug.Log("(AnimViewer) Refreshing next update...");
    }
    private void Update()
    {
        if (!refresh) { return; }
        RefreshView();
        RefreshCycle();
        refresh = false;
    }


    // MAIN REFRESH VIEW METHOD
    private void RefreshView()
    {
        player.SetOrientationFromString(orientation.ToString());

        // here we get the CapableData from the CapableEngine
        CapableData data = CapableEngine.LazyInstance.GetTemplateData(skin);
        if (data == null)
        {
            Debug.LogWarning($"(AnimViewer) No data found for skin {skin} !!");
            return;
        }
        if (data.anim_data == null)
        {
            Debug.LogWarning($"(AnimViewer) No animdata found for skin {skin} ://");
            return;
        }
        
        // we load the layers + the player
        CapableBank.LazyInstance.LayerBank.LoadAnimData(player, data.anim_data);
        
        // finally we launch the player
        player.Show();
        // Debug.Log($"(AnimViewer) Viewing skin {skin}");
    }


    // MAIN CYCLE METHODS
    private int cycle_index = 0;
    private void RefreshCycle()
    {
        cycle_index = 0;
        OnAnimationEnded("\\\\\\ °° cycle break °° ///");
    }
    private async void OnAnimationEnded(string capacity_just_done)
    {
        // here we need to go to the next anim
        string capacity = get_next_anim();
        if (string.IsNullOrEmpty(capacity)) { capacity = "idle"; }
        if (logs) { Debug.Log("Animation " + capacity_just_done + " is done. Playing next animation : " + capacity); }
        player.StopPlaying(capacity_just_done);
        player.Play(capacity);

        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        
        player.ForceUpdateEachFrame();
    }
    private string get_next_anim()
    {
        if (anim_cycle.Count == 0) { return "idle"; }
        cycle_index++;
        int i = 0;
        if (extended_logs) { Debug.Log("(AnimViewer) CYCLE INDEX IS NOW " + cycle_index); }
        foreach (AnimCapacity ac in anim_cycle)
        {
            if (extended_logs) { Debug.Log("(AnimViewer) checking anim " + ac.capacity + " with " + ac.repetitions); }
            i += ac.repetitions;
            if (cycle_index <= i)
            {
                // we need to play this one !
                if (extended_logs) { Debug.Log("(AnimViewer) it's this one ! " + ac.capacity); }
                return ac.capacity;
            }
        }
        if (extended_logs) { Debug.Log("(AnimViewer) CYCLE INDEX IS NOW 1. we went to the end of the cycle. Playing first anim"); }
        cycle_index = 1;
        return anim_cycle[0].capacity;
    }
}

[Serializable] public class AnimCapacity
{
    public string capacity;
    [SerializeField , Range(1,10)] public int repetitions = 1;
}

public enum CapableOrientation
{
    L,
    LU,
    U,
    RU,
    R,
    RD,
    D,
    LD
}