using System.Collections.Generic;
using UnityEngine;

public interface Hackable
{

    // unity
    GameObject gameObject { get; }
    Transform transform { get; }
    string name { get => gameObject.name; }

    // interaction properties
    bool CanInteract(Capable capable); // a zombo can open certain door and perso can't

    // hackable properties
    string Key { get; }
    int SecurityLevel { get; }
    // List<string> exploits_vulnerabilities { get; }
    bool IsVulnerableTo(Exploit exploit);
    void OnHackStarted(Hack hack);
    void OnHackCompleted(Hack hack);
}