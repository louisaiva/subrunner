using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Logger : Singleton<Logger>
{
    [Header("Log parameters")]
    public bool LOG_HACKS = false;
    public bool LOG_CONNECTIONS = false;
    public bool LOG_CORES = false;
}