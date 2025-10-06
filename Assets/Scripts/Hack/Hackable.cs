using System.Collections.Generic;
using UnityEngine;

public interface Lockable
{
    // KEYS
    bool Locked { get; } // whether the hackable is locked or not
    Key Key { get; }
    string Password { get; }


    // UNLOCK / LOCK
    void Unlock();
    void Lock();
    // bool IsUnlockableVia(Exploit exploit);
}