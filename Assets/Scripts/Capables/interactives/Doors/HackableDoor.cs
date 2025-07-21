using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;


public class HackableDoor : Door, Hackable
{
    [Header("Hackable")]
    public bool Locked = true;
    // public bool IsLocked => locked; // whether the door is locked or not
    [SerializeField] private Key key; // the key needed to hack this door
    public string Key => key.key; // the type of key needed to hack this door
    [SerializeField] private int securityLevel = 1;
    public int SecurityLevel => securityLevel;

    // HACKABLE
    public bool IsVulnerableTo(Exploit exploit)
    {
        return exploit.name.Contains(key.key_type);
    }
    public void OnHackStarted(Hack hack)
    {
        // Handle the hack start event
        if (debug) { Debug.Log($"(HackableDoor) Hack started on {name} with exploit {hack.exploit.name}"); }
    }
    public void OnHackCompleted(Hack hack)
    {
        // Handle the hack completion event
        if (debug) { Debug.Log($"(HackableDoor) Hack completed on {name} with exploit {hack.exploit.name}"); }
        this.Do("open");
    }
    public bool CanInteract(Capable capable)
    {
        if (capable is not Perso perso) { return true; } // if the capable is not a perso, we allow interaction (e.g. zombies can open doors)
        if (!Locked) { return true; } // if the door is not locked, we allow interaction

        // checks if perso has the laptop
        Laptop laptop = perso.Laptop;
        if (laptop == null) { return false; } // if the perso doesn't have a laptop, it can't hack the door
        if (!laptop.HasKeyFor(this)) { return false; } // if the laptop doesn't have the key, it can't hack the door
        
        // we already have the key so we can open the door !
        return true;
    }
}