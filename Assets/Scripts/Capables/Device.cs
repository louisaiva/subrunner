using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface Device
{
    public ProcessCapacity Processor { get; }
    public ConnectCapacity Connector { get; }
    public HackCapacity Hacker { get; } // this is the os actually ...

    // MODULES MANAGEMENT
    public void OnNetworkModuleChanged();
    public void OnHDD_Changed();
    public event System.Action<List<StoreCapacity>> OnDisksChanged;

    // FILES MANAGEMENT
    public bool WriteFile(File file);
    public List<StoreCapacity> GetDisks();
    public List<Exploit> GetExploits();

    // KEYS MANAGEMENT
    public bool HasKeyFor(Lockable target);
    public Key GetKeyFor(Lockable target);
}