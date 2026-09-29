using System;
using UnityEngine;

public class WorldLogger : MonoBehaviour
{
    public static bool ShowInConsoleAsWell = false;
    public static void Log(string log)
    {
        string world_id = WorldManager.LazyWorld;
        if (string.IsNullOrEmpty(world_id))
        {
            Debug.LogError($"(WorldLogger) Could not send log because world_id is null or empty : {log}");
            return;
        }

        AppManager.AddLogToWorldLogger(world_id, log);
        if (ShowInConsoleAsWell) { Debug.Log("(WorldLogger) : " + log); }
    }

    public float periodic_fast_delay = 2f; // every 2 sec
    private float next_fast_period = 0f;
    public float periodic_slow_delay = 20f; // every 20 sec
    private float next_slow_period = 0f;

    private void Update()
    {
        if (string.IsNullOrEmpty(WorldManager.LazyWorld)) { return; }

        // fast log
        if (next_fast_period > 0f) { next_fast_period -= Time.unscaledDeltaTime; }
        else
        {
            next_fast_period = periodic_fast_delay;
            send_periodic_fast_log();
        }

        // slow log
        if (next_slow_period > 0f) { next_slow_period -= Time.deltaTime; }
        else
        {
            next_slow_period = periodic_slow_delay;
            send_periodic_slow_log();
        }

    }

    private void send_periodic_fast_log()
    {
        // Log(" -- Fast Logs");
        Log($"FPS : {DebugManager.Instance.FPS}");
        Log("Controller : " + Controller.LazyInstance.GetControllerPosition());

        // Log(" -- End Fast Logs");
    }

    private void send_periodic_slow_log()
    {
        Log(" -- ");
        Log(CapableEngine.LazyInstance.GetWorldCapablesInfo());
        Log(" -- ");
    }
}