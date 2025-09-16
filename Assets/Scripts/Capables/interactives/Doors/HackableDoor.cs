using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;


public class HackableDoor : Door, Lockable
{
    [Header("Lockable")]
    [SerializeField] private float locking_interval = 5f; // the time before the door is locked again after being unlocked
    public bool Locked { get; private set; } = true;
    [SerializeField] private Key key; // the key needed to hack this door
    public Key Key => key; // return the key
    public string Password => key.data; // return the key password


    [Header("Hackable")]
    [SerializeField] private int securityLevel = 1;
    public List<Hack> RunningHacks { get; private set; } = new List<Hack>();
    public int SecurityLevel => securityLevel;


    [Header("Components")]
    private HoverCapacity hoverer;

    // TARGETED
    public SpriteRenderer spriteRenderer { get; private set; }
    public Material TargetMaterial { get; private set; }
    public Material DefaultMaterial { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
        DefaultMaterial = spriteRenderer.material;
        TargetMaterial = Resources.Load<Material>("materials/targeted/hack_door");
    }


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
        if (exploit == Exploit.Nmap) { return true; }
        if (exploit.name == "bruteforce") { return true; }
        if (exploit.name == "dictionary_attack") { return true; }
        if (exploit is FileExploit file_exploit && exploit.name == "type_password")
        {
            return key.Matches(file_exploit.file.data);
        }

        return false;
    }
    public void OnHackStarted(Hack hack)
    {
        // Handle the hack start event
        if (debug) { Debug.Log($"(HackableDoor) Hack started on {name} with exploit {hack.name}"); }

        // we show the hacked animation
        anim_player.Play("hacked");

        RunningHacks.Add(hack);
    }
    public void OnHackFailed(Hack hack)
    {
        // Handle the hack failure event
        if (debug) { Debug.Log($"(HackableDoor) Hack failed on {name} with exploit {hack.name}"); }
        anim_player.StopPlaying("hacked");
        Lock();

        RunningHacks.Remove(hack);
    }
    public void OnHackCompleted(Hack hack)
    {
        if (debug) { Debug.Log($"(HackableDoor) Hack completed on {name} with exploit {hack.name}"); }
        anim_player.StopPlaying("hacked");
        RunningHacks.Remove(hack);

        // if the hack was successful we unlock the door
        if (hack.name == "bruteforce" || hack.name == "dictionary_attack")
        {
            Unlock();
        }
        if (hack.program is FileExploit file_exploit && hack.name == "type_password" && key.Matches(file_exploit.file.data))
        {
            Unlock();
        }
    }

    // UNLOCKING
    private async void Unlock()
    {
        Locked = false;
        if (debug) { Debug.Log($"(HackableDoor) {name} is now unlocked"); }

        // we play unlock animation
        anim_player.Play("unlock");

        // we update the hover animation to show a nice unlocked anim
        hoverer.ChangeAnimation("hover");

        // we stop playing idle_locked
        anim_player.StopPlaying("idle_locked");

        // we wait for the unlock animation to stop
        while (anim_player.current_capacity == "unlock") { await System.Threading.Tasks.Task.Yield(); }

        // we open the door
        open();

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
    public bool IsUnlockableVia(Exploit exploit)
    {
        if (exploit == Exploit.Nmap) { return false; }
        return IsVulnerableTo(exploit);
    }
}