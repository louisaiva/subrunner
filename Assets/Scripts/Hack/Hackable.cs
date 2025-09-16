using System.Collections.Generic;
using UnityEngine;

public interface Hackable
{

    // unity
    GameObject gameObject { get; }
    SpriteRenderer spriteRenderer { get; }
    Transform transform { get; }
    string name { get => gameObject.name; }

    // VULNERABILITIES
    int SecurityLevel { get; }
    bool IsVulnerableTo(Exploit exploit);

    // TARGETED
    Material TargetMaterial { get; }
    Material DefaultMaterial { get; }

    // BEING HACKED
    // List<ConnectionType> ConnectionTypes { get; } // the types of connections that can be used to connect to this hackable
    List<Hack> RunningHacks { get; }
    void OnHackStarted(Hack hack);
    void OnHackCompleted(Hack hack);
    void OnHackFailed(Hack hack);
}

public interface Lockable : Hackable
{
    // LOCKS
    bool Locked { get; } // whether the hackable is locked or not
    Key Key { get; }
    string Password { get; }
    bool IsUnlockableVia(Exploit exploit);
}