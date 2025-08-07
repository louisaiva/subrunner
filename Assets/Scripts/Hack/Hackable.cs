using System.Collections.Generic;
using UnityEngine;

public interface Hackable
{

    // unity
    GameObject gameObject { get; }
    Transform transform { get; }
    string name { get => gameObject.name; }

    // hackable properties
    bool Locked { get; } // whether the hackable is locked or not
    string Key { get; }
    int SecurityLevel { get; }
    // List<string> exploits_vulnerabilities { get; }
    bool IsVulnerableTo(Exploit exploit);
    void OnHackStarted(Hack hack);
    void OnHackCompleted(Hack hack);
    void OnHackFailed(Hack hack);
}