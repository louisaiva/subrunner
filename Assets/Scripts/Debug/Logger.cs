using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Logger : Singleton<Logger>
{
    [Header("Log parameters")]
    public bool LOG_HACKS = false;
    public bool LOG_CONNECTIONS = false;
    public bool LOG_CORES = false;

    [Header("GOAP Target Sensor Logs")]
    public bool LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING = false;
    public bool LOG_WANDER_TARGET_SENSOR = false;
    public bool LOG_HUNGER_SENSOR = false;
    public bool LOG_CLOSEST_FOOD_SENSOR = false;
    public bool LOG_CLOSEST_BEING_SENSOR = false;

    [Header("GOAP Actions Logs")]
    public bool LOG_IA_ACTION = false;
    public bool LOG_ATTACK_ACTION = false;
    public bool LOG_EAT_ACTION = false;
}
