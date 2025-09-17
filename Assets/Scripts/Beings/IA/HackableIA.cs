using System.Collections.Generic;
using UnityEngine;

public class HackableIA : IA, Hackable
{
    
    [Header("CPU Overheat Damage")]
    public int cpu_overheat_damage = 5;
    public int cpu_overheat_knockback = 500;


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


    // HACKABLE
    public bool IsVulnerableTo(Exploit exploit)
    {
        if (exploit == Exploit.Nmap) { return true; }
        if (exploit.name == "cpu_overheat") { return true; }
        if (exploit.name == "cpu_melt") { return true; }
        if (exploit.name == "cyborg_puppet") { return true; }
        return false;
    }
    public void OnHackStarted(Hack hack)
    {
        if (debug) { Debug.Log($"(HackableIA) {name} is being hacked by {hack.name}"); }

        RunningHacks.Add(hack);
    }
    public void OnHackCompleted(Hack hack)
    {
        if (debug) { Debug.Log($"(HackableIA) {name} has been hacked by {hack.name}"); }

        int damage = 0;
        Force knockback = null;

        // if the exploit is a damage exploit, we deal damage to the zombie
        if (hack.name == "cpu_overheat")
        {
            damage = cpu_overheat_damage;
            // we create a knockback force
            knockback = new Force(
                name: "cpu_overheat_knockback",
                direction: new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized,
                magnitude: cpu_overheat_knockback
            );
        }
        else if (hack.name == "cpu_melt")
        {
            // we insta-kill the zombie
            damage = 1000;

            // we create a knockback force
            knockback = new Force(
                name: "cpu_overheat_knockback",
                direction: new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized,
                magnitude: cpu_overheat_knockback
            );
        }
        else if (hack.name == "cyborg_puppet")
        {
            // we move the PersoInputsController to the capable for 30 seconds
            PersoInputsController.Instance.ChangeCapableTarget(this, 30f);
        }

        // take damage if needed
        if (damage > 0) { take_damage(damage, knockback); }

        if (RunningHacks.Contains(hack)) { RunningHacks.Remove(hack); } // if the zombo is dead we may have already removed the hack
    }
    public void OnHackFailed(Hack hack)
    {
        if (debug) { Debug.Log($"(HackableIA) {name} failed to hack by {hack.name}"); }
        RunningHacks.Remove(hack);
    }


    // DIE
    public override void Die()
    {
        base.Die();

        spriteRenderer.material = DefaultMaterial;
        int hacks = RunningHacks.Count;
        while (RunningHacks.Count > 0)
        {
            Hack hack = RunningHacks[0];
            if (hack.state == HackState.Completed) { RunningHacks.RemoveAt(0); }
            else { hack.Fail(); }
        }

        Debug.Log($"(HackableIA) {name} has died. {hacks} running hacks were forced to fail.");
    }
}
