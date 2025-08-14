using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyberZombo : IA, Hackable
{
    [Header("Processor exploit damage")]
    public int processor_exploit_damage = 20;
    public int processor_exploit_knockback_magnitude = 5;


    [Header("Components")]
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

    [Header("Hackable")]
    public int SecurityLevel => 1;
    public List<Hack> RunningHacks { get; private set; } = new List<Hack>();
    public bool IsVulnerableTo(Exploit exploit)
    {
        return exploit.name == "processor_exploit";
    }

    // BEING HACKED
    public void OnHackStarted(Hack hack)
    {
        if (debug) { Debug.Log($"(CyberZombo) {name} is being hacked by {hack.name}"); }

        RunningHacks.Add(hack);
    }
    public void OnHackCompleted(Hack hack)
    {
        if (debug) { Debug.Log($"(CyberZombo) {name} has been hacked by {hack.name}"); }
        // if the exploit is a processor exploit, we deal damage to the zombie
        if (hack.name == "processor_exploit")
        {
            // we create a knockback force
            Force knockback_force = new Force(
                name: "processor_exploit_knockback",
                direction: new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized,
                magnitude: processor_exploit_knockback_magnitude
            );

            take_damage(processor_exploit_damage, knockback_force);
        }

        if (RunningHacks.Contains(hack)) { RunningHacks.Remove(hack); } // if the zombo is dead we may have already removed the hack
    }
    public void OnHackFailed(Hack hack)
    {
        if (debug) { Debug.Log($"(CyberZombo) {name} failed to hack by {hack.name}"); }
        RunningHacks.Remove(hack);
    }


    // DIE
    public override void Die()
    {
        spriteRenderer.material = DefaultMaterial;
        int hacks = RunningHacks.Count;
        while (RunningHacks.Count > 0)
        {
            Hack hack = RunningHacks[0];
            if (hack.state == HackState.Completed) { RunningHacks.RemoveAt(0); }
            else { hack.Fail(); }
        }

        Debug.Log($"(CyberZombo) {name} has died. {hacks} running hacks were forced to fail.");
    }
}
