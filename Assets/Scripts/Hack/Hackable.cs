using System.Collections.Generic;
using UnityEngine;

public interface Hackable
{

    // unity
    GameObject gameObject { get; }
    Transform transform { get; }
    string name { get => gameObject.name; }

    // VULNERABILITIES
    int SecurityLevel { get; }
    bool IsVulnerableTo(Exploit exploit);

    // BEING HACKED
    void OnHackStarted(Hack hack);
    void OnHackCompleted(Hack hack);
    void OnHackFailed(Hack hack);
    Vector3 HackPoint { get; } // the local position offset for the hackray to point to
}

public interface Lockable : Hackable
{
    // LOCKS
    bool Locked { get; } // whether the hackable is locked or not
    string Key { get; }
}