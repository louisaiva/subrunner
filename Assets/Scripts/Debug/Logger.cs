using System;
using UnityEngine;

public class Logger : MonoBehaviour
{
    private static Logger _instance;
    public static Logger LazyInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<Logger>();
                if (_instance == null)
                {
                    Debug.LogError("(Logger) No Logger instance found in the scene. Please add a Logger component to a GameObject in the scene.");
                }
            }
            return _instance;
        }
    }

    [Header("Log parameters")]
    public bool LOG_HACKS = false;
    public bool LOG_CONNECTIONS = false;
    public bool LOG_CORES = false;

    [Header("GOAP Target Sensor Logs")]
    public bool LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING = false;
    public bool LOG_WANDER_TARGET_SENSOR = false;
    public bool LOG_HUNGER_SENSOR = false;
    public bool LOG_CLOSEST_FOOD_SENSOR = false;
    public bool LOG_CLOSEST_TRASH_SENSOR = false;
    public bool LOG_CLOSEST_BEING_SENSOR = false;
    public bool LOG_GTR_SENSOR = false;

    [Header("GOAP Actions Logs")]
    public bool LOG_IA_ACTION = false;
    public bool LOG_ATTACK_ACTION = false;
    public bool LOG_EAT_ACTION = false;
    public bool LOG_INTERACT_ACTION = false;
    public bool LOG_GTR_ACTION = false;


    public static void Error(string message, Verbosity verbose)
    {
        if (verbose < Verbosity.ErrorsOnly) { return; }
        Debug.LogError(message);
    }
    public static void Warning(string message, Verbosity verbose)
    {
        if (verbose < Verbosity.WarningAndErrors) { return; }
        Debug.LogWarning(message);
    }
    public static void Log(string message, Verbosity verbose)
    {
        if (verbose < Verbosity.Normal) { return; }
        Debug.Log(message);
    }
    public static void LogExtended(string message, Verbosity verbose)
    {
        if (verbose < Verbosity.Extended) { return; }
        Debug.Log(message);
    }

    // specific
    public static void LogSpecific(string message, Verbosity verbose)
    {
        if (verbose < Verbosity.Specific) { return; }
        Debug.Log(message);
    }
    public static void LogVerySpecific(string message, Verbosity verbose)
    {
        if (verbose < Verbosity.VerySpecific) { return; }
        Debug.Log(message);
    }
    public static void LogOMGThatsVeryVerySpecific(string message, Verbosity verbose)
    {
        if (verbose < Verbosity.OMG_ThatsVeryVerySpecific) { return; }
        Debug.Log(message);
    }
}

[Serializable] public class Loggable<T> where T : MonoBehaviour
{
    public Loggable()
    {
        source_type = "(" + typeof(T).Name + ") ";
    }

    private string source_type;
    public Verbosity Verbose = Verbosity.Normal;
    public void Error(string message) { Logger.Error(source_type + message, Verbose); }
    public void Error(bool condition, string message) { if (!condition) { return; } Logger.Error(source_type + message, Verbose); }
    public void Warning(string message) { Logger.Warning(source_type + message, Verbose); }
    public void Warning(bool condition, string message) { if (!condition) { return; } Logger.Warning(source_type + message, Verbose); }
    public void Log(string message) { Logger.Log(source_type + message, Verbose); }
    public void Log(bool condition, string message) { if (!condition) { return; } Logger.Log(source_type + message, Verbose); }
    
    // extended
    public void LogExtended(string message) { Logger.LogExtended(source_type + message, Verbose); }
    public void LogExtended(bool condition, string message) { if (!condition) { return; } Logger.LogExtended(source_type + message, Verbose); }

    // specific
    public void LogSpecific(string message) { Logger.LogSpecific(source_type + message, Verbose); }
    public void LogSpecific(bool condition, string message) { if (!condition) { return; } Logger.LogSpecific(source_type + message, Verbose); }
    public void LogVerySpecific(string message) { Logger.LogVerySpecific(source_type + message, Verbose); }
    public void LogVerySpecific(bool condition, string message) { if (!condition) { return; } Logger.LogVerySpecific(source_type + message, Verbose); }
    public void LogOMGThatsVeryVerySpecific(string message) { Logger.LogOMGThatsVeryVerySpecific(source_type + message, Verbose); }
    public void LogOMGThatsVeryVerySpecific(bool condition, string message) { if (!condition) { return; } Logger.LogOMGThatsVeryVerySpecific(source_type + message, Verbose); }
}

public enum Verbosity
{
    NoLogs = 0,
    ErrorsOnly = 1,
    WarningAndErrors = 2,
    Normal = 3,
    Extended = 4, // includes all the above as well
    Specific = 5, // includes all the above as well
    VerySpecific = 6,
    OMG_ThatsVeryVerySpecific = 7
}