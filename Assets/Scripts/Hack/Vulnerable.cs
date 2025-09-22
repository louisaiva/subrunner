using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// a Vulnerable is something that can be hacked
/// replacement of Hackable kind of
/// it has vulnerabilities and multiple methods that we can adjust in the
/// inspector through unity events for building cool vulnerabilities
/// should be placed on the connect capacity
/// </summary>
public class Vulnerable : MonoBehaviour
{

    [Header("VULNERABILITIES")]
    [SerializeField] private List<Vulnerability> vulnerabilities = new List<Vulnerability>();
    public int SecurityLevel = 1;

    [Header("RUNNING HACKS")]
    [SerializeField] private List<Hack> running_hacks = new List<Hack>();


    [Header("Components")]
    [HideInInspector] public Capable capable = null;
    [HideInInspector] public ConnectCapacity Connector = null;
    [HideInInspector] public SpriteRenderer Renderer = null;
    [HideInInspector] public Material TargetMaterial;
    [HideInInspector] public Material DefaultMaterial;


    [Header("Log")]
    [SerializeField] private bool log = false;

    // AWAKE
    private void Awake()
    {
        capable = transform.parent.GetComponent<Capable>();
        if (capable == null) { Debug.LogError($"(Vulnerable) {name} has no capable parent!"); }
        Renderer = capable.GetComponent<SpriteRenderer>();
        Connector = GetComponent<ConnectCapacity>();

        DefaultMaterial = Renderer.material;
        TargetMaterial = Resources.Load<Material>("materials/targeted/hack_door");
    }

    // VULNERABILITIES
    public bool IsVulnerableTo(Exploit exploit)
    {
        if (exploit == Exploit.Nmap) { return true; }
        if (capable is Lockable lockable)
        {
            if (exploit is FileExploit file_exploit) { return lockable.Key.Matches(file_exploit.file.data); }
            if (exploit == Exploit.TypePassword) { return false; } // type password vide c nul on veut pas
        }

        return vulnerabilities.Any(v => v.exploit_name == exploit.name);
    }
    public Exploit GetHighestVulnerability(List<Exploit> exploits)
    {
        List<Exploit> effective_exploits = new List<Exploit>();
        foreach (Exploit exploit in exploits)
        {
            if (IsVulnerableTo(exploit)) { effective_exploits.Add(exploit); }
        }
        if (effective_exploits.Count == 0) { return null; }

        effective_exploits = effective_exploits.OrderByDescending(
            e => vulnerabilities.Find(v => v.exploit_name == e.name)?.selection_priority ?? 0
        ).ToList();

        return effective_exploits.FirstOrDefault();
    }

    // BEING HACKED
    public void OnHackStarted(Hack hack)
    {
        if (log) { Debug.Log($"(Vulnerable) {name} is being hacked by {hack.name}"); }
        running_hacks.Add(hack);

        // we trigger the vulnerability events
        vulnerabilities.Find(v => v.exploit_name == hack.program?.name)?.on_hack_started?.Invoke(hack);
    }
    public void OnHackSucceeded(Hack hack)
    {
        if (log) { Debug.Log($"(Vulnerable) {name} has been hacked by {hack.name}"); }

        // we trigger the vulnerability events
        vulnerabilities.Find(v => v.exploit_name == hack.program?.name)?.on_hack_succeeded?.Invoke(hack);
    }
    public void OnHackFailed(Hack hack)
    {
        if (log) { Debug.Log($"(Vulnerable) {name} hack by {hack.name} has failed"); }

        // we trigger the vulnerability events
        vulnerabilities.Find(v => v.exploit_name == hack.program?.name)?.on_hack_failed?.Invoke(hack);
    }
    public void OnHackDone(Hack hack)
    {
        if (log) { Debug.Log($"(Vulnerable) {name} hack by {hack.name} is done"); }
        running_hacks.Remove(hack);

        // we trigger the vulnerability events
        vulnerabilities.Find(v => v.exploit_name == hack.program?.name)?.on_hack_done?.Invoke(hack);
    }

    // OnDISABLE
    private void OnDisable()
    {
        Renderer.material = DefaultMaterial;
        int hacks = running_hacks.Count;
        while (running_hacks.Count > 0)
        {
            Hack hack = running_hacks[0];
            if (hack.state == ProcessusState.Completed) { running_hacks.RemoveAt(0); }
            else { hack.Fail(); }
        }
    }




    // USEFUL EXPLOIT EFFECT RELATED METHOD

    // CONTROL & DAMAGE
    public void DamageCapable(Hack hack)
    {
        float damage = 0f;
        float knockback_magnitude = 0f;
        if (hack.program is DamageExploit damager)
        {
            damage = damager.damage;
            knockback_magnitude = damager.knockback_magnitude;
        }


        // todo temporary solution waiting for a better Exploit management (maybe with SO ??)
        if (hack.program.name == "cpu_overheat") { damage = 10f; knockback_magnitude = 5f; }
        else if (hack.program.name == "cpu_melt") { damage = 1000f; knockback_magnitude = 5f; }

        // we create a knockback force
        Force knockback = new Force(
            name: "cpu_overheat_knockback",
            direction: new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized,
            magnitude: knockback_magnitude
        );

        if (capable is Being being) { being.take_damage(damage, knockback); }
    }
    public void ControlCapable(Hack hack)
    {
        // we move the PersoInputsController to the capable for duration seconds
        float duration = -888f;
        if (!hack.exploit.wait_end) { duration = hack.exploit.end_timer; }
        Controller.Instance.ChangeCapableTarget(capable, duration);
    }
    public void UncontrolController(Hack hack)
    {
        Controller.Instance.BreakCapableTarget(capable);
    }

    // UNLOCKING
    public void TypePassword(Hack hack)
    {
        if (capable is not Lockable lockable) { return; }
        if (hack.program is not FileExploit file_exploit) { return; }
        bool unlocked = lockable.Key.Matches(file_exploit.file.data);

        // unlock / lock doors depending on if the passwords are the same
        if (unlocked) { lockable.Unlock(); }
        else { lockable.Lock(); }
    }
    public void BruteforcePassword(Hack hack)
    {
        if (capable is not Lockable lockable) { return; }

        // the hack found the password
        hack.Download(lockable.Key);

        // unlock the door
        lockable.Unlock();
    }
}


[Serializable] public class Vulnerability
{
    // links an exploit and effects
    public string exploit_name;
    public int selection_priority = 1;
    public UnityEvent<Hack> on_hack_started;
    public UnityEvent<Hack> on_hack_succeeded;
    public UnityEvent<Hack> on_hack_failed;
    public UnityEvent<Hack> on_hack_done;
}