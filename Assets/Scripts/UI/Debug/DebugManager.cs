using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// this class manages all the debug elements by counting stuff
/// on awake, and nothing else for now !
/// </summary>
public class DebugManager : Singleton<DebugManager>
{
    [Header("Debug")]
    [SerializeField] private Transform debug;
    private Transform debugs_group; // parent of all debugs except FPS
    private List<SingleDebugger> debugs = new List<SingleDebugger>();
    private Dictionary<Type, Debuggable> debuggables = new Dictionary<Type, Debuggable>();

    [Header("FPS Debug")]
    [SerializeField] private TextMeshProUGUI fps;
    [SerializeField] private float smoothed_fps = 0f; // smoothed fps for the debug text
    private List<float> fps_samples = new List<float>(); // list of fps samples for smoothing
    [SerializeField] private int fps_sample_size = 60;
    [SerializeField] private string precision = "F0"; // precision of the fps text
    [SerializeField] private bool show_unscaled_fps = false; // if we want to show the unscaled fps or not (djizzi)


    [Header("Logs")]
    [SerializeField] private bool log = false; // if we want to log warnings when
    [SerializeField] private bool log_debuggables_added = false;

    // START
    protected override void Awake()
    {
        base.Awake();

        // find all debuggables in our children
        Debuggable[] debuggables_array = GetComponentsInChildren<Debuggable>(includeInactive: true);
        foreach (Debuggable d in debuggables_array)
        {
            Type type = d.GetType();
            if (!debuggables.ContainsKey(type))
            {
                debuggables.Add(type, d);
            }
        }

        // find all debugs in ui debug children
        debugs_group = debug.Find("group");
        debugs = debug.GetComponentsInChildren<SingleDebugger>(includeInactive: true).ToList();
        debugs = debugs.Where(d => d.gameObject.activeSelf).ToList(); // we get only active debugs
        foreach (SingleDebugger d in debugs)
        {
            d.gameObject.SetActive(false); // disable all debugs at start
        }
    }
    protected void Start()
    {
        SettingsManager.Instance.RegisterCallback("debug", on_debug_setting_changed);
    }
    void OnDestroy()
    {
        SettingsManager.Instance?.UnregisterCallback("debug", on_debug_setting_changed);
    }

    // UPDATE + FPS
    private void Update() { update_fps(); }
    private void update_fps()
    {
        if (!fps) { return; }

        // showing unscaled_fps if activated
        if (show_unscaled_fps)
        {
            fps.text = (1f / Time.unscaledDeltaTime).ToString(precision) + " FPS";
            return;
        }

        // else we smooth the fps
        if (fps_samples.Count >= fps_sample_size)
        {
            fps_samples.RemoveAt(0); // remove the oldest sample
        }
        fps_samples.Add(1f / Time.unscaledDeltaTime);

        // calculate the smoothed fps
        smoothed_fps = fps_samples.Average();

        // update the text
        fps.text = smoothed_fps.ToString(precision) + " FPS";
    }

    // TOGGLE DEBUG
    private void on_debug_setting_changed(Setting setting)
    {
        switch (setting.Value)
        {
            case 0f:
                debug.gameObject.SetActive(false);
                break;
            case 1f: // only fps
                debug.gameObject.SetActive(true);
                debugs_group.gameObject.SetActive(false);
                break;
            case 2f: // all debugs
                debug.gameObject.SetActive(true);
                debugs_group.gameObject.SetActive(true);
                break;
        }
    }

    // ADD DEBUGGABLE TO DEBUGGER
    public void AddDebuggable(Debuggable debuggable, string name)
    {
        // find the debug with the right name
        SingleDebugger debug = debugs.Find(d => d.name == name);
        if (debug == null)
        {
            if (log) { Debug.LogWarning("(DebugManager) No debug found with name " + name); }
            return;
        }

        // we add the debuggable to the dictionary if not already there
        Type type = debuggable.GetType();
        if (!debuggables.ContainsKey(type)) { debuggables.Add(type, debuggable); }

        // add the debuggable to the debug
        debug.SetDebuggable(debuggable);
        debug.gameObject.SetActive(true); // enable the debug

        if (log_debuggables_added) { Debug.Log($"(DebugManager) Added debuggable of type {type} to debug {name}"); }
    }

    // GETTERS
    public T GetDebuggable<T>() where T : Debuggable
    {
        Type type = typeof(T);
        if (debuggables.ContainsKey(type))
        {
            if (debuggables[type] is T debuggable) { return debuggable; }
        }
        if (log) { Debug.LogWarning("(DebugManager) No debuggable found with type " + type); }
        return default(T);
    }
}