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
    void OnHackStarted(Hack hack);
    void OnHackCompleted(Hack hack);
    void OnHackFailed(Hack hack);
    // Vector3 HackPoint { get; } // the local position offset for the hackray to point to
    // HackableSlot HackableSlot { get; }
}

public interface Lockable : Hackable
{
    // LOCKS
    bool Locked { get; } // whether the hackable is locked or not
    string Key { get; }
}