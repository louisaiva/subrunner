using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// HackCapacity is a capacity that allows a being to hack things
/// in their range. need to have a laptop for this to work
/// </summary>

public class HackCapacity : Capacity
{
    [Header("Target Selection")]
    public Hackable hovered_target;

    [Header("Hacks")]
    public List<Hack> running_hacks = new List<Hack>();

    [Header("Exploits")]
    public List<Exploit> exploits = new List<Exploit>();

    [Header("Components")]
    [SerializeField] private Laptop laptop;
    [SerializeField] private Being being;
    [SerializeField] private AnimPlayer anim_player;
    [SerializeField] private float hacking_animation_duration = 2f; // duration of the hacking animation
    [SerializeField] private CircleCollider2D hack_collider;


    // START
    private void Start()
    {
        laptop = capable.GetComponent<Laptop>();
        hack_collider = laptop.GetCapacity<InteractHackCapacity>()?.GetComponent<CircleCollider2D>();
    }

    // USE
    public override async void Use(Capable capable)
    {
        // checks if we have a hovered target
        if (hovered_target == null)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack but no target is hovered."); }
            return;
        }


        // we set the bearer and its components
        if (capable is Being) { being = capable as Being; }/* 
        else if (capable is Laptop laptop && laptop.Holder is Being holder)
        {
            being = holder;
        } */
        else
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack but we have no being that can play animation"); }
            return;
        }

        // check if we can hack this hovered target
        Hack hack = CanHack(hovered_target);
        if (hack == null) { return; }

        // we start the cooldown for the time of the animation
        startCooldown(hacking_animation_duration);

        // we play the animation
        anim_player = capable.GetComponent<AnimPlayer>();
        await play_animation();
        /* Anim anim = anim_player.Play("hack");
        if (anim == null)
        {
            // we remove the animation from the pile
            anim_player.StopPlaying("hack", true);
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} tried to hack but the animation can't be played right now."); }
            return;
        } */

        // we run the exploit
        RunExploit(hack);
    }

    // HACKING
    public Hack CanHack(Hackable target)
    {
        // we check if the target is in range
        if (Vector3.Distance(transform.position, target.transform.position) > hack_collider.radius)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {being.name} tried to hack {target.name} but it is out of range."); }
            return null;
        }

        // check if we have at least 1 core
        if (laptop.HasFreeCores() == false)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {being.name} tried to hack {target.name} but has no free cores."); }
            return null;
        }

        // we check if we already have the key for this target (instant hack)
        if (laptop.HasKeyFor(target))
        {
            if (debug) { Debug.Log($"(HackCapacity) {being.name} instantly hacked {target.name}."); }
            return new Hack(target, exploits[0]/* new Exploit("admin_connection", 1000, 0.1f, 1) */);
        }

        // we check if we have the right exploit for this target
        foreach (Exploit exploit in exploits)
        {
            if (target.IsVulnerableTo(exploit) && laptop.HasFreeCores(exploit.cores_cost))
            {
                if (debug) { Debug.Log($"(HackCapacity) {being.name} can hack {target.name} with exploit {exploit.name}."); }
                return new Hack(target, exploit);
            }
        }

        // we have no hack we can't hack it
        if (debug) { Debug.Log($"(HackCapacity) {being.name} tried to hack {target.name} but no vulnerability was found."); }
        return null;
    }
    public void RunExploit(Hack hack)
    {
        // we run the hack
        float duration = hack.CalculateDuration();
        hack.Run(duration);

        // we occupy some cores for the hack duration
        laptop.UseCores(hack.exploit.cores_cost, duration);

        // we add the hack to the running hacks
        running_hacks.Add(hack);
        hack.target.OnHackStarted(hack);
    }
    protected override void Update()
    {
        base.Update();

        // we cycle through all the running exploits and we check few things
        for (int i = running_hacks.Count - 1; i >= 0; --i)
        {
            Hack hack = running_hacks[i];

            // checks if we are too far away from the target
            if (Vector3.Distance(transform.position, hack.target.transform.position) > hack_collider.radius)
            {
                if (debug) { Debug.LogWarning($"(HackCapacity) {being.name} is too far away from {hack.target.name} to continue the hack."); }
                // we remove the hack from the running hacks
                hack.Quit();
                running_hacks.RemoveAt(i);
                continue;
            }

            // we check if the hack is done
            if (hack.state == HackState.Completed || hack.state == HackState.Failed)
            {
                if (debug) { Debug.Log($"(HackCapacity) {being.name} finished hacking {hack.target.name} with exploit {hack.exploit.name}."); }
                // we remove the hack from the running hacks
                running_hacks.RemoveAt(i);
                continue;
            }

            // we update the progress of the hack
            hack.progress += Time.deltaTime / hack.duration * 100f;
        }
    }


    // TARGET MANAGEMENT
    public void Select(Hackable target)
    {
        // we check if the target is null
        if (target == null)
        {
            hovered_target = null;
            return;
        }

        // we check if the target is in range
        if (Vector3.Distance(transform.position, target.transform.position) > hack_collider.radius)
        {
            if (debug) { Debug.LogWarning($"(HackCapacity) {capable.name} is hovering {target.name} but it is out of range."); }
        }

        // we set the hovered target
        hovered_target = target;
    }
    public void Deselect()
    {
        // we reset the hovered target
        if (debug) { Debug.Log($"(HackCapacity) {capable.name} stopped hovering {hovered_target.name}."); }
        hovered_target = null;
    }

    // STOP ANIMATION
    private async Awaitable play_animation()
    {
        anim_player.Play("hack");
        await Task.Delay((int)(hacking_animation_duration * 1000));
        anim_player.StopPlaying("hack");
    }
}




[System.Serializable]
public class Hack
{
    [Header("Hack Details")]
    public float progress;
    public Hackable target;
    public Exploit exploit;
    public float duration;
    public HackState state = HackState.NotStarted;

    // CONSTRUCTOR
    public Hack(Hackable target, Exploit exploit)
    {
        this.target = target;
        this.exploit = exploit;
        this.progress = 0f;
    }

    // RUNNING / QUITTING / COMPLETING
    public async void Run(float duration)
    {
        this.duration = duration;
        // Here you would implement the logic to start the hack, e.g., starting a coroutine or a timer
        // For now, we just log the hack start
        this.progress = 0f;
        state = HackState.Running;
        Debug.Log($"Starting hack on {target.name} with exploit {exploit.name}");

        // Simulate the hacking process
        await Task.Delay((int)(duration * 1000));
        Complete();
    }
    public void Quit()
    {
        // Here you would implement the logic to stop the hack, e.g., stopping a coroutine or a timer
        // For now, we just log the hack quit
        Debug.Log($"Hack on {target.name} with exploit {exploit.name} was quit.");
        this.progress = 0f;
        this.state = HackState.Failed;
    }
    public void Complete()
    {
        // Here you would implement the logic to complete the hack, e.g., giving rewards or unlocking features
        // For now, we just log the hack completion
        Debug.Log($"Hack on {target.name} with exploit {exploit.name} completed successfully.");
        this.progress = 100f;
        this.state = HackState.Completed;

        // Notify the target that the hack is completed
        target.OnHackCompleted(this);
    }

    // GETTERS
    public float CalculateDuration(float duration_multiplier = 2.25f)
    {
        float duration = exploit.base_duration;
        int security_level_difference = target.SecurityLevel - exploit.security_level;

        // checks if the difference is > 100
        if (security_level_difference > 100) { return 1000f; }
        else if (security_level_difference < -1000) { return 0.1f; }

        // multiply the duration by multiplier once for eache security level difference
        if (security_level_difference > 0) { duration_multiplier = 1 / duration_multiplier; }
        for (int i = 0; i < security_level_difference; ++i)
        {
            duration *= duration_multiplier;
        }
        return duration;
    }
}

public enum HackState
{
    NotStarted,
    Running,
    Completed,
    Failed
}

[System.Serializable]
public class Exploit
{
    [Header("Exploit Details")]
    public string name;
    public int security_level;
    public float base_duration;
    public int cores_cost;

    // CONSTRUCTOR
    public Exploit(string name, int security_level, float base_duration, int cores_cost)
    {
        this.name = name;
        this.security_level = security_level;
        this.base_duration = base_duration;
        this.cores_cost = cores_cost;
    }
    public Exploit(Exploit exploit, float base_duration = default)
    {
        this.name = exploit.name;
        this.security_level = exploit.security_level;
        this.base_duration = base_duration == default ? exploit.base_duration : base_duration;
        this.cores_cost = exploit.cores_cost;
    }
}