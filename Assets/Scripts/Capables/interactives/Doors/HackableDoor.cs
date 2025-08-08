using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;


public class HackableDoor : Door, Hackable
{
    [Header("Locking")]
    [SerializeField] private float locking_interval = 5f; // the time before the door is locked again after being unlocked

    [Header("Hackable")]
    public bool Locked { get; private set; } = true;
    [SerializeField] private Key key; // the key needed to hack this door
    public string Key => key.key; // the type of key needed to hack this door
    [SerializeField] private int securityLevel = 1;
    public int SecurityLevel => securityLevel;

    [Header("Components")]
    private HoverCapacity hoverer;


    // START
    protected override void Start()
    {
        base.Start();

        // get the hover capacity
        hoverer = GetCapacity<HoverCapacity>();
        if (hoverer == null) { Debug.LogError($"(HackableDoor) {name} has no HoverCapacity component"); }

        // we play the idle_locked if we are locked
        if (Locked) { anim_player.Play("idle_locked"); }
    }

    // INTERACTION
    public override void OnInteract(Capable interactor)
    {
        if (!Locked) { base.OnInteract(interactor); return; }

        // we are locked, we check if interactor has a laptop with the right key (instant hack)
        if (interactor is Hacker hacker)
        {
            Laptop laptop = hacker.Laptop;
            if (laptop != null && laptop.HasKeyFor(this))
            {
                Unlock();
                base.OnInteract(interactor);
                return;
            }
        }

        // else we are locked we can't interact.
        if (debug) { Debug.Log($"(HackableDoor) {interactor.name} tried to interact with locked door {name}"); }

        // if we have a hover capacity we try to update its animation (so it will reset the animation, which alert the player it's locked)
        hoverer.ChangeAnimation("hover_locked");
    }

    // HACKABLE
    public bool IsVulnerableTo(Exploit exploit)
    {
        string[] exploitType = exploit.name.Split('_');
        if (debug) { Debug.Log($"(HackableDoor) checking if {name} is vulnerable to exploit {exploit.name} ?" + exploitType); }
        if (exploitType.Length < 2) { return false; }
        if (exploitType[1] == key.key_type) { return true; } // if the exploit type matches the key type, we can hack the door
        return false;
    }
    public void OnHackStarted(Hack hack)
    {
        // Handle the hack start event
        if (debug) { Debug.Log($"(HackableDoor) Hack started on {name} with exploit {hack.exploit.name}"); }

        // we show the hacked animation
        anim_player.Play("hacked");
    }
    public void OnHackFailed(Hack hack)
    {
        // Handle the hack failure event
        if (debug) { Debug.Log($"(HackableDoor) Hack failed on {name} with exploit {hack.exploit.name}"); }
        anim_player.StopPlaying("hacked");
        Lock();
    }
    public void OnHackCompleted(Hack hack)
    {
        // Handle the hack completion event
        if (debug) { Debug.Log($"(HackableDoor) Hack completed on {name} with exploit {hack.exploit.name}"); }
        // this.Do("open");

        // we unlock the door & play unlock anim
        anim_player.StopPlaying("hacked");
        Unlock();
    }

    // UNLOCKING
    private void Unlock()
    {
        Locked = false;
        if (debug) { Debug.Log($"(HackableDoor) {name} is now unlocked"); }

        // we play unlock animation
        anim_player.Play("unlock");

        // we update the hover animation to show a nice unlocked anim
        hoverer.ChangeAnimation("hover");

        // we stop playing idle_locked
        anim_player.StopPlaying("idle_locked");

        // we invoke the locking after x seconds
        Invoke(nameof(Lock), locking_interval);
    }
    private void Lock()
    {
        CancelInvoke();
        Locked = true;
        if (debug) { Debug.Log($"(HackableDoor) {name} is now locked"); }

        // we play lock animation
        anim_player.Play("lock");

        // we update the hover animation to show the locked anim
        hoverer.ChangeAnimation("hover_locked");

        // we start to play idle_locked again
        anim_player.AddToPile("idle_locked");

        // if we are open we close ourselves
        if (is_open)
        {
            if (debug) { Debug.Log($"(HackableDoor) {name} is open, closing it now"); }
            close();
        }
    }
}