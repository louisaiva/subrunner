using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyberZombo : IA, Hackable
{

    [Header("Processor exploit damage")]
    public int processor_exploit_damage = 20;
    public int processor_exploit_knockback_magnitude = 5;

    // HACKABLE
    public int SecurityLevel => 1;
    public Vector3 HackPoint => transform.Find("body").localPosition;
    public bool IsVulnerableTo(Exploit exploit)
    {
        return exploit.name == "processor_exploit";
    }

    // BEING HACKED
    public void OnHackStarted(Hack hack)
    {
        if (debug) { Debug.Log($"(CyberZombo) {name} is being hacked by {hack.exploit.name}"); }
    }
    public void OnHackCompleted(Hack hack)
    {
        if (debug) { Debug.Log($"(CyberZombo) {name} has been hacked by {hack.exploit.name}"); }
        // if the exploit is a processor exploit, we deal damage to the zombie
        if (hack.exploit.name == "processor_exploit")
        {
            // we create a knockback force
            Force knockback_force = new Force(
                name: "processor_exploit_knockback",
                direction:new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized,
                magnitude:processor_exploit_knockback_magnitude
            );

            take_damage(processor_exploit_damage, knockback_force);
        }
    }
    public void OnHackFailed(Hack hack)
    {
        if (debug) { Debug.Log($"(CyberZombo) {name} failed to hack by {hack.exploit.name}"); }
    }
}
